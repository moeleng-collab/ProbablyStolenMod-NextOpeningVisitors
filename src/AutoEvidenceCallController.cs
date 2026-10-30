using System;
using HarmonyLib;
using UnityEngine;

namespace NextOpeningVisitors
{
    [HarmonyPatch(typeof(StorePhoneClient), nameof(StorePhoneClient.GetCallDialog))]
    internal static class AutoEvidenceCallDialogPatch
    {
        internal static Dialogue? InjectedDialog;

        private static bool Prefix(StorePhoneClient __instance, ref Dialogue? __result)
        {
            if (InjectedDialog == null || __instance == null ||
                __instance != StorePhoneClient.GetWildPhoneClient()) return true;
            __result = InjectedDialog;
            return false;
        }
    }

    [HarmonyPatch(typeof(PhoneUIManager), nameof(PhoneUIManager.OpenUI))]
    internal static class AutoEvidencePhoneOpenPatch
    {
        private static void Postfix() => AutoEvidenceCallController.OnPhoneOpened();
    }

    internal static class AutoEvidenceCallController
    {
        internal const string UpgradeId = "NEXT_OPENING_VISITORS_AUTO_EVIDENCE";
        internal const string SaveKey = "NextOpeningVisitors.AutoEvidence.State";
        private const string Source = "nextopeningvisitors";

        private static AutoEvidenceCallState? _state;
        private static AutoEvidenceCallHost? _host;
        private static bool _armed;
        private static bool _acceptInProgress;
        private static int _presentedOfferDay = -1;

        internal static void OnGameLoaded()
        {
            EnsureRegistered();
            LoadState();
            _presentedOfferDay = -1;
            EnsureHost();
            ArmIfPending();
        }

        internal static void OnNewGame()
        {
            EnsureRegistered();
            LoadState();
            _presentedOfferDay = -1;
            _armed = false;
            EnsureHost();
        }

        internal static void OnWakingUp()
        {
            EnsureRegistered();
            LoadState();
            PlayerStore? store = PlayerStore.Instance;
            if (store?.secData == null || StoreStation.Instance == null) return;
            NetworkUpgrade? service = GetRuntimeService();
            if (service == null) return;

            int day = StoreStation.Instance.dayCounter;
            bool created = _state!.BeginDay(day, store.secData.evidenceLevel, service.cost);
            SaveState();
            if (created)
            {
                NextOpeningVisitorsMod.Log?.Log(
                    $"Automatic evidence cleanup offer created: day={day}, price={_state.OfferPrice}.");
            }
            _presentedOfferDay = -1;
            EnsureHost();
            ArmIfPending();
        }

        internal static void Shutdown()
        {
            _armed = false;
            _acceptInProgress = false;
            _state = null;
            AutoEvidenceCallDialogPatch.InjectedDialog = null;
            if (_host != null) UnityEngine.Object.Destroy(_host.gameObject);
            _host = null;
            NetworkUpgrade.RemoveModUpgrades(Source);
        }

        private static void EnsureRegistered()
        {
            var definition = new NetworkUpgrade(UpgradeId)
            {
                cost = AutoEvidenceCallState.InitialPrice,
                state = 2,
                isRepeatable = true,
                cooldownCurrent = 0,
                cooldownDuration = 0,
                source = Source,
                iconKey = NetworkUpgrade.EVIDENCE_REMOVAL,
                titleProvider = () => CrimeForecastLocalization.Get(
                    CrimeForecastLocalization.ServiceTitle, LocHelper.GetCurrentLocaleCode()),
                descriptionProvider = () => CrimeForecastLocalization.Get(
                    CrimeForecastLocalization.ServiceDescription, LocHelper.GetCurrentLocaleCode()),
                alreadyBoughtProvider = () => CrimeForecastLocalization.Get(
                    CrimeForecastLocalization.ServiceStatus, LocHelper.GetCurrentLocaleCode(),
                    GetRuntimeService()?.cost ?? AutoEvidenceCallState.InitialPrice)
            };
            NetworkUpgrade.RegisterModUpgrade(definition, null);
        }

        private static NetworkUpgrade? GetRuntimeService()
        {
            PlayerStore? store = PlayerStore.Instance;
            if (store?.networkUpgrade == null) return null;
            store.networkUpgrade.TryGetValue(UpgradeId, out NetworkUpgrade? service);
            return service;
        }

        private static void LoadState()
        {
            _state = new AutoEvidenceCallState();
            PlayerStore? store = PlayerStore.Instance;
            if (store == null) return;
            if (store.ModData().TryGetValue(SaveKey, out string? serialized) &&
                !AutoEvidenceCallState.TryDeserialize(serialized, out _state))
            {
                NextOpeningVisitorsMod.Log?.Warning(
                    "Invalid automatic evidence cleanup state; using safe defaults without changing investigation or cash.");
                _state = new AutoEvidenceCallState();
            }
        }

