using System;

namespace NextOpeningVisitors
{
    internal enum DemandHighlightLevel
    {
        None = 0,
        Demand = 1,
        Stolen = 2,
        Contraband = 3
    }

    internal static class DemandHighlightRules
    {
        internal const int MaximumDepth = 32;

        internal static DemandHighlightLevel Classify(bool matches, bool stolen, bool contraband)
        {
            if (!matches) return DemandHighlightLevel.None;
            if (contraband) return DemandHighlightLevel.Contraband;
            return stolen ? DemandHighlightLevel.Stolen : DemandHighlightLevel.Demand;
        }

        internal static DemandHighlightLevel Merge(DemandHighlightLevel first, DemandHighlightLevel second)
        {
            return first >= second ? first : second;
        }

        internal static bool CanDescend(bool inspection, bool behindLock, bool lockedContainer, bool blocksSearch)
        {
            if (behindLock || lockedContainer) return false;
            return !inspection || !blocksSearch;
        }

        internal static bool ShouldInspectTrash(bool inspection, bool lazyInspector)
        {
            return inspection && !lazyInspector;
        }

        internal static bool IsGunsmithOrder(bool gunsmithIntent, bool hasOrder)
        {
            return gunsmithIntent && hasOrder;
        }

        internal static bool MatchesGunsmithWeapon(bool serviceOrder, bool advancedGun, bool caliberAccepted)
        {
            return !serviceOrder && advancedGun && caliberAccepted;
        }

        internal static bool IsGunsmithRequirementPending(bool requirementMet)
        {
            return !requirementMet;
        }

        internal static bool MatchesGunsmithStatPart(bool weaponMod, bool mustBeLarger, int percent, int flat)
        {
            if (!weaponMod) return false;
            return mustBeLarger ? percent > 0 || flat > 0 : percent < 0 || flat < 0;
        }

        internal static bool MatchesGunsmithSizePart(bool weaponMod, bool mustBeLarger)
        {
            return weaponMod && mustBeLarger;
        }

        internal static bool MatchesGunsmithSuppressor(bool weaponMod, bool suppressor, int level, int required)
        {
            return weaponMod && suppressor && level > required;
        }

        internal static float BorderWidth(int gridSize)
        {
            return Math.Max(1f, gridSize / 16f);
        }
    }
}
