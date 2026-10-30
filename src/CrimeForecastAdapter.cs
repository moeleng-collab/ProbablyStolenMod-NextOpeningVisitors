using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal static class CrimeForecastAdapter
    {
        internal static TradeForecastResult? ForecastCurrentTrade(NegociationUIManager manager)
        {
            PlayerStore? store = PlayerStore.Instance;
            if (store?.secData == null || store.currentClientInstance == null || manager == null) return null;
            StoreClient? client = store.currentClientInstance.GetClientBlueprint();
            if (client == null) return null;

            List<GameItem> sellable = manager.GetSellableItem();
            if (sellable == null || sellable.Count == 0) return null;

            var input = new TradeForecastInput
            {
                EvidenceLevel = store.secData.evidenceLevel,
                ClientFaction = client.clientFaction ?? "",
                ClientDoesNotContributeToEvidence = client.isNoContributeToEvidence,
                LowerLevelTightLips = StoreReputation.IsPerkUnlocked("LLTIGHTLIPS"),
                Intimidating = StartingPerk.IsIntimidating(),
                Blabbermouth = StartingPerk.IsBlabbermouth()
            };
            CopyAmounts(store.secData.crimeAmount, input.CurrentCrimeAmounts);

            foreach (GameItem item in sellable)
            {
                if (item == null) continue;
                AddItem(input.Items, item);
                List<GameItem> children = item.FindAllChildItems();
                if (children == null) continue;
                foreach (GameItem child in children)
                {
                    if (child != null && !child.IsTag("ITEM_BEHIND_LOCK_TAG")) AddItem(input.Items, child);
                }
            }
            return CrimeForecastRules.ForecastTrade(input);
        }

        internal static NightForecastResult? ForecastCurrentNight()
        {
            PlayerStore? store = PlayerStore.Instance;
            if (store?.secData == null) return null;
            var input = new NightForecastInput
            {
                EvidenceLevel = store.secData.evidenceLevel,
                UseFixer = StoreService.GetWildFixerService()?.isUsing ?? false,
                FixerTier = NetworkUpgrade.GetWildFixerTier(),
                SecTrusted = StoreReputation.IsPerkUnlocked("SEC_TRUSTED"),
                SecPriority = StoreReputation.IsPerkUnlocked("SEC_PRIORITY"),
                SecWatchlist = StoreReputation.IsPerkUnlocked("SEC_WATCHLIST"),
                UlPetition = StoreReputation.IsPerkUnlocked("UL_PETITION")
            };
            CopyAmounts(store.secData.crimeAmount, input.CurrentCrimeAmounts);
            return CrimeForecastRules.ForecastNight(input);
        }

        private static void AddItem(ICollection<TradeItemForecastInput> target, GameItem item)
        {
            target.Add(new TradeItemForecastInput
            {
                NegotiatedValue = (int)item.GetNegociatedValue(),
                IsStolen = item.IsTag("STOLEN_TAG"),
                StolenHeat = item.IsTag("STOLEN_TAG") ? item.GetTagReadonly("STOLEN_VALUE_INT").GetInt() : 0,
                ContrabandKind = GetContrabandKind(item)
            });
        }

        private static TradeContrabandKind GetContrabandKind(GameItem item)
        {
            if (!item.IsTag("CONTRABAND_ITEM_TAG")) return TradeContrabandKind.None;
            if (item.IsGameItemType("SUBSTANCE")) return TradeContrabandKind.Substance;
            if (item.IsGameItemType("WEAPON")) return TradeContrabandKind.Weapon;
            if (item.IsGameItemType("ACCESS_CARD")) return TradeContrabandKind.AccessCard;
            return TradeContrabandKind.Other;
        }

        private static void CopyAmounts(IEnumerable<int>? source, ICollection<int> target)
        {
            if (source == null) return;
            foreach (int amount in source) target.Add(amount);
        }
    }
}