        private static void SaveState()
        {
            if (_state == null || PlayerStore.Instance == null) return;
            PlayerStore.Instance.ModData()[SaveKey] = _state.Serialize();
        }

        private static void EnsureHost()
        {
            if (_host != null) return;
            var hostObject = new GameObject("NextOpeningVisitors.AutoEvidenceCallHost");
            UnityEngine.Object.DontDestroyOnLoad(hostObject);
            _host = hostObject.AddComponent<AutoEvidenceCallHost>();
        }

        private static void ArmIfPending()
        {
            _armed = _state?.HasPendingOffer ?? false;
        }

        internal static void OnPhoneOpened()
        {
            if (_state?.HasPendingOffer != true || StoreStation.Instance == null) return;
            int day = StoreStation.Instance.dayCounter;
            if (_state.OfferDay != day || _presentedOfferDay != day ||
                PhoneUIManager.Instance?.currentPhoneClient != null) return;

            // A dismissed, unanswered offer becomes available again when the player reopens the phone.
            _presentedOfferDay = -1;
            _armed = true;
        }

        internal static void TryPresentPending()
        {
            if (!_armed || _state == null) return;
            PlayerStore? store = PlayerStore.Instance;
            PhoneUIManager? phone = PhoneUIManager.Instance;
            if (store?.secData == null || phone == null || StoreStation.Instance == null) return;

            int day = StoreStation.Instance.dayCounter;
            if (!_state.HasPendingOffer || _state.OfferDay != day || store.secData.evidenceLevel < 100)
            {
                if (_state.HasPendingOffer)
                {
                    _state.CancelPending();
                    SaveState();
                }
                _armed = false;
                return;
            }
            if (_presentedOfferDay == day)
            {
                _armed = false;
                return;
            }
            if (IsBusy(phone)) return;

            bool wasPhoneOpen = phone.phonePanel != null && phone.phonePanel.activeSelf;
            try
            {
                if (!wasPhoneOpen) phone.OpenUI();
                Dialogue offer = BuildOfferDialog(_state.OfferPrice);
                AutoEvidenceCallDialogPatch.InjectedDialog = offer;
                try { phone.StartPhoneDialog(StorePhoneClient.WILD_CLIENT_NUMBER); }
                finally { AutoEvidenceCallDialogPatch.InjectedDialog = null; }

                if (phone.currentPhoneClient == null)
                    throw new InvalidOperationException("The Wild phone dialogue did not start.");
                _presentedOfferDay = day;
                _armed = false;
                NextOpeningVisitorsMod.Log?.Log(
                    $"Automatic evidence cleanup call presented: day={day}, price={_state.OfferPrice}.");
            }
            catch (Exception exception)
            {
                AutoEvidenceCallDialogPatch.InjectedDialog = null;
                NextOpeningVisitorsMod.Log?.Warning("Could not present the automatic evidence cleanup call. " + exception.Message);
                if (!wasPhoneOpen) phone.CloseUI();
            }
        }

        private static bool IsBusy(PhoneUIManager phone)
        {
            if (PlayerStore.Instance.currentClientInstance != null) return true;
            if (MapUIManager.Instance != null && MapUIManager.Instance.isOutside) return true;
            if (StartOfDayUIManager.Instance != null && StartOfDayUIManager.Instance.UIPanel != null &&
                StartOfDayUIManager.Instance.UIPanel.activeSelf) return true;
            if (NegociationUIManager.Instance != null && NegociationUIManager.Instance.IsOpen) return true;
            if (EndOfDayUIManager.Instance != null && EndOfDayUIManager.Instance.IsOpen) return true;
            if (WildNetworkUI.IsOpen) return true;
            if (phone.currentPhoneClient != null) return true;

            DialoguePhoneUIManager? dialogue = DialoguePhoneUIManager.Instance;
            if (dialogue != null && ((dialogue.clientBubble != null && dialogue.clientBubble.gameObject.activeSelf) ||
                (dialogue.playerBubble != null && dialogue.playerBubble.gameObject.activeSelf))) return true;

            StoreUIManager? storeUI = StoreUIManager.Instance;
            if (storeUI?.paperUICanvasObject != null)
            {
                foreach (Transform child in storeUI.paperUICanvasObject.transform)
                {
                    if (!child.gameObject.activeSelf) continue;
                    if (phone.phonePanel != null &&
                        (child == phone.phonePanel.transform || child.IsChildOf(phone.phonePanel.transform))) continue;
                    return true;
                }
            }
            return false;
        }

