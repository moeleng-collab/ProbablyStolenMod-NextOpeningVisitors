namespace NextOpeningVisitors
{
    internal static class FeatureSettings
    {
        private const string SaveKey = "nextopeningvisitors.feature_settings.v1";
        private static FeatureSettingsState _state = new FeatureSettingsState();
        private static bool _active;

        internal static bool IsEnabled(ModFeature feature)
        {
            return _active && _state.IsEnabled(feature);
        }

        internal static void SetModActive(bool active)
        {
            _active = active;
        }

        internal static void SetEnabled(ModFeature feature, bool enabled)
        {
            if (!_active) return;
            _state.Set(feature, enabled);
            SaveState();
            ApplyFeature(feature);
        }

        internal static void SetAll(bool enabled)
        {
            if (!_active) return;
            _state.SetAll(enabled);
            SaveState();
            ApplyAll();
        }

        internal static void LoadState()
        {
            _state = new FeatureSettingsState();
            PlayerStore? store = PlayerStore.HasInstance ? PlayerStore.Instance : null;
            if (store == null) return;
            if (store.ModData().TryGetValue(SaveKey, out string? serialized) &&
                !FeatureSettingsState.TryDeserialize(serialized, out _state))
            {
                _state = new FeatureSettingsState();
                NextOpeningVisitorsMod.Log?.Warning(
                    "Invalid mod feature settings; all features were restored to their enabled defaults.");
            }
        }

        internal static void ApplyAll()
        {
            foreach (ModFeature feature in FeatureCatalog.All) ApplyFeature(feature);
            FeatureSettingsUI.SetEnabled(_active);
        }

        private static void ApplyFeature(ModFeature feature)
        {
            switch (feature)
            {
                case ModFeature.PhoneCooldownReset:
                    PhoneCooldownResetButton.SetEnabled(IsEnabled(feature));
                    break;
                case ModFeature.TradeForecast:
                    TradeForecastUI.SetEnabled(IsEnabled(feature));
                    break;
                case ModFeature.NightForecast:
                    NightForecastUI.SetEnabled(IsEnabled(feature));
                    break;
                case ModFeature.CustomerDemandHighlight:
                case ModFeature.SecurityInspectionHighlight:
                case ModFeature.GunsmithHighlight:
                    DemandHighlightUI.SetEnabled(
                        IsEnabled(ModFeature.CustomerDemandHighlight) ||
                        IsEnabled(ModFeature.SecurityInspectionHighlight) ||
                        IsEnabled(ModFeature.GunsmithHighlight));
                    break;
                case ModFeature.BerryQualityMarker:
                    BerryQualityUI.SetEnabled(IsEnabled(feature));
                    break;
                case ModFeature.AutomaticAppraisalValue:
                case ModFeature.AppraisalDetails:
                case ModFeature.MagnifierAugDetection:
                case ModFeature.ArrivalIdentityAlert:
                    AppraisalAssistantUI.SetEnabled(
                        IsEnabled(ModFeature.AutomaticAppraisalValue) ||
                        IsEnabled(ModFeature.AppraisalDetails) ||
                        IsEnabled(ModFeature.MagnifierAugDetection) ||
                        IsEnabled(ModFeature.ArrivalIdentityAlert));
                    break;
                case ModFeature.AutomaticEvidenceCleanup:
                    AutoEvidenceCallController.SetEnabled(IsEnabled(feature));
                    break;
            }
        }

        private static void SaveState()
        {
            PlayerStore? store = PlayerStore.HasInstance ? PlayerStore.Instance : null;
            if (store == null)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not save mod feature settings because the store is unavailable.");
                return;
            }
            store.ModData()[SaveKey] = _state.Serialize();
        }
    }
}
