using System;
using System.Collections.Generic;
using UnityEngine;

namespace NextOpeningVisitors
{
    internal sealed class FeatureSettingsInputHandler : EmptyActionHandler
    {
        private static readonly List<KeyCode> Keys = new List<KeyCode> { KeyCode.F8 };
        private bool _consume;

        public override List<KeyCode> keyListeners => Keys;

        protected override void OnEventPress(ISet<KeyCode> keysPressed)
        {
            _consume = FeatureSettingsUI.IsActive && keysPressed.Contains(KeyCode.F8);
            if (!_consume) return;
            NextOpeningVisitorsMod.Log?.Log("Feature list hotkey received: F8.");
            try { FeatureSettingsUI.Toggle(); }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not toggle the mod feature list. " + exception.Message);
            }
        }

        protected override bool OnEventUpdate()
        {
            bool consume = _consume;
            _consume = false;
            return consume;
        }
    }

    internal static class FeatureSettingsUI
    {
        private const string WindowId = "nextopeningvisitors.feature_settings";
        private static CustomUIWindow? _window;
        private static FeatureSettingsInputHandler? _inputHandler;
        private static bool _enabled;

        internal static bool IsActive => _enabled;

        internal static void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (enabled)
            {
                EnsureInputHandler();
                return;
            }
            Close();
            if (_inputHandler != null) UnityEngine.Object.Destroy(_inputHandler.gameObject);
            _inputHandler = null;
        }

        private static void EnsureInputHandler()
        {
            if (InputActionManager.current == null) return;
            if (_inputHandler == null)
            {
                var host = new GameObject("NextOpeningVisitors.FeatureSettingsInput");
                UnityEngine.Object.DontDestroyOnLoad(host);
                _inputHandler = host.AddComponent<FeatureSettingsInputHandler>();
            }
            _inputHandler.actionPriority = 10000;
            if (InputActionManager.current.Add(_inputHandler))
                NextOpeningVisitorsMod.Log?.Log("Feature list input registered: F8.");
        }

        internal static void Toggle()
        {
            if (!_enabled) return;
            if (_window != null && _window.IsAlive)
            {
                if (_window.IsVisible) Close();
                else _window.Show();
                return;
            }
            Open();
        }

        private static void Open()
        {
            if (CustomUIManager.Instance == null)
            {
                NextOpeningVisitorsMod.Log?.Warning("The mod feature list is unavailable until the native UI manager is ready.");
                return;
            }
            string locale = LocHelper.GetCurrentLocaleCode();
            CustomUIBuilder builder = CustomUIManager.Window(WindowId,
                FeatureSettingsLocalization.Get("ui.title", locale), "overlay");
            _window = builder.Handle;
            _window.onClose = () => _window = null;
            builder.SetSize(580f, 650f)
                .Center()
                .SetDraggable(true)
                .SetCloseOnEscape(true)
                .UseMetaPrefabs(true);
            builder.AddLabel(FeatureSettingsLocalization.Get("ui.intro", locale));
            builder.AddButton(FeatureSettingsLocalization.Get("ui.enableAll", locale), () => SetAll(true), "enable_all");
            builder.AddButton(FeatureSettingsLocalization.Get("ui.disableAll", locale), () => SetAll(false), "disable_all");
            builder.BeginScroll(500f);
            BuildGroup(builder, FeatureSettingsLocalization.Get("ui.groupVisitors", locale), locale,
                ModFeature.VisitorPriority,
                ModFeature.PhoneCooldownReset,
                ModFeature.AutomaticEvidenceCleanup);
            BuildGroup(builder, FeatureSettingsLocalization.Get("ui.groupForecasts", locale), locale,
                ModFeature.TradeForecast,
                ModFeature.NightForecast,
                ModFeature.CustomerDemandHighlight,
                ModFeature.SecurityInspectionHighlight,
                ModFeature.GunsmithHighlight);
            BuildGroup(builder, FeatureSettingsLocalization.Get("ui.groupAppraisal", locale), locale,
                ModFeature.AutomaticAppraisalValue,
                ModFeature.AppraisalDetails,
                ModFeature.BerryQualityMarker,
                ModFeature.MagnifierAugDetection,
                ModFeature.ArrivalIdentityAlert);
            builder.End();
            builder.Show();
            AppraisalAssistantUI.ApplyItemFontToWindow(_window, locale);
            CustomUIBuilder.RebuildLayout(_window.Rect);
        }

        private static void BuildGroup(CustomUIBuilder builder, string title, string locale, params ModFeature[] features)
        {
            builder.AddHeader(title);
            builder.AddSeparator(null, 2);
            foreach (ModFeature feature in features)
            {
                ModFeature captured = feature;
                builder.AddToggle(FeatureCatalog.Label(captured, locale), FeatureSettings.IsEnabled(captured),
                    value => FeatureSettings.SetEnabled(captured, value), "feature_" + (int)captured);
            }
            builder.AddSpace(10f);
        }

        private static void SetAll(bool enabled)
        {
            FeatureSettings.SetAll(enabled);
            Close();
            Open();
            NextOpeningVisitorsMod.Log?.Log("All mod features " + (enabled ? "enabled." : "disabled."));
        }

        private static void Close()
        {
            CustomUIWindow? window = _window;
            _window = null;
            if (window == null) return;
            window.onClose = null;
            if (window.IsAlive) window.Close();
        }
    }
}
