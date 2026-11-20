using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NextOpeningVisitors
{
    [HarmonyPatch(typeof(GameItemElement), nameof(GameItemElement.Validate))]
    internal static class BerryQualityValidatePatch
    {
        private static void Postfix(GameItemElement __instance)
        {
            try { BerryQualityUI.Register(__instance); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not register a glowberry quality marker. " + exception.Message);
            }
        }
    }

    internal static class BerryQualityUI
    {
        private const string SaveKey = "nextopeningvisitors.berry_appraisal.v1";
        private static readonly Dictionary<GameItemElement, BerryQualityMarkerPair> Markers =
            new Dictionary<GameItemElement, BerryQualityMarkerPair>();
        private static readonly List<GameItemElement> PruneBuffer = new List<GameItemElement>();
        private static BerryAppraisalState _state = new BerryAppraisalState();
        private static BerryQualityHost? _host;
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
            LoadState();
            RegisterExistingItems();
            RefreshNow();
        }

        internal static void OnGameLoaded()
        {
            if (!_enabled) return;
            LoadState();
            RegisterExistingItems();
            RefreshNow();
        }

        private static void RegisterExistingItems()
        {
            PlayerStore? store = PlayerStore.HasInstance ? PlayerStore.Instance : null;
            if (store == null || EmporiumEntry.Instance == null) return;
            foreach (GameItem item in store.FindAllItem(false))
                if (item is GameItemElement element) Register(element);
        }

        internal static void Register(GameItemElement element)
        {
            if (!_enabled || element == null || element.middle == null || Markers.ContainsKey(element)) return;
            var marker = new BerryQualityMarkerPair(element);
            Markers.Add(element, marker);
            marker.Apply(GetStyle(element));
        }

        internal static void MarkAppraised(GameItem? item)
        {
            if (!_enabled || item == null || !item.IsTag("BERRY_TAG") || item.uniqueId <= 0) return;
            BerryQuality quality = AppraisalItemDetails.GetBerryQuality(item);
            if (quality == BerryQuality.Unknown || !_state.Mark(item.uniqueId, quality)) return;
            SaveState();
            RefreshNow();
        }

        internal static void RefreshNow()
        {
            if (!_enabled) return;
            PruneDestroyed();
            foreach (KeyValuePair<GameItemElement, BerryQualityMarkerPair> pair in Markers)
                pair.Value.Apply(GetStyle(pair.Key));
        }

        private static BerryMarkerStyle GetStyle(GameItemElement? element)
        {
            if (element == null || !element.IsTag("BERRY_TAG") ||
                !_state.TryGetQuality(element.uniqueId, out BerryQuality quality))
                return BerryMarkerStyle.None;
            return AppraisalAssistantRules.GetBerryMarkerStyle(quality);
        }

        private static void LoadState()
        {
            _state = new BerryAppraisalState();
            PlayerStore? store = PlayerStore.HasInstance ? PlayerStore.Instance : null;
            if (store == null) return;
            if (store.ModData().TryGetValue(SaveKey, out string? serialized) &&
                !BerryAppraisalState.TryDeserialize(serialized, out _state))
            {
                _state = new BerryAppraisalState();
                NextOpeningVisitorsMod.Log?.Warning(
                    "Invalid glowberry appraisal data; quality markers were hidden without changing item data.");
            }
        }

        private static void SaveState()
        {
            PlayerStore? store = PlayerStore.HasInstance ? PlayerStore.Instance : null;
            if (store == null)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not save glowberry appraisal data because the store is unavailable.");
                return;
            }
            store.ModData()[SaveKey] = _state.Serialize();
        }

        private static void EnsureHost()
        {
            if (_host != null) return;
            var hostObject = new GameObject("NextOpeningVisitors.BerryQualityHost");
            UnityEngine.Object.DontDestroyOnLoad(hostObject);
            _host = hostObject.AddComponent<BerryQualityHost>();
        }

        private static void PruneDestroyed()
        {
            PruneBuffer.Clear();
            foreach (KeyValuePair<GameItemElement, BerryQualityMarkerPair> pair in Markers)
            {
                if (pair.Key == null || pair.Key.IsDestroyed())
                {
                    pair.Value.Destroy();
                    PruneBuffer.Add(pair.Key!);
                }
            }
            foreach (GameItemElement element in PruneBuffer) Markers.Remove(element);
            PruneBuffer.Clear();
        }

        private static void Shutdown()
        {
            if (_host != null) UnityEngine.Object.Destroy(_host.gameObject);
            _host = null;
            foreach (BerryQualityMarkerPair marker in Markers.Values) marker.Destroy();
            Markers.Clear();
            PruneBuffer.Clear();
            _state = new BerryAppraisalState();
        }
    }

    internal sealed class BerryQualityHost : MonoBehaviour
    {
        private float _timer = 0.2f;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < 0.2f) return;
            _timer = 0f;
            try { BerryQualityUI.RefreshNow(); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not refresh glowberry quality markers. " + exception.Message);
            }
        }
    }

    internal sealed class BerryQualityMarkerPair
    {
        private readonly GameItemElement _element;
        private BerryQualityMarker? _real;
        private BerryQualityMarker? _fake;

        internal BerryQualityMarkerPair(GameItemElement element)
        {
            _element = element;
        }

        internal void Apply(BerryMarkerStyle style)
        {
            if (_element == null || _element.middle == null) return;
            if (style != BerryMarkerStyle.None) EnsureCreated();
            bool identified = style != BerryMarkerStyle.None && _element.unitCount > 0 && !_element.isDebugMenu;
            bool realVisible = identified && _element.background != null && _element.background.image.enabled;
            bool fakeVisible = identified && _element.fakeBackground != null && _element.fakeBackground.image.enabled;
            _real?.Apply(style, realVisible);
            _fake?.Apply(style, fakeVisible);
        }

        private void EnsureCreated()
        {
            if (_real == null) _real = new BerryQualityMarker(_element.middle, false);
            if (_fake == null && _element.fakeMiddle != null)
                _fake = new BerryQualityMarker(_element.fakeMiddle, true);
        }

        internal void Destroy()
        {
            _real?.Destroy();
            _fake?.Destroy();
            _real = null;
            _fake = null;
        }
    }

    internal sealed class BerryQualityMarker
    {
        private static readonly Color32 Ink = new Color32(43, 35, 45, 255);
        private readonly TreeNodeRender _root;
        private readonly TreeNodeRender[] _lines = new TreeNodeRender[6];

        internal BerryQualityMarker(TreeNodeRender parent, bool fake)
        {
            _root = TreeNodeRender.Instantiate().SetParent(parent);
            _root.gameObject.name = fake
                ? "NextOpeningVisitors.BerryQuality.Fake"
                : "NextOpeningVisitors.BerryQuality";
            _root.image.sprite = null;
            _root.image.raycastTarget = false;
            RectTransform rect = _root.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            for (int i = 0; i < _lines.Length; i++)
            {
                _lines[i] = TreeNodeRender.Instantiate().SetParent(_root);
                _lines[i].gameObject.name = "Symbol" + i;
                _lines[i].image.sprite = null;
                _lines[i].image.color = Ink;
                _lines[i].image.raycastTarget = false;
                _lines[i].image.enabled = false;
                RectTransform lineRect = _lines[i].rectTransform;
                lineRect.anchorMin = new Vector2(0.5f, 0.5f);
                lineRect.anchorMax = new Vector2(0.5f, 0.5f);
                lineRect.pivot = new Vector2(0.5f, 0.5f);
            }
            _root.image.enabled = false;
        }

        internal void Apply(BerryMarkerStyle style, bool visible)
        {
            int px = Math.Max(1, UISettings.current.gridSize / 16);
            float unit = px;
            _root.rectTransform.sizeDelta = new Vector2(5f * unit + 2f, 5f * unit + 2f);
            _root.rectTransform.anchoredPosition = new Vector2(-2f * unit, -2f * unit);
            _root.rectTransform.SetAsLastSibling();
            _root.image.color = GetColor(style);
            _root.image.enabled = visible;
            foreach (TreeNodeRender line in _lines) line.image.enabled = false;
            if (!visible) return;

            switch (style)
            {
                case BerryMarkerStyle.Moldy:
                    ConfigureLine(0, new Vector2(3.2f * unit, 0.75f * unit), Vector2.zero, 45f);
                    ConfigureLine(1, new Vector2(3.2f * unit, 0.75f * unit), Vector2.zero, -45f);
                    break;
                case BerryMarkerStyle.Dry:
                    ConfigureLine(2, new Vector2(0.8f * unit, 2.2f * unit), new Vector2(0f, 0.65f * unit), 0f);
                    ConfigureLine(3, new Vector2(0.9f * unit, 0.9f * unit), new Vector2(0f, -1.35f * unit), 0f);
                    break;
                case BerryMarkerStyle.Normal:
                    ConfigureLine(4, new Vector2(2.8f * unit, 0.75f * unit), Vector2.zero, 0f);
                    break;
                case BerryMarkerStyle.Small:
                    ConfigureLine(4, new Vector2(2.8f * unit, 0.75f * unit), Vector2.zero, 0f);
                    ConfigureLine(5, new Vector2(0.75f * unit, 2.8f * unit), Vector2.zero, 0f);
                    break;
            }
        }

        private void ConfigureLine(int index, Vector2 size, Vector2 position, float rotation)
        {
            TreeNodeRender line = _lines[index];
            line.rectTransform.sizeDelta = size;
            line.rectTransform.anchoredPosition = position;
            line.rectTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
            line.image.enabled = true;
        }

        private static Color GetColor(BerryMarkerStyle style)
        {
            switch (style)
            {
                case BerryMarkerStyle.Moldy: return new Color32(238, 74, 80, 255);
                case BerryMarkerStyle.Dry: return new Color32(190, 112, 225, 255);
                case BerryMarkerStyle.Normal: return new Color32(245, 196, 75, 255);
                case BerryMarkerStyle.Small: return new Color32(61, 199, 126, 255);
                default: return Color.clear;
            }
        }

        internal void Destroy()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root.gameObject);
        }
    }
}
