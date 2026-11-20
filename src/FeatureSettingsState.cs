using System;
using System.Collections.Generic;
using System.Globalization;

namespace NextOpeningVisitors
{
    internal enum ModFeature
    {
        VisitorPriority = 0,
        AppointmentDeduplication = 1,
        PhoneCooldownReset = 2,
        PhoneDialogueText = 3,
        TradeForecast = 4,
        NightForecast = 5,
        AutomaticEvidenceCleanup = 6,
        CustomerDemandHighlight = 7,
        SecurityInspectionHighlight = 8,
        GunsmithHighlight = 9,
        AutomaticAppraisalValue = 10,
        AppraisalDetails = 11,
        BerryQualityMarker = 12,
        MagnifierAugDetection = 13,
        ArrivalIdentityAlert = 14
    }

    internal static class FeatureCatalog
    {
        internal static readonly ModFeature[] All =
        {
            ModFeature.VisitorPriority,
            ModFeature.PhoneCooldownReset,
            ModFeature.TradeForecast,
            ModFeature.NightForecast,
            ModFeature.AutomaticEvidenceCleanup,
            ModFeature.CustomerDemandHighlight,
            ModFeature.SecurityInspectionHighlight,
            ModFeature.GunsmithHighlight,
            ModFeature.AutomaticAppraisalValue,
            ModFeature.AppraisalDetails,
            ModFeature.BerryQualityMarker,
            ModFeature.MagnifierAugDetection,
            ModFeature.ArrivalIdentityAlert
        };

        internal static string Label(ModFeature feature, string localeCode)
        {
            return FeatureSettingsLocalization.Label(feature, localeCode);
        }
    }

    internal sealed class FeatureSettingsState
    {
        private const int CurrentSchemaVersion = 1;
        private readonly Dictionary<ModFeature, bool> _values = new Dictionary<ModFeature, bool>();

        internal FeatureSettingsState()
        {
            SetAll(true);
        }

        internal bool IsEnabled(ModFeature feature)
        {
            return !_values.TryGetValue(feature, out bool value) || value;
        }

        internal void Set(ModFeature feature, bool enabled)
        {
            _values[feature] = enabled;
        }

        internal void SetAll(bool enabled)
        {
            foreach (ModFeature feature in FeatureCatalog.All) _values[feature] = enabled;
        }

        internal string Serialize()
        {
            var disabled = new List<int>();
            foreach (ModFeature feature in FeatureCatalog.All)
                if (!IsEnabled(feature)) disabled.Add((int)feature);
            disabled.Sort();
            var values = new List<string>(disabled.Count);
            foreach (int id in disabled) values.Add(id.ToString(CultureInfo.InvariantCulture));
            return CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture) + "|" + string.Join(",", values);
        }

        internal static bool TryDeserialize(string serialized, out FeatureSettingsState state)
        {
            state = new FeatureSettingsState();
            if (string.IsNullOrEmpty(serialized)) return true;
            string[] sections = serialized.Split('|');
            if (sections.Length != 2 || sections[0] != CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture))
                return false;
            if (sections[1].Length == 0) return true;

            var disabled = new HashSet<ModFeature>();
            string[] entries = sections[1].Split(',');
            foreach (string entry in entries)
            {
                if (!int.TryParse(entry, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ||
                    !Enum.IsDefined(typeof(ModFeature), id))
                    return false;
                ModFeature feature = (ModFeature)id;
                if (!disabled.Add(feature)) return false;
            }
            foreach (ModFeature feature in disabled) state.Set(feature, false);
            return true;
        }
    }
}
