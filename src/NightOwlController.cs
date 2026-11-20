using System;
using System.Collections.Generic;
using HarmonyLib;

namespace NextOpeningVisitors
{
    internal static class NightOwlController
    {
        private const string ModId = "nextopeningvisitors";
        private static bool _modActive;

        internal static void Register()
        {
            _modActive = true;
            ModHelper.AddStartingPerk(
                ModId,
                NightOwlRules.PerkId,
                NightOwlRules.PerkCost,
                StartingPerk.StartingPerkType.POSITIVE,
                () => NightOwlLocalization.Name(CurrentLocale()),
                () => NightOwlLocalization.Description(CurrentLocale()));
        }

        internal static void Unregister()
        {
            _modActive = false;
            ModHelper.RemoveStartingPerks(ModId);
        }

        internal static bool IsActive()
        {
            return _modActive && PlayerStore.HasInstance && ModHelper.IsStartingPerkActive(NightOwlRules.PerkId);
        }

        internal static bool IsVisitedTonight(AfterhourLocation? location)
        {
            if (location == null || !PlayerStore.HasInstance || StoreStation.Instance == null) return false;
            AfterhourLocationState state = PlayerStore.Instance.GetAfterhourState(location.Id);
            return NightOwlRules.HasVisitedToday(
                location.Id,
                state.visits,
                state.lastVisitDay,
                StoreStation.Instance.dayCounter);
        }

        internal static bool ShouldBlock(AfterhourLocation? location)
        {
            return IsActive() && IsVisitedTonight(location);
        }

        internal static void NotifyRepeatVisit()
        {
            if (StoreUIManager.Instance != null)
                StoreUIManager.Instance.Notify(NightOwlLocalization.RepeatVisit(CurrentLocale()));
        }

        private static bool HasNativeOutingBlocker(PlayerStore store)
        {
            return HealthData.IsSeriouslyWounded() ||
                StartingPerkContent.IsAgoraphobiaResting() ||
                StoreStation.Instance.storeEventManager.IsEventActive("crackdown") ||
                !store.isAfterhoursIntroduced;
        }

        private static void PrepareDoor()
        {
            if (!IsActive() || StoreStation.Instance == null) return;
            PlayerStore store = PlayerStore.Instance;
            if (store.storeState != PlayerStore.StoreState.CLOSED || store.visitLeftTonight > 0 ||
                HasNativeOutingBlocker(store)) return;
            if (HasUnvisitedDiscoveredLocation()) store.visitLeftTonight = 1;
        }

        private static void UpdateDoorTooltip(ref string result)
        {
            if (!IsActive() || StoreStation.Instance == null) return;
            PlayerStore store = PlayerStore.Instance;
            if (store.storeState != PlayerStore.StoreState.CLOSED || store.visitLeftTonight != 0 ||
                HasNativeOutingBlocker(store)) return;
            result = HasUnvisitedDiscoveredLocation()
                ? LocHelper.GetLocalizedInteractable("door_leave_store")
                : NightOwlLocalization.AllLocationsVisited(CurrentLocale());
        }

        private static bool HasUnvisitedDiscoveredLocation()
        {
            if (StoreStation.Instance == null) return false;
            List<AfterhourLocation> locations = AfterhourLocation.GetDiscovered();
            int today = StoreStation.Instance.dayCounter;
            return NightOwlRules.HasUnvisited(
                locations,
                location => location.Id,
                location => location.State.visits,
                location => location.State.lastVisitDay,
                today);
        }

        private static string CurrentLocale()
        {
            return LocHelper.GetCurrentLocaleCode();
        }

        [HarmonyPatch(typeof(StoreDoor), nameof(StoreDoor.OnClick))]
        internal static class StoreDoorClickPatch
        {
            private static void Prefix()
            {
                try
                {
                    PrepareDoor();
                }
                catch (Exception exception)
                {
                    NextOpeningVisitorsMod.Log?.Warning("Night Owl could not restore the nightly outing; vanilla rules will continue. " + exception.Message);
                }
            }
        }

        [HarmonyPatch(typeof(StoreDoor), nameof(StoreDoor.GetTooltip))]
        internal static class StoreDoorTooltipPatch
        {
            private static void Postfix(ref string __result)
            {
                try
                {
                    UpdateDoorTooltip(ref __result);
                }
                catch (Exception exception)
                {
                    NextOpeningVisitorsMod.Log?.Warning("Night Owl could not update the store door tooltip. " + exception.Message);
                }
            }
        }

        [HarmonyPatch(typeof(MapUIManager), nameof(MapUIManager.Travel))]
        internal static class TravelPatch
        {
            private static bool Prefix(AfterhourLocation location)
            {
                try
                {
                    if (!ShouldBlock(location)) return true;
                    NotifyRepeatVisit();
                    return false;
                }
                catch (Exception exception)
                {
                    NextOpeningVisitorsMod.Log?.Warning("Night Owl could not validate an outing; vanilla travel will continue. " + exception.Message);
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(TransitUI), "BuildCard")]
        internal static class TransitCardPatch
        {
            private static void Postfix(AfterhourLocation location, TransitCardView __result)
            {
                try
                {
                    if (__result != null && ShouldBlock(location)) __result.available = false;
                }
                catch (Exception exception)
                {
                    NextOpeningVisitorsMod.Log?.Warning("Night Owl could not update a transit card. " + exception.Message);
                }
            }
        }

        [HarmonyPatch(typeof(TransitUI), nameof(TransitUI.Select))]
        internal static class TransitSelectionPatch
        {
            private static bool Prefix(AfterhourLocation location)
            {
                try
                {
                    if (!ShouldBlock(location)) return true;
                    NotifyRepeatVisit();
                    return false;
                }
                catch (Exception exception)
                {
                    NextOpeningVisitorsMod.Log?.Warning("Night Owl could not validate a transit selection; vanilla selection will continue. " + exception.Message);
                    return true;
                }
            }
        }
    }
}
