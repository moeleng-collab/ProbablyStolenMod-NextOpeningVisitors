using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace NextOpeningVisitors
{
    [HarmonyPatch(typeof(EndOfDayUIManager), nameof(EndOfDayUIManager.InitUI))]
    internal static class NightForecastInitPatch
    {
        private static void Prefix() => NightForecastUI.Invalidate();
    }

    [HarmonyPatch(typeof(EndOfDayUIManager), nameof(EndOfDayUIManager.RefreshUI))]
    internal static class NightForecastRefreshPatch
    {
        private static void Postfix(EndOfDayUIManager __instance)
        {
            try { NightForecastUI.Refresh(__instance); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not refresh the nightly investigation forecast. " + exception.Message);
                NightForecastUI.Hide();
            }
        }
    }

    internal static class NightForecastUI
    {
        private const float AddedHeight = 86f;
        private static readonly Func<EndOfDayUIManager, EndOfDayTabletFrame?> FrameRef = manager =>
            AccessTools.Field(typeof(EndOfDayUIManager), "frame")?.GetValue(manager) as EndOfDayTabletFrame;

        private static EndOfDayTabletFrame? _frame;
        private static TextMeshProUGUI? _crimeLine;
        private static TextMeshProUGUI? _deltaLine;
        private static TextMeshProUGUI? _evidenceLine;
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

        internal static void Refresh(EndOfDayUIManager manager)
        {
            if (!_enabled)
            {
                Hide();
                return;
            }

            EndOfDayTabletFrame? frame = FrameRef(manager);
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

            NightForecastResult? forecast = CrimeForecastAdapter.ForecastCurrentNight();
            if (forecast == null)
            {
                Hide();
                return;
            }

            EnsureLines(frame);
            string locale = LocHelper.GetCurrentLocaleCode();
            _crimeLine!.text = CrimeForecastLocalization.Get(
                CrimeForecastLocalization.NightCrime, locale,
                forecast.CurrentCrimeTotal, forecast.AfterDecayCrimeTotal);
            _deltaLine!.text = CrimeForecastLocalization.Get(
                CrimeForecastLocalization.NightDelta, locale, forecast.EvidenceDelta);
            _evidenceLine!.text = CrimeForecastLocalization.Get(
                CrimeForecastLocalization.NightEvidence, locale,
                forecast.CurrentEvidenceLevel, forecast.NextEvidenceLevel);
            if (forecast.ReachesInvestigationCap)
            {
                _evidenceLine.text += CrimeForecastLocalization.Get(CrimeForecastLocalization.CapWarning, locale);
                _evidenceLine.color = TabletStyle.RED;
            }
            else
            {
                _evidenceLine.color = TabletStyle.INK;
            }

            TabletUI.LayoutSection(frame, _baseInfoHeight + AddedHeight, _baseSeparator, 500f);
            _extended = true;
        }

        private static void CaptureLayout(EndOfDayTabletFrame frame)
        {
            _baseInfoHeight = frame.infoHost.sizeDelta.y;
            _baseSeparator = frame.separatorTop != null && frame.separatorTop.gameObject.activeSelf;
        }

        private static void EnsureLines(EndOfDayTabletFrame frame)
        {
            if (_crimeLine != null && _deltaLine != null && _evidenceLine != null) return;
            DestroyLines();
            float y = _baseInfoHeight + 8f;
            _crimeLine = NewLine(frame, "NightCrime", y);
            _deltaLine = NewLine(frame, "NightDelta", y + 26f);
            _evidenceLine = NewLine(frame, "NightEvidence", y + 52f);
        }

        private static TextMeshProUGUI NewLine(EndOfDayTabletFrame frame, string name, float y)
        {
            TextMeshProUGUI line = TabletUI.NewText(frame, frame.infoHost,
                "NextOpeningVisitors." + name, 24f, y, 482f, 23f,
                FontLoader.FontRole.Noto_Sans, 16f, TabletStyle.INK, TextAlignmentOptions.MidlineLeft);
            TabletUI.AutoSize(line);
            return line;
        }

        private static void RestoreLayout(EndOfDayTabletFrame frame)
        {
            TabletUI.LayoutSection(frame, _baseInfoHeight, _baseSeparator, 500f);
        }

        private static void DestroyLines()
        {
            if (_crimeLine != null) UnityEngine.Object.Destroy(_crimeLine.gameObject);
            if (_deltaLine != null) UnityEngine.Object.Destroy(_deltaLine.gameObject);
            if (_evidenceLine != null) UnityEngine.Object.Destroy(_evidenceLine.gameObject);
            _crimeLine = null;
            _deltaLine = null;
            _evidenceLine = null;
        }
    }
}
