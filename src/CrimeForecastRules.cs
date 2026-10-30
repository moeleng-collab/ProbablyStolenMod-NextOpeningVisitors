using System;
using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal static class CrimeForecastRules
    {
        private static readonly int[] FixerPercent = { 15, 20, 25 };
        private static readonly int[] FixerFlat = { 50, 150, 400 };

        internal static TradeForecastResult ForecastTrade(TradeForecastInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var result = new TradeForecastResult
            {
                EvidenceLevel = input.EvidenceLevel,
                CurrentCrimeTotal = Sum(input.CurrentCrimeAmounts)
            };

            if (!input.ClientDoesNotContributeToEvidence && input.Items != null)
            {
                foreach (TradeItemForecastInput item in input.Items)
                {
                    if (item.IsStolen)
                    {
                        int value = MultiplyAndRound(item.NegotiatedValue, item.StolenHeat / 100f);
                        value = ApplyFactionModifier(value, input.ClientFaction, input.LowerLevelTightLips);
                        value = ApplyStartingPerks(value, input.Intimidating, input.Blabbermouth);
                        AddCrime(result.CrimeIncreases, SecData.FENCING, value);
                    }

                    if (item.ContrabandKind != TradeContrabandKind.None)
                    {
                        int value = ApplyFactionModifier(item.NegotiatedValue, input.ClientFaction, input.LowerLevelTightLips);
                        value = ApplyStartingPerks(value, input.Intimidating, input.Blabbermouth);
                        AddCrime(result.CrimeIncreases, GetContrabandCrimeId(item.ContrabandKind), value);
                    }
                }
            }

            foreach (int amount in result.CrimeIncreases.Values) result.AddedCrimeTotal += amount;
            result.ResultingCrimeTotal = result.CurrentCrimeTotal + result.AddedCrimeTotal;
            return result;
        }

        internal static NightForecastResult ForecastNight(NightForecastInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var afterFixer = new List<int>();
            if (input.CurrentCrimeAmounts != null)
            {
                int tier = Math.Max(1, Math.Min(3, input.FixerTier));
                float percent = FixerPercent[tier - 1] / 100f;
                int flat = FixerFlat[tier - 1];
                foreach (int current in input.CurrentCrimeAmounts)
                {
                    int amount = current;
                    if (input.UseFixer)
                    {
                        if (amount > 0) amount -= (int)(amount * percent);
                        else amount = 0;
                        amount -= flat;
                    }
                    afterFixer.Add(Math.Max(0, amount));
                }
            }

            var afterDecay = new List<int>(afterFixer.Count);
            foreach (int current in afterFixer)
            {
                int amount = current;
                if (input.EvidenceLevel < 80) amount -= 20;
                afterDecay.Add(Math.Max(0, amount));
            }

            int baseDelta = GetBaseInvestigationDelta(Sum(afterDecay));
            int delta = baseDelta;
            if (input.SecTrusted) delta /= 2;
            if (input.SecPriority) delta = MultiplyAndRound(delta, 1.5);
            else if (input.SecWatchlist) delta = MultiplyAndRound(delta, 1.2000000476837158);
            if (input.UlPetition) delta = MultiplyAndRound(delta, 1.5);
            int nextEvidence = Math.Min(100, input.EvidenceLevel + delta);

            return new NightForecastResult
            {
                CurrentEvidenceLevel = input.EvidenceLevel,
                CurrentCrimeTotal = Sum(input.CurrentCrimeAmounts),
                AfterFixerCrimeTotal = Sum(afterFixer),
                AfterDecayCrimeTotal = Sum(afterDecay),
                BaseEvidenceDelta = baseDelta,
                EvidenceDelta = delta,
                NextEvidenceLevel = nextEvidence,
                ReachesInvestigationCap = nextEvidence >= 100,
                AfterFixerAmounts = afterFixer,
                AfterDecayAmounts = afterDecay
            };
        }

        internal static int GetBaseInvestigationDelta(int totalCrimeValue)
        {
            if (totalCrimeValue < 100) return 0;
            if (totalCrimeValue < 300) return 2;
            if (totalCrimeValue < 850) return 6;
            if (totalCrimeValue < 1200) return 9;
            if (totalCrimeValue < 1500) return 12;
            if (totalCrimeValue < 2500) return 20;
            return 33;
        }

        internal static int MultiplyAndRound(int value, double multiplier)
        {
            return (int)Math.Round(value * multiplier);
        }

        private static int ApplyFactionModifier(int value, string faction, bool tightLips)
        {
            switch (faction)
            {
                case "FACTION_BLACK_MARKET": return value / 2;
                case "FACTION_REVOLUTION": return value / 3;
                case "FACTION_LOWER_LEVEL": return tightLips ? value / 2 : value;
                case "FACTION_CARTEL": return 0;
                default: return value;
            }
        }

        private static int ApplyStartingPerks(int value, bool intimidating, bool blabbermouth)
        {
            if (intimidating) value = MultiplyAndRound(value, 0.6699999570846558);
            if (blabbermouth) value = MultiplyAndRound(value, 1.3300000429153442);
            return value;
        }

        private static string GetContrabandCrimeId(TradeContrabandKind kind)
        {
            switch (kind)
            {
                case TradeContrabandKind.Substance: return SecData.DRUG_TRAFFICKING;
                case TradeContrabandKind.Weapon: return SecData.WEAPON_TRAFFICKING;
                case TradeContrabandKind.AccessCard: return SecData.CARD_TRAFFICKING;
                default: return SecData.CONTRABAND_TRAFFICKING;
            }
        }

        private static void AddCrime(IDictionary<string, int> target, string crimeId, int amount)
        {
            target.TryGetValue(crimeId, out int current);
            target[crimeId] = current + amount;
        }

        private static int Sum(IEnumerable<int>? values)
        {
            int total = 0;
            if (values != null)
            {
                foreach (int value in values) total += value;
            }
            return total;
        }
    }
}
