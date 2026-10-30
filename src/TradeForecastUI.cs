using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace NextOpeningVisitors
{
    [HarmonyPatch(typeof(NegociationUIManager), "Rebuild")]
    internal static class TradeForecastRebuildPatch
    {
        private static void Prefix() => TradeForecastUI.Invalidate();
    }

    [HarmonyPatch(typeof(NegociationUIManager), "RefreshNow")]
    internal static class TradeForecastRefreshPatch
    {
        private static void Postfix(NegociationUIManager __instance)
        {
            try { TradeForecastUI.Refresh(__instance); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not refresh the trade crime forecast. " + exception.Message);
                TradeForecastUI.Hide();
            }
        }
    }

    internal static class TradeForecastUI
    {
        private const float AddedHeight = 54f;
        private static readonly Func<NegociationUIManager, NegotiationTabletFrame?> FrameRef = manager =>
            AccessTools.Field(typeof(NegociationUIManager), "frame")?.GetValue(manager) as NegotiationTabletFrame;

        private static NegotiationTabletFrame? _frame;
        private static TextMeshProUGUI? _evidenceLine;
        private static TextMeshProUGUI? _crimeLine;
        private static float _baseInfoHeight;
        private static bool _baseSeparator;
        private static bool _extended;
        private static bool _enabled;

        internal static void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled) Shutdown();
        }

        internal static void Invalidate()
        {
            DestroyLines();
            _frame = null;
            _extended = false;
        }

        internal static void Shutdown()
        {
            if (_extended && _frame != null) RestoreLayout(_frame);
            Invalidate();
        }

        internal static void Hide()
        {
            if (_extended && _frame != null) RestoreLayout(_frame);
            DestroyLines();
            _extended = false;
        }

        internal static void Refresh(NegociationUIManager manager)
        {
            if (!_enabled)
            {
                Hide();
                return;
            }

            NegotiationTabletFrame? frame = FrameRef(manager);
            if (frame == null || frame.infoHost == null) return;
            if (_frame != frame)
            {
                Shutdown();
                _frame = frame;
                CaptureLayout(frame);
            }
            else if (!_extended)
            {
                CaptureLayout(frame);
            }

            TradeForecastResult? forecast = CrimeForecastAdapter.ForecastCurrentTrade(manager);
            if (forecast == null)
            {
                Hide();
                return;
            }

            EnsureLines(frame);
            string locale = LocHelper.GetCurrentLocaleCode();
            _evidenceLine!.text = CrimeForecastLocalization.Get(
                CrimeForecastLocalization.TradeEvidence, locale, forecast.EvidenceLevel);
            _crimeLine!.text = CrimeForecastLocalization.Get(
                CrimeForecastLocalization.TradeCrime, locale,
                forecast.CurrentCrimeTotal, forecast.ResultingCrimeTotal, forecast.AddedCrimeTotal);
            _crimeLine.color = forecast.AddedCrimeTotal > 0 ? TabletStyle.RED : TabletStyle.INK;

            TabletUI.LayoutSection(frame, _baseInfoHeight + AddedHeight, _baseSeparator, 432f);
            _extended = true;
        }

        private static void CaptureLayout(NegotiationTabletFrame frame)
        {
            _baseInfoHeight = frame.infoHost.sizeDelta.y;
            _baseSeparator = frame.separatorTop != null && frame.separatorTop.gameObject.activeSelf;
        }

        private static void EnsureLines(NegotiationTabletFrame frame)
        {
            if (_evidenceLine == null || _crimeLine == null)
            {
                DestroyLines();
                float y = _baseInfoHeight + 8f;
                _evidenceLine = TabletUI.NewText(frame, frame.infoHost,
                    "NextOpeningVisitors.TradeEvidence", 24f, y, 482f, 23f,
                    FontLoader.FontRole.Noto_Sans, 16f, TabletStyle.INK, TextAlignmentOptions.MidlineLeft);
                _crimeLine = TabletUI.NewText(frame, frame.infoHost,
                    "NextOpeningVisitors.TradeCrime", 24f, y + 23f, 482f, 23f,
                    FontLoader.FontRole.Noto_Sans, 17f, TabletStyle.INK, TextAlignmentOptions.MidlineLeft);
                TabletUI.AutoSize(_evidenceLine);
                TabletUI.AutoSize(_crimeLine);
            }
        }

        private static void RestoreLayout(NegotiationTabletFrame frame)
        {
            TabletUI.LayoutSection(frame, _baseInfoHeight, _baseSeparator, 432f);
        }

        private static void DestroyLines()
        {
            if (_evidenceLine != null) UnityEngine.Object.Destroy(_evidenceLine.gameObject);
            if (_crimeLine != null) UnityEngine.Object.Destroy(_crimeLine.gameObject);
            _evidenceLine = null;
            _crimeLine = null;
        }
    }
}
