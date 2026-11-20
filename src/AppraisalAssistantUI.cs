using System;
using System.Globalization;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NextOpeningVisitors
{
    [HarmonyPatch(typeof(InspectionUIManager), nameof(InspectionUIManager.OnInspectItem))]
    internal static class AutoAppraisalInspectPatch
    {
        private static void Postfix(InspectionUIManager __instance, GameItem gameItem)
        {
            try { AppraisalAssistantUI.AutoFill(__instance, gameItem); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not fill the appraisal estimate; the original value was kept. " + exception.Message);
            }
        }
    }

    [HarmonyPatch(typeof(InspectionUIManager))]
    internal static class AppraisalItemDetailsPatches
    {
        [HarmonyPostfix, HarmonyPatch(nameof(InspectionUIManager.HandleInjector))]
        private static void Injector(InspectionUIManager __instance, GameItem gameItem) => Show(__instance, gameItem);

        [HarmonyPostfix, HarmonyPatch(nameof(InspectionUIManager.HandleCigarette))]
        private static void Cigarette(InspectionUIManager __instance, GameItem gameItem) => Show(__instance, gameItem);

        [HarmonyPostfix, HarmonyPatch(nameof(InspectionUIManager.HandleStamp))]
        private static void Stamp(InspectionUIManager __instance, GameItem gameItem) => Show(__instance, gameItem);

        [HarmonyPostfix, HarmonyPatch(nameof(InspectionUIManager.HandleRibwich))]
        private static void Ribwich(InspectionUIManager __instance, GameItem gameItem) => Show(__instance, gameItem);

        [HarmonyPostfix, HarmonyPatch(nameof(InspectionUIManager.HandleAppHand))]
        private static void Hand(InspectionUIManager __instance, GameItem gameItem) => Show(__instance, gameItem);

        [HarmonyPostfix, HarmonyPatch(nameof(InspectionUIManager.HandlePlasma))]
        private static void Plasma(InspectionUIManager __instance, GameItem gameItem) => Show(__instance, gameItem);

        [HarmonyPostfix, HarmonyPatch(nameof(InspectionUIManager.HandleInsBerry))]
        private static void Berry(InspectionUIManager __instance, GameItem gameItem)
        {
            try { BerryQualityUI.MarkAppraised(gameItem); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not record the glowberry appraisal. " + exception.Message);
            }
            Show(__instance, gameItem);
        }

        private static void Show(InspectionUIManager manager, GameItem item)
        {
            try { AppraisalAssistantUI.ShowItemDetails(manager, item); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not show item appraisal details. " + exception);
                AppraisalAssistantUI.DestroyItemPanel();
            }
        }
    }

    [HarmonyPatch(typeof(InspectionUIManager), nameof(InspectionUIManager.CloseUI))]
    internal static class AppraisalDetailsClosePatch
    {
        private static void Postfix() => AppraisalAssistantUI.DestroyItemPanel();
    }

    [HarmonyPatch(typeof(StoreUIManager), nameof(StoreUIManager.EnterMagnifyingMode))]
    internal static class AugDetectionEnterPatch
    {
        private static void Postfix(StoreUIManager __instance)
        {
            try { AppraisalAssistantUI.OnEnterMagnifying(__instance); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not show augmented-human inspection details. " + exception.Message);
                AppraisalAssistantUI.DestroyPanel();
            }
        }
    }

    [HarmonyPatch(typeof(StoreUIManager), nameof(StoreUIManager.ExitMagnifyingMode))]
    internal static class AugDetectionExitPatch
    {
        private static void Postfix() => AppraisalAssistantUI.DestroyPanel();
    }

    internal static class AppraisalAssistantUI
    {
        private static TreeNodeRender? _panel;
        private static CustomUIWindow? _itemWindow;
        private static CustomUIWindow? _arrivalWindow;
        private static string? _arrivalDiagnosticKey;
        private static readonly ArrivalAlertSession ArrivalSession = new ArrivalAlertSession();
        private static CustomerAlertHost? _host;
        private static bool _enabled;

        internal static void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!FeatureSettings.IsEnabled(ModFeature.AppraisalDetails)) DestroyItemPanel();
            if (!FeatureSettings.IsEnabled(ModFeature.MagnifierAugDetection)) DestroyPanel();
            if (!FeatureSettings.IsEnabled(ModFeature.ArrivalIdentityAlert)) DestroyArrivalPanel();
            if (!enabled)
            {
                if (_host != null) UnityEngine.Object.Destroy(_host.gameObject);
                _host = null;
                ArrivalSession.Reset();
                DestroyPanel();
                DestroyItemPanel();
                DestroyArrivalPanel();
                return;
            }
            EnsureHost();
        }

        internal static void AutoFill(InspectionUIManager manager, GameItem? item)
        {
            if (!_enabled || !FeatureSettings.IsEnabled(ModFeature.AutomaticAppraisalValue) ||
                manager == null || item == null || manager.apprisalServiceInputField == null) return;
            StoreClient? client = PlayerStore.Instance?.currentClientInstance?.GetClientBlueprint();
            if (client == null) return;

            bool appraisalService = client.clientIntent == StoreClient.ClientIntent.APPRAISAL_SERVICE;
            bool appraisalItem = item.IsTag(AppraisalHelper.APPRAISAL_ITEM);
            if (!appraisalService || !appraisalItem) return;

            int correctValue = AppraisalHelper.GetCorrectBaseValue(item);
            if (!AppraisalAssistantRules.CanAutoFill(appraisalService, appraisalItem, correctValue)) return;

            TMP_InputField input = manager.apprisalServiceInputField;
            input.SetTextWithoutNotify(correctValue.ToString(CultureInfo.InvariantCulture));
            manager.OnValueChanged(input.text);
        }

        internal static void ShowItemDetails(InspectionUIManager manager, GameItem? item)
        {
            DestroyItemPanel();
            if (!_enabled || !FeatureSettings.IsEnabled(ModFeature.AppraisalDetails) ||
                manager == null || manager.panel == null || item == null) return;
            string locale = LocHelper.GetCurrentLocaleCode();
            string text = AppraisalItemDetails.Build(item, locale);
            if (text.Length == 0)
            {
                NextOpeningVisitorsMod.Log?.Warning("Appraisal detail text was empty for item: " + item.identifier);
                return;
            }

            CreateItemWindow(item, text, locale);
        }

        internal static void OnEnterMagnifying(StoreUIManager manager)
        {
            DestroyPanel();
            if (!_enabled || !FeatureSettings.IsEnabled(ModFeature.MagnifierAugDetection) ||
                manager == null || manager.magnifyingScreen == null || manager.isShootingMode) return;
            StoreClient? client = PlayerStore.Instance?.currentClientInstance?.GetClientBlueprint();
            if (client == null) return;

            AugDetectionReason reason = AppraisalAssistantRules.DetectAug(client.isAug,
                AppraisalAssistantRules.HasVisibleAugSprite(client.spriteName),
                client.augIdleTell == StoreClient.AugIdleTell.Freeze,
                client.augIdleTell == StoreClient.AugIdleTell.Seizure,
                client.augTier);
            string locale = LocHelper.GetCurrentLocaleCode();
            int titleKey = reason == AugDetectionReason.None ? AssistantLocalization.NoAug : AssistantLocalization.AugDetected;
            string text = AssistantLocalization.Get(titleKey, locale);
            if (reason != AugDetectionReason.None)
                text += "\n" + AssistantLocalization.Get(AssistantLocalization.ReasonKey(reason), locale);
            CreateTextPanel(manager.magnifyingScreen.transform, "AugDetection", new Vector2(0.5f, 0f),
                new Vector2(0f, 26f), new Vector2(720f, 92f), 20f, TextAlignmentOptions.Center,
                out _panel, out TreeNodeRender textNode);
            if (textNode?.text == null) return;
            textNode.text.text = text;
            textNode.text.color = reason == AugDetectionReason.None
                ? new Color32(152, 220, 164, 255)
                : new Color32(245, 184, 65, 255);
        }

        internal static void RefreshArrivalAlert()
        {
            if (!_enabled || !FeatureSettings.IsEnabled(ModFeature.ArrivalIdentityAlert))
            {
                DestroyArrivalPanel();
                return;
            }
            PlayerStore? store = PlayerStore.HasInstance ? PlayerStore.Instance : null;
            bool arrived = store != null && store.isClientArrived && store.currentClientInstance != null;
            StoreClientInstance? customer = arrived && store != null ? store.currentClientInstance : null;
            StoreClient? client = customer?.GetClientBlueprint();
            bool uiReady = CustomUIManager.Instance != null;
            bool eligible = client != null && AppraisalAssistantRules.ShouldAlertArrival(
                arrived, client.isAug, client.isWanted);
            bool shouldShow = ArrivalSession.ShouldShow(arrived, customer, eligible, uiReady);
            LogArrivalState(arrived, client, uiReady);
            if (!shouldShow || client == null)
            {
                DestroyArrivalPanel();
                return;
            }

            string locale = LocHelper.GetCurrentLocaleCode();
            string title = client.isAug && client.isWanted ? "arrival.both" : client.isAug ? "arrival.aug" : "arrival.wanted";
            var text = new StringBuilder(AssistantAppraisalLocalization.Get(title, locale));
            AppendAlertLine(text, "arrival.name", locale, Safe(client.displayName));
            if (client.isWanted) AppendAlertLine(text, "arrival.realName", locale, Safe(client.realName));
            if (client.isAug) AppendAlertLine(text, "arrival.tier", locale, client.augTier.ToString(CultureInfo.InvariantCulture));
            if (client.isWanted) AppendAlertLine(text, "arrival.reward", locale, client.reportReward.ToString(CultureInfo.InvariantCulture));

            if (_arrivalWindow == null || !_arrivalWindow.IsAlive)
            {
                CreateArrivalWindow(text.ToString(), locale, customer!);
                return;
            }

            _arrivalWindow.SetTitle(ArrivalWindowTitle(locale));
            CustomUIElement? element = _arrivalWindow.Get("details");
            if (element?.text == null) return;
            element.text.text = text.ToString();
            ApplyItemFont(element.text, locale);
        }

        private static void LogArrivalState(bool arrived, StoreClient? client, bool uiReady)
        {
            string key = arrived.ToString(CultureInfo.InvariantCulture) + "|" + uiReady.ToString(CultureInfo.InvariantCulture);
            if (client != null)
                key += "|" + client.identifier + "|" + client.isAug + "|" + client.augTier + "|" + client.isWanted;
            if (key == _arrivalDiagnosticKey) return;
            _arrivalDiagnosticKey = key;
            NextOpeningVisitorsMod.Log?.Log("Customer alert state: arrived=" + arrived +
                ", client=" + (client == null ? "none" : Safe(client.displayName)) +
                ", isAug=" + (client != null && client.isAug) +
                ", augTier=" + (client == null ? "—" : client.augTier.ToString(CultureInfo.InvariantCulture)) +
                ", isWanted=" + (client != null && client.isWanted) +
                ", nativeUIReady=" + uiReady);
        }

        private static string ArrivalWindowTitle(string locale) =>
            AssistantAppraisalLocalization.Get("title.arrival", locale);

        private static void CreateArrivalWindow(string text, string locale, StoreClientInstance customer)
        {
            if (CustomUIManager.Instance == null)
                throw new InvalidOperationException("The native custom UI manager is not ready.");

            CustomUIBuilder builder = CustomUIManager.Window(
                "nextopeningvisitors.customer_alert", ArrivalWindowTitle(locale), "overlay");
            CustomUIWindow window = builder.Handle;
            _arrivalWindow = window;
            window.onClose = () => DismissArrivalAlert(customer);
            builder.SetSize(480f, 210f)
                .SetPosition(new Vector2(0f, 320f))
                .SetDraggable(true)
                .SetCloseOnEscape(false)
                .Show();

            builder.AddLabel(text, "details");
            CustomUIElement element = window.Get("details");
            if (element == null || element.text == null)
                throw new InvalidOperationException("The native customer alert label could not be created.");

            TextMeshProUGUI label = element.text;
            ApplyItemFont(label, locale);
            if (window.frame != null && window.frame.titleText != null)
                ApplyItemFont(window.frame.titleText, locale);
            label.enableAutoSizing = false;
            label.fontSize = 20f;
            label.enableWordWrapping = true;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.raycastTarget = false;
            element.SetPreferredSize(438f, 150f);
            CustomUIBuilder.RebuildLayout(window.Rect);
            window.SetSize(480f, 210f);
            window.Show();
            label.ForceMeshUpdate();

            NextOpeningVisitorsMod.Log?.Log("Customer alert window: alive=" + window.IsAlive +
                ", visible=" + window.IsVisible +
                ", rect=" + window.Rect.rect +
                ", renderedChars=" + label.textInfo.characterCount +
                ", font=" + (label.font != null ? label.font.name : "none"));
        }

        private static void DismissArrivalAlert(StoreClientInstance customer)
        {
            if (!ArrivalSession.Dismiss(customer)) return;
            if (_arrivalWindow != null) _arrivalWindow.onClose = null;
            _arrivalWindow = null;
        }

        private static void AppendAlertLine(StringBuilder text, string key, string locale, string value)
        {
            text.Append('\n').Append(AssistantAppraisalLocalization.Get(key, locale, value));
        }

        private static string Safe(string? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value)) return "—";
            return value.Replace('<', '‹').Replace('>', '›').Trim();
        }

        private static void EnsureHost()
        {
            if (_host != null) return;
            var hostObject = new GameObject("NextOpeningVisitors.CustomerAlertHost");
            UnityEngine.Object.DontDestroyOnLoad(hostObject);
            _host = hostObject.AddComponent<CustomerAlertHost>();
        }

        private static void CreateTextPanel(Transform parent, string name, Vector2 anchor, Vector2 position,
            Vector2 size, float fontSize, TextAlignmentOptions alignment,
            out TreeNodeRender panel, out TreeNodeRender textNode)
        {
            panel = TreeNodeRender.InstantiateAsRoot(parent, false);
            panel.gameObject.name = "NextOpeningVisitors." + name;
            panel.image.sprite = null;
            panel.image.color = new Color(0.035f, 0.045f, 0.055f, 0.94f);
            panel.image.raycastTarget = false;
            RectTransform panelRect = panel.rectTransform;
            panelRect.anchorMin = anchor;
            panelRect.anchorMax = anchor;
            panelRect.pivot = anchor;
            panelRect.sizeDelta = size;
            panelRect.anchoredPosition = position;
            panelRect.SetAsLastSibling();
            panel.gameObject.SetActive(true);

            textNode = TreeNodeRender.InstantiateText().SetParent(panel);
            textNode.gameObject.SetActive(true);
            textNode.gameObject.name = "NextOpeningVisitors." + name + "Text";
            textNode.text.raycastTarget = false;
            textNode.text.alignment = alignment;
            textNode.text.fontSize = fontSize;
            textNode.text.enableWordWrapping = true;
            textNode.text.overflowMode = TextOverflowModes.Overflow;
            RectTransform textRect = textNode.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 10f);
            textRect.offsetMax = new Vector2(-16f, -10f);
        }

        private static void CreateItemWindow(GameItem item, string text, string locale)
        {
            if (CustomUIManager.Instance == null)
                throw new InvalidOperationException("The native custom UI manager is not ready.");

            string title = AssistantAppraisalLocalization.Get("title.appraisal", locale);
            CustomUIBuilder builder = CustomUIManager.Window("nextopeningvisitors.item_appraisal", title, "overlay");
            CustomUIWindow window = builder.Handle;
            _itemWindow = window;
            builder.SetSize(420f, 160f)
                .SetPosition(new Vector2(-280f, 60f))
                .SetDraggable(true)
                .SetCloseOnEscape(false)
                .Show();

            builder.AddLabel(text, "details");
            CustomUIElement element = window.Get("details");
            if (element == null || element.text == null)
                throw new InvalidOperationException("The native appraisal label could not be created.");

            TextMeshProUGUI label = element.text;
            ApplyItemFont(label, locale);
            if (window.frame != null && window.frame.titleText != null)
                ApplyItemFont(window.frame.titleText, locale);
            label.enableAutoSizing = false;
            label.enableWordWrapping = true;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.raycastTarget = false;

            Vector2 preferred = label.GetPreferredValues(text, 380f, float.PositiveInfinity);
            element.SetPreferredSize(380f, Math.Max(22f, Mathf.Ceil(preferred.y) + 2f));
            CustomUIBuilder.RebuildLayout(window.Rect);
            float height = Mathf.Clamp(Mathf.Ceil(LayoutUtility.GetPreferredHeight(window.Rect)), 80f, 520f);
            window.SetSize(420f, height);
            CustomUIBuilder.RebuildLayout(window.Rect);
            label.ForceMeshUpdate();

            NextOpeningVisitorsMod.Log?.Log("Appraisal detail window: item=" + item.identifier +
                ", chars=" + text.Length +
                ", renderedChars=" + label.textInfo.characterCount +
                ", alive=" + window.IsAlive +
                ", visible=" + window.IsVisible +
                ", rect=" + window.Rect.rect +
                ", font=" + (label.font != null ? label.font.name : "none"));
        }

        private static void ApplyItemFont(TMP_Text text, string locale)
        {
            FontLoader loader = FontLoader.Instance;
            if (loader == null) return;
            string normalized = locale.Replace('_', '-').ToLowerInvariant();
            TMP_FontAsset? font = normalized.StartsWith("zh", StringComparison.Ordinal)
                ? loader.simplifiedChineseFont
                : normalized.StartsWith("ja", StringComparison.Ordinal) ? loader.japaneseFont : loader.notoSans;
            if (font != null) text.font = font;
        }

        internal static void ApplyItemFontToWindow(CustomUIWindow? window, string locale)
        {
            if (window?.root == null) return;
            foreach (Component component in window.root.GetComponentsInChildren(typeof(TextMeshProUGUI), true))
                if (component is TextMeshProUGUI text) ApplyItemFont(text, locale);
        }

        internal static void DestroyPanel()
        {
            if (_panel != null) UnityEngine.Object.Destroy(_panel.gameObject);
            _panel = null;
        }

        internal static void DestroyItemPanel()
        {
            CustomUIWindow? window = _itemWindow;
            _itemWindow = null;
            if (window == null) return;
            try
            {
                window.onClose = null;
                if (window.IsAlive) window.Close();
            }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not close the appraisal detail window. " + exception.Message);
            }
        }

        private static void DestroyArrivalPanel()
        {
            CustomUIWindow? window = _arrivalWindow;
            _arrivalWindow = null;
            if (window == null) return;
            try
            {
                window.onClose = null;
                if (window.IsAlive) window.Close();
            }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not close the customer arrival alert. " + exception.Message);
            }
        }
    }

    internal sealed class CustomerAlertHost : MonoBehaviour
    {
        private float _timer = 0.25f;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < 0.25f) return;
            _timer = 0f;
            try { AppraisalAssistantUI.RefreshArrivalAlert(); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not refresh the customer arrival alert. " + exception.Message);
            }
        }
    }
}