        private static Dialogue BuildOfferDialog(int lockedPrice)
        {
            string locale = LocHelper.GetCurrentLocaleCode();
            string wildName = StorePhoneClient.GetWildPhoneClient()?.GetLocalizedDisplayName() ?? "Wild";
            bool choiceResolved = false;
            var acceptChoice = new Dialogue().SetText("PLAYER",
                CrimeForecastLocalization.Get(CrimeForecastLocalization.Accept, locale, lockedPrice));
            var declineChoice = new Dialogue().SetText("PLAYER",
                CrimeForecastLocalization.Get(CrimeForecastLocalization.Decline, locale));

            var offer = new Dialogue().SetText(wildName,
                CrimeForecastLocalization.Get(CrimeForecastLocalization.CallOffer, locale, lockedPrice));
            offer.AddChoice(DirectoryMaster.Item("$"), "", "", acceptChoice, () =>
            {
                if (choiceResolved) return;
                choiceResolved = true;
                string message = AcceptOffer(out int nextPrice);
                string result = nextPrice > 0
                    ? CrimeForecastLocalization.Get(CrimeForecastLocalization.Success, locale, nextPrice)
                    : message;
                SetResult(acceptChoice, wildName, result);
            });
            offer.AddChoice(DirectoryMaster.Item("$"), "", "", declineChoice, () =>
            {
                if (choiceResolved) return;
                choiceResolved = true;
                if (_state != null && StoreStation.Instance != null &&
                    _state.Reject(StoreStation.Instance.dayCounter)) SaveState();
                NextOpeningVisitorsMod.Log?.Log("Automatic evidence cleanup offer declined; no cash or investigation changed.");
                SetResult(declineChoice, wildName,
                    CrimeForecastLocalization.Get(CrimeForecastLocalization.Rejected, locale));
            });
            return offer;
        }

        private static void SetResult(Dialogue choice, string wildName, string message)
        {
            choice.NextDialogue().SetText(wildName, message).SetEndAction(() =>
            {
                PhoneUIManager.Instance?.OnPhoneDialogEnd();
            });
        }

        private static string AcceptOffer(out int nextPrice)
        {
            nextPrice = 0;
            string locale = LocHelper.GetCurrentLocaleCode();
            if (_acceptInProgress)
                return CrimeForecastLocalization.Get(CrimeForecastLocalization.Failure, locale);
            _acceptInProgress = true;

            PlayerStore? store = PlayerStore.Instance;
            NetworkUpgrade? service = GetRuntimeService();
            AutoEvidenceCallState? state = _state;
            if (store?.secData == null || service == null || state == null || StoreStation.Instance == null)
            {
                _acceptInProgress = false;
                return CrimeForecastLocalization.Get(CrimeForecastLocalization.Failure, locale);
            }

            AutoEvidenceAcceptanceStatus status = AutoEvidenceCallRules.ValidateAcceptance(
                state, StoreStation.Instance.dayCounter, store.secData.evidenceLevel, store.playerCash);
            if (status != AutoEvidenceAcceptanceStatus.Valid)
            {
                if (status != AutoEvidenceAcceptanceStatus.InsufficientCash && state.HasPendingOffer)
                {
                    state.CancelPending();
                    SaveState();
                }
                _acceptInProgress = false;
                return status == AutoEvidenceAcceptanceStatus.InsufficientCash
                    ? CrimeForecastLocalization.Get(CrimeForecastLocalization.InsufficientCash, locale, state.OfferPrice)
                    : CrimeForecastLocalization.Get(CrimeForecastLocalization.Expired, locale);
            }

            int oldCash = store.playerCash;
            int oldEvidence = store.secData.evidenceLevel;
            int oldCaseState = store.secData.state;
            int oldServiceCost = service.cost;
            string oldCallState = state.Serialize();
            try
            {
                int increasedPrice = AutoEvidenceCallRules.GetNextPrice(state.OfferPrice);
                store.playerCash -= state.OfferPrice;
                store.secData.evidenceLevel = 0;
                store.secData.state = 0;
                service.cost = increasedPrice;
                state.CompleteAcceptance();
                SaveState();
                nextPrice = increasedPrice;
                NextOpeningVisitorsMod.Log?.Log(
                    $"Automatic evidence cleanup accepted: paid={oldCash - store.playerCash}, nextPrice={increasedPrice}; crime values and visitor queues unchanged.");
                return string.Empty;
            }
            catch (Exception exception)
            {
                store.playerCash = oldCash;
                store.secData.evidenceLevel = oldEvidence;
                store.secData.state = oldCaseState;
                service.cost = oldServiceCost;
                if (AutoEvidenceCallState.TryDeserialize(oldCallState, out AutoEvidenceCallState? restored)) _state = restored;
                try { SaveState(); }
                catch (Exception saveException)
                {
                    NextOpeningVisitorsMod.Log?.Error("Could not restore the automatic evidence offer state. " + saveException.Message);
                }
                NextOpeningVisitorsMod.Log?.Error("Automatic evidence cleanup failed; all changes were rolled back. " + exception.Message);
                return CrimeForecastLocalization.Get(CrimeForecastLocalization.Failure, locale);
            }
            finally
            {
                _acceptInProgress = false;
            }
        }
    }

    internal sealed class AutoEvidenceCallHost : MonoBehaviour
    {
        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < 0.75f) return;
            _timer = 0f;
            try { AutoEvidenceCallController.TryPresentPending(); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Automatic evidence call retry failed safely. " + exception.Message);
            }
        }
    }
}
