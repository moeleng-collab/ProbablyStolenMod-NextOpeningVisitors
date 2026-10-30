using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal enum TradeContrabandKind
    {
        None,
        Substance,
        Weapon,
        AccessCard,
        Other
    }

    internal struct TradeItemForecastInput
    {
        internal int NegotiatedValue;
        internal bool IsStolen;
        internal int StolenHeat;
        internal TradeContrabandKind ContrabandKind;
    }

    internal sealed class TradeForecastInput
    {
        internal int EvidenceLevel;
        internal IList<int> CurrentCrimeAmounts = new List<int>();
        internal IList<TradeItemForecastInput> Items = new List<TradeItemForecastInput>();
        internal string ClientFaction = "";
        internal bool ClientDoesNotContributeToEvidence;
        internal bool LowerLevelTightLips;
        internal bool Intimidating;
        internal bool Blabbermouth;
    }

    internal sealed class TradeForecastResult
    {
        internal int EvidenceLevel;
        internal int CurrentCrimeTotal;
        internal int AddedCrimeTotal;
        internal int ResultingCrimeTotal;
        internal Dictionary<string, int> CrimeIncreases = new Dictionary<string, int>();
    }

    internal sealed class NightForecastInput
    {
        internal int EvidenceLevel;
        internal IList<int> CurrentCrimeAmounts = new List<int>();
        internal bool UseFixer;
        internal int FixerTier = 1;
        internal bool SecTrusted;
        internal bool SecPriority;
        internal bool SecWatchlist;
        internal bool UlPetition;
    }

    internal sealed class NightForecastResult
    {
        internal int CurrentEvidenceLevel;
        internal int CurrentCrimeTotal;
        internal int AfterFixerCrimeTotal;
        internal int AfterDecayCrimeTotal;
        internal int BaseEvidenceDelta;
        internal int EvidenceDelta;
        internal int NextEvidenceLevel;
        internal bool ReachesInvestigationCap;
        internal List<int> AfterFixerAmounts = new List<int>();
        internal List<int> AfterDecayAmounts = new List<int>();
    }
}
