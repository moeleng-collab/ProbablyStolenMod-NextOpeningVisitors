using System;
using HarmonyLib;

namespace NextOpeningVisitors
{
    public sealed class NextOpeningVisitorsMod : IMod
    {
        internal static ModLog? Log { get; private set; }

        public void Init(ModManifest manifest)
        {
            Log = new ModLog(manifest);
        }

        public void OnEnable()
        {
            NightOwlController.Register();
            FeatureSettings.SetModActive(true);
            FeatureSettings.ApplyAll();
            ModHook.OnGameLoadedLate += HandleGameLoaded;
            ModHook.OnNewGameStarted += HandleNewGame;
            ModHook.OnWakingUpLate += AutoEvidenceCallController.OnWakingUp;
            Log?.Log("Initialized: Next Opening Visitors v1.7.0");
        }

        public void OnDisable()
        {
            NightOwlController.Unregister();
            FeatureSettings.SetModActive(false);
            FeatureSettings.ApplyAll();
            ModHook.OnGameLoadedLate -= HandleGameLoaded;
            ModHook.OnNewGameStarted -= HandleNewGame;
            ModHook.OnWakingUpLate -= AutoEvidenceCallController.OnWakingUp;
            Log?.Log("Disabled: Next Opening Visitors");
        }

        private static void HandleGameLoaded()
        {
            FeatureSettings.LoadState();
            FeatureSettings.ApplyAll();
            AutoEvidenceCallController.OnGameLoaded();
            BerryQualityUI.OnGameLoaded();
        }

        private static void HandleNewGame(string scenarioId)
        {
            FeatureSettings.LoadState();
            FeatureSettings.ApplyAll();
            AutoEvidenceCallController.OnNewGame();
            BerryQualityUI.OnGameLoaded();
        }
    }

    [HarmonyPatch(typeof(PlayerStore), nameof(PlayerStore.HandleFutureClientQueue))]
    internal static class HandleFutureClientQueuePatch
    {
        private static void Prefix(PlayerStore __instance)
        {
            try
            {
                int duplicates = VisitorRules.RemoveDuplicateAppointments(
                    __instance.futurStoreClientIdQueue,
                    __instance.futurStoreClientDayQueue);
                if (duplicates > 0)
                    NextOpeningVisitorsMod.Log?.Log($"Removed {duplicates} duplicate supplier/Odin appointment(s); kept the earliest.");

                if (FeatureSettings.IsEnabled(ModFeature.VisitorPriority))
                {
                    int changed = VisitorRules.NormalizeDelays(
                        __instance.futurStoreClientIdQueue,
                        __instance.futurStoreClientDayQueue);
                    if (changed > 0)
                        NextOpeningVisitorsMod.Log?.Log($"Scheduled {changed} supplier/Odin visit(s) for the next opening.");
                }
            }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not normalize future visits; vanilla scheduling will continue. " + exception.Message);
            }
        }

        private static void Postfix(PlayerStore __instance)
        {
            if (!FeatureSettings.IsEnabled(ModFeature.VisitorPriority)) return;
            try
            {
                StoreClientManager? manager = __instance.storeClientManager;
                if (manager?.clientStack == null) return;

                int prioritized = VisitorRules.Prioritize(
                    manager.clientStack,
                    client => client == null ? null : client.identifier);
                if (prioritized > 0)
                    NextOpeningVisitorsMod.Log?.Log($"Prioritized {prioritized} supplier/Odin visitor(s) at the front of the opening queue.");
            }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not prioritize the opening queue; vanilla order will continue. " + exception.Message);
            }
        }
    }
}
