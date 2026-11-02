using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NextOpeningVisitors
{
    [HarmonyPatch(typeof(GameItemElement), nameof(GameItemElement.Validate))]
    internal static class DemandHighlightValidatePatch
    {
        private static void Postfix(GameItemElement __instance)
        {
            try { DemandHighlightUI.Register(__instance); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not register an item highlight. " + exception.Message);
            }
        }
    }

    internal static class DemandHighlightUI
    {
        private static readonly Dictionary<GameItemElement, DemandHighlightFrame> Frames =
            new Dictionary<GameItemElement, DemandHighlightFrame>();
        private static readonly List<GameItemElement> PruneBuffer = new List<GameItemElement>();
        private static readonly List<GameItemElement> ScanBuffer = new List<GameItemElement>();
        private static readonly HashSet<GameInventory> RootSet = new HashSet<GameInventory>();
        private static DemandHighlightHost? _host;
        private static bool _enabled;

        internal static void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled)
            {
                Shutdown();
                return;
            }
            EnsureHost();
        }

        internal static void Register(GameItemElement element)
        {
            if (!_enabled || element == null || element.middle == null || Frames.ContainsKey(element)) return;
            Frames.Add(element, new DemandHighlightFrame(element));
        }

        internal static void RefreshNow()
        {
            if (!_enabled) return;
            PruneDestroyed();

            PlayerStore? store = PlayerStore.Instance;
            EmporiumEntry? emporium = EmporiumEntry.Instance;
            StoreClient? client = store?.currentClientInstance?.GetClientBlueprint();
            if (store == null || emporium == null || client == null)
            {
                HideAll();
                return;
            }

            bool inspection = client.clientIntent == StoreClient.ClientIntent.INSPECTION;
            bool gunsmith = DemandHighlightRules.IsGunsmithOrder(
                client.clientIntent == StoreClient.ClientIntent.GUNSMITH, client.gunOrderData != null);
            if (!inspection && !gunsmith && !client.IsClientBuying())
            {
                HideAll();
                return;
            }

            var levels = new Dictionary<GameItem, DemandHighlightLevel>();
            var visited = new HashSet<GameItem>();
            RootSet.Clear();
            AddRoot(emporium.invElement, inspection, true);
            AddRoot(emporium.frontInvinvElement, inspection, true);
            AddRoot(emporium.backInvinvElement, inspection, true);
            AddRoot(emporium.showcaseElement, inspection, true);
            AddRoot(emporium.hiddenElement, inspection, true);
            if (DemandHighlightRules.ShouldInspectTrash(inspection, client.isLazyInspector))
                AddRoot(emporium.trashInvElement, true, true);
            AddRoot(emporium.backInvinvElementCounter, inspection, false);
            AddRoot(emporium.bazarLeftinvElement, inspection, false);
            AddRoot(emporium.soldElement, inspection, false);

            foreach (GameInventory root in RootSet)
            {
                if (root == null || root.childItems == null) continue;
                if (inspection && root == emporium.hiddenElement &&
                    (StorePoster.Instance == null || !StorePoster.Instance.isPulled)) continue;
                foreach (GameItem item in root.childItems)
                    EvaluateTree(item, client, inspection, gunsmith, 0, visited, levels);
            }

            if (gunsmith)
            {
                // Printed guns can remain in a machine output inventory outside the shop roots.
                ScanBuffer.Clear();
                foreach (GameItemElement element in Frames.Keys)
                    if (element != null && !visited.Contains(element)) ScanBuffer.Add(element);
                foreach (GameItemElement element in ScanBuffer)
                {
                    if (element == null || visited.Contains(element) || !GeneralHelper.IsItemOwned(element)) continue;
                    EvaluateTree(element, client, false, true, 0, visited, levels);
                }
                ScanBuffer.Clear();
            }

            foreach (KeyValuePair<GameItemElement, DemandHighlightFrame> pair in Frames)
            {
                DemandHighlightLevel level = DemandHighlightLevel.None;
                if (pair.Key != null) levels.TryGetValue(pair.Key, out level);
                pair.Value.Apply(level);
            }
        }

        private static void AddRoot(GameInventory? root, bool inspection, bool nativeInspectionRoot)
        {
            if (root != null && (!inspection || nativeInspectionRoot)) RootSet.Add(root);
        }

        private static DemandHighlightLevel EvaluateTree(GameItem? item, StoreClient client, bool inspection, bool gunsmith,
            int depth, HashSet<GameItem> visited, Dictionary<GameItem, DemandHighlightLevel> levels)
        {
            if (item == null || depth > DemandHighlightRules.MaximumDepth || !visited.Add(item))
                return DemandHighlightLevel.None;

            bool behindLock = item.IsTag("ITEM_BEHIND_LOCK_TAG");
            bool blocksSearch = inspection && ContrabandHelper.IsBlockSearchContainer(item);
            if (behindLock || blocksSearch || item.unitCount <= 0)
            {
                levels[item] = DemandHighlightLevel.None;
                return DemandHighlightLevel.None;
            }

            bool matches = inspection
                ? IsInspectable(item)
                : gunsmith ? IsGunsmithDemand(client, item) : client.IsClientBuyingThisItem(item, true);
            DemandHighlightLevel result = DemandHighlightRules.Classify(matches,
                StolenHelper.IsStolenItem(item), ContrabandHelper.IsContraband(item));

            bool lockedContainer = LockHelper.IsCurrentlyLocked(item);
            if (DemandHighlightRules.CanDescend(inspection, behindLock, lockedContainer, blocksSearch))
            {
                foreach (GameItem child in GraphUtils.FindChildrenType<GameItem>(item))
                    result = DemandHighlightRules.Merge(result,
                        EvaluateTree(child, client, inspection, gunsmith, depth + 1, visited, levels));
            }

            levels[item] = result;
            if (item is GameItemElement element) Register(element);
            return result;
        }

        private static bool IsGunsmithDemand(StoreClient client, GameItem item)
        {
            return IsGunsmithWeapon(client, item) || IsGunsmithPart(client, item);
        }

        private static bool IsGunsmithWeapon(StoreClient client, GameItem item)
        {
            GunOrderData? order = client.gunOrderData;
            if (order == null || order.isServiceOrder || !item.IsTag("ADV_GUN_TAG")) return false;
            bool caliberAccepted = true;
            if (order.mandatoryRequirements != null)
            {
                foreach (GunOrderRequirement requirement in order.mandatoryRequirements)
                {
                    if (requirement != null && requirement.isCaliber && !requirement.IsMet(item))
                    {
                        caliberAccepted = false;
                        break;
                    }
                }
            }
            return DemandHighlightRules.MatchesGunsmithWeapon(false, true, caliberAccepted);
        }

        private static bool IsGunsmithPart(StoreClient client, GameItem item)
        {
            GunOrderData? order = client.gunOrderData;
            if (order == null || !item.IsTag("WEAPON_MOD_TAG")) return false;
            GameItem? orderGun = GunOrderData.ResolveOrderGun();
            return MatchesAnyRequirement(item, order.mandatoryRequirements, orderGun) ||
                MatchesAnyRequirement(item, order.bonusRequirements, orderGun);
        }

        private static bool MatchesAnyRequirement(GameItem item, List<GunOrderRequirement>? requirements,
            GameItem? orderGun)
        {
            if (requirements == null) return false;
            foreach (GunOrderRequirement requirement in requirements)
            {
                if (requirement == null) continue;
                bool requirementMet = orderGun != null && requirement.IsMet(orderGun);
                if (!DemandHighlightRules.IsGunsmithRequirementPending(requirementMet)) continue;
                if (MatchesRequirement(item, requirement)) return true;
            }
            return false;
        }

        private static bool MatchesRequirement(GameItem item, GunOrderRequirement requirement)
        {
            if (requirement.id == "GUN_SIZE_TAG")
                return DemandHighlightRules.MatchesGunsmithSizePart(true, requirement.isMustBeLargerThan);
            if (requirement.id == "GUN_SUPPRESSED_TAG")
            {
                int level = item.IsGameItemType("MAKESHIFT") ? 1 : 2;
                return DemandHighlightRules.MatchesGunsmithSuppressor(true,
                    item.IsTag("MOD_SUPPRESSOR_TAG"), level, requirement.value);
            }
            if (!TryGetPartStatTags(requirement.id, out string percentTag, out string flatTag)) return false;
            int percent = item.GetTagReadonly(percentTag).GetInt();
            int flat = item.GetTagReadonly(flatTag).GetInt();
            return DemandHighlightRules.MatchesGunsmithStatPart(true,
                requirement.isMustBeLargerThan, percent, flat);
        }

        private static bool TryGetPartStatTags(string requirementId, out string percentTag, out string flatTag)
        {
            switch (requirementId)
            {
                case "GUN_CURRENT_FIREPOWER_TAG":
                    percentTag = "GUN_FIREPOWER_TAG";
                    flatTag = "MOD_BASE_FIREPOWER_TAG";
                    return true;
                case "GUN_CURRENT_STYLE_TAG":
                    percentTag = "GUN_STYLE_TAG";
                    flatTag = "MOD_BASE_STYLE_TAG";
                    return true;
                case "GUN_CURRENT_RELIABILITY_TAG":
                    percentTag = "GUN_RELIABILITY_TAG";
                    flatTag = "MOD_BASE_RELIABILITY_TAG";
                    return true;
                case "GUN_CURRENT_RECOIL_TAG":
                    percentTag = "GUN_RECOIL_TAG";
                    flatTag = "MOD_BASE_RECOIL_TAG";
                    return true;
                case "GUN_CURRENT_ERGONOMICS_TAG":
                    percentTag = "GUN_ERGONOMICS_TAG";
                    flatTag = "MOD_BASE_ERGONOMICS_TAG";
                    return true;
                case "GUN_CURRENT_ACCURACY_TAG":
                    percentTag = "GUN_ACCURACY_TAG";
                    flatTag = "MOD_BASE_ACCURACY_TAG";
                    return true;
                default:
                    percentTag = "";
                    flatTag = "";
                    return false;
            }
        }

        private static bool IsInspectable(GameItem item)
        {
            // Same read-only tag checks as 0.46D InspectItem, without its per-refresh Debug.Log.
            if (item.IsTag("ITEM_BEHIND_LOCK_TAG") || item.IsTag("ITEM_HIDDEN_TAG") || item.IsTag("LOCKED_TAG"))
                return false;
            bool stolen = item.IsTag("STOLEN_TAG");
            bool contraband = item.IsTag("CONTRABAND_ITEM_TAG");
            if (!stolen && !contraband) return false;
            return stolen || item.GetTagReadonly("CONTRABAND_LEVEL").GetString() != "CONTRABAND_LEVEL_NONE";
        }

        private static void EnsureHost()
        {
            if (_host != null) return;
            var hostObject = new GameObject("NextOpeningVisitors.DemandHighlightHost");
            UnityEngine.Object.DontDestroyOnLoad(hostObject);
            _host = hostObject.AddComponent<DemandHighlightHost>();
        }

        private static void PruneDestroyed()
        {
            PruneBuffer.Clear();
            foreach (KeyValuePair<GameItemElement, DemandHighlightFrame> pair in Frames)
            {
                if (pair.Key == null || pair.Key.IsDestroyed())
                {
                    pair.Value.Destroy();
                    PruneBuffer.Add(pair.Key!);
                }
            }
            foreach (GameItemElement element in PruneBuffer) Frames.Remove(element);
            PruneBuffer.Clear();
        }

        private static void HideAll()
        {
            foreach (DemandHighlightFrame frame in Frames.Values) frame.Apply(DemandHighlightLevel.None);
        }

        private static void Shutdown()
        {
            if (_host != null) UnityEngine.Object.Destroy(_host.gameObject);
            _host = null;
            foreach (DemandHighlightFrame frame in Frames.Values) frame.Destroy();
            Frames.Clear();
            RootSet.Clear();
            PruneBuffer.Clear();
            ScanBuffer.Clear();
        }
    }

    internal sealed class DemandHighlightHost : MonoBehaviour
    {
        private float _timer = 0.25f;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < 0.25f) return;
            _timer = 0f;
            try { DemandHighlightUI.RefreshNow(); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not refresh item highlights. " + exception.Message);
            }
        }
    }

    internal sealed class DemandHighlightFrame
    {
        private readonly GameItemElement _element;
        private readonly TreeNodeRender[] _real = new TreeNodeRender[4];
        private readonly TreeNodeRender[] _fake = new TreeNodeRender[4];
        private bool _created;

        internal DemandHighlightFrame(GameItemElement element)
        {
            _element = element;
        }

        internal void Apply(DemandHighlightLevel level)
        {
            if (_element == null || _element.middle == null) return;
            if (level != DemandHighlightLevel.None) EnsureCreated();
            if (!_created) return;

            Color color = GetColor(level);
            float width = DemandHighlightRules.BorderWidth(UISettings.current.gridSize);
            bool visible = level != DemandHighlightLevel.None && _element.unitCount > 0;
            bool fakeVisible = visible && _element.fakeBackground != null && _element.fakeBackground.image.enabled;
            for (int i = 0; i < 4; i++)
            {
                ApplyNode(_real[i], color, width, visible);
                ApplyNode(_fake[i], color, width, fakeVisible);
            }
        }

        private void EnsureCreated()
        {
            if (_created) return;
            for (int i = 0; i < 4; i++)
            {
                // Both placed items and drag previews rotate these nodes, so the frame follows them.
                _real[i] = CreateNode(_element.rotator, i);
                _fake[i] = CreateNode(_element.fakeRotator, i);
            }
            _created = true;
        }

        private static TreeNodeRender CreateNode(TreeNodeRender parent, int edge)
        {
            TreeNodeRender node = TreeNodeRender.Instantiate().SetParent(parent);
            node.gameObject.name = "NextOpeningVisitors.DemandHighlight." + edge;
            node.image.sprite = null;
            node.image.raycastTarget = false;
            node.image.enabled = false;
            RectTransform rect = node.rectTransform;
            switch (edge)
            {
                case 0:
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    break;
                case 1:
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    break;
                case 2:
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 0.5f);
                    break;
                default:
                    rect.anchorMin = new Vector2(1f, 0f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 0.5f);
                    break;
            }
            rect.anchoredPosition = Vector2.zero;
            rect.SetAsLastSibling();
            return node;
        }

        private static void ApplyNode(TreeNodeRender node, Color color, float width, bool visible)
        {
            if (node == null) return;
            int edge = node.gameObject.name.EndsWith(".0", StringComparison.Ordinal) ? 0
                : node.gameObject.name.EndsWith(".1", StringComparison.Ordinal) ? 1
                : node.gameObject.name.EndsWith(".2", StringComparison.Ordinal) ? 2 : 3;
            node.rectTransform.sizeDelta = edge < 2
                ? new Vector2(0f, width)
                : new Vector2(width, edge == 2 || edge == 3 ? -2f * width : 0f);
            node.image.color = color;
            node.image.enabled = visible;
        }

        private static Color GetColor(DemandHighlightLevel level)
        {
            switch (level)
            {
                case DemandHighlightLevel.Contraband: return new Color32(238, 74, 74, 245);
                case DemandHighlightLevel.Stolen: return new Color32(245, 184, 65, 245);
                case DemandHighlightLevel.Demand: return new Color32(70, 220, 110, 245);
                default: return Color.clear;
            }
        }

        internal void Destroy()
        {
            DestroyNodes(_real);
            DestroyNodes(_fake);
            _created = false;
        }

        private static void DestroyNodes(TreeNodeRender[] nodes)
        {
            foreach (TreeNodeRender node in nodes)
                if (node != null) UnityEngine.Object.Destroy(node.gameObject);
        }
    }
}
