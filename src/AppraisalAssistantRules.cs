using System;
using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal enum AugDetectionReason
    {
        None = 0,
        VisibleModifications = 1,
        Freeze = 2,
        Seizure = 3,
        SevereAnomaly = 4,
        NoVisibleClue = 5
    }

    internal enum BerryQuality
    {
        Unknown = 0,
        Normal = 1,
        Small = 2,
        Moldy = 3,
        Unripe = 4,
        Dry = 5
    }

    internal sealed class AppraisalValueLine
    {
        internal readonly string Label;
        internal readonly int Amount;
        internal readonly string? Detail;

        internal AppraisalValueLine(string label, int amount, string? detail = null)
        {
            Label = label;
            Amount = amount;
            Detail = detail;
        }
    }

    internal sealed class AppraisalValueBreakdown
    {
        internal readonly List<AppraisalValueLine> Lines = new List<AppraisalValueLine>();
        internal readonly List<AppraisalValueLine> Reasons = new List<AppraisalValueLine>();
        internal int Total;
        internal int Subtotal;
        internal int Minimum;
        internal string? FixedLabel;

        internal bool MatchesNative(int nativeValue) => Total == nativeValue;

        internal void Add(string label, int amount, string? detail = null)
        {
            Lines.Add(new AppraisalValueLine(label, amount, detail));
            Subtotal += amount;
        }

        internal void Finish(int minimum)
        {
            Minimum = minimum;
            Total = Math.Max(minimum, Subtotal);
        }

        internal static AppraisalValueBreakdown Fixed(int value, string label)
        {
            return new AppraisalValueBreakdown
            {
                Total = value, Subtotal = value, Minimum = value, FixedLabel = label
            };
        }
    }

    internal static class AppraisalAssistantRules
    {
        internal static bool CanAutoFill(bool appraisalService, bool appraisalItem, int correctValue)
        {
            return appraisalService && appraisalItem && correctValue > 0;
        }

        internal static AugDetectionReason DetectAug(bool isAug, bool visibleModifications,
            bool freezeTell, bool seizureTell, int augTier)
        {
            if (!isAug) return AugDetectionReason.None;
            if (visibleModifications) return AugDetectionReason.VisibleModifications;
            if (freezeTell) return AugDetectionReason.Freeze;
            if (seizureTell) return AugDetectionReason.Seizure;
            if (augTier == 4) return AugDetectionReason.SevereAnomaly;
            return AugDetectionReason.NoVisibleClue;
        }

        internal static bool HasVisibleAugSprite(string? spriteName)
        {
            if (spriteName == null || spriteName.Length == 0) return false;
            string value = spriteName.ToLowerInvariant();
            return value.StartsWith("aug", StringComparison.Ordinal) ||
                value.Contains("_aug") || value.Contains("-aug") ||
                value.Contains("aug_") || value.Contains("aug-") ||
                value.Contains("cyber") || value.Contains("android");
        }

        internal static bool ShouldAlertArrival(bool arrived, bool isAug, bool isWanted)
        {
            return arrived && (isAug || isWanted);
        }

        internal static BerryQuality GetBerryQuality(int condition, int? visualVariant)
        {
            if (condition == -2) return BerryQuality.Moldy;
            if (condition == 2) return BerryQuality.Small;
            if (condition == 0 || condition == 1) return BerryQuality.Normal;
            if (condition == -1 && visualVariant.HasValue)
            {
                if (visualVariant.Value == 0 || visualVariant.Value == 1) return BerryQuality.Unripe;
                if (visualVariant.Value == 2) return BerryQuality.Dry;
            }
            return BerryQuality.Unknown;
        }

        internal static AppraisalValueBreakdown? Hand(string? material, string? iface,
            string? condition, string? armored, string? bladed)
        {
            if (!OneOf(material, "ALUMINUM_ALLOY", "CARBON_COMPOSITE", "GOLD_PLATED") ||
                !OneOf(iface, "EMG", "EMG_PLUS", "NEURAL", "UNSPECIFIED") ||
                !OneOf(condition, "NONE", "WEAR", "DAMAGE") ||
                !OneOf(armored, "NONE", "ARMORED") ||
                !OneOf(bladed, "NONE", "BLADED")) return null;

            var result = new AppraisalValueBreakdown();
            result.Add("value.base", 100);
            if (material == "CARBON_COMPOSITE") result.Add("value.carbon", 50);
            if (material == "GOLD_PLATED") result.Add("value.gold", 200);
            if (iface == "NEURAL") result.Add("value.neural", 120);
            if (iface == "EMG_PLUS") result.Add("value.emgPlus", 45);
            if (iface == "UNSPECIFIED") result.Add("value.unspecifiedInterface", -60);
            if (condition == "WEAR") result.Add("value.wear", -35);
            if (condition == "DAMAGE") result.Add("value.damage", -85);
            if (condition == "NONE") result.Add("value.intact", 0);
            if (armored == "ARMORED") result.Add("value.armored", 50);
            if (bladed == "BLADED") result.Add("value.bladed", 80);
            if (material == "CARBON_COMPOSITE" && (iface == "EMG_PLUS" || iface == "NEURAL") &&
                condition == "NONE" && armored == "ARMORED" && bladed == "BLADED")
                result.Add("value.tacticalSet", 100);
            if (material == "GOLD_PLATED" && iface == "NEURAL" && condition == "NONE")
                result.Add("value.deluxeSet", 200);
            result.Finish(50);
            return result;
        }

        internal static AppraisalValueBreakdown? Plasma(string? wear, string? crack,
            string? uncovered, string? reinforced, string? color, string? seal)
        {
            if (!OneOf(wear, "NONE", "WEAR", "WEAR_HEAVY") ||
                !OneOf(crack, "NONE", "CRACK") || !OneOf(uncovered, "NONE", "UNCOVERED") ||
                !OneOf(reinforced, "NONE", "REINFORCED") ||
                !OneOf(color, "STANDARD", "POOR1", "POOR2", "HIGH_QUALITY") ||
                !OneOf(seal, "NONE", "LL", "ZHONGHUA", "REPUBLIC", "IGNITION_LAB")) return null;

            var result = new AppraisalValueBreakdown();
            result.Add("value.base", 300);
            if (wear == "WEAR") result.Add("value.lightWear", -45);
            if (wear == "WEAR_HEAVY") result.Add("value.heavyWear", -90);
            if (crack == "CRACK") result.Add("value.cracked", -150);
            if (uncovered == "UNCOVERED") result.Add("value.exposedScrews", -30);
            if (reinforced == "REINFORCED") result.Add("value.reinforced", 60);
            if (color == "POOR1" || color == "POOR2") result.Add("value.lowColor", -150);
            if (color == "HIGH_QUALITY") result.Add("value.highColor", 150);
            if (seal == "LL" && color != "HIGH_QUALITY") result.Add("value.wildBonus", 60);
            else if (seal == "ZHONGHUA" && wear == "NONE" && crack == "NONE" &&
                uncovered == "NONE" && color == "STANDARD") result.Add("value.zhonghuaBonus", 120);
            else if (seal == "REPUBLIC" && crack != "CRACK" && color == "STANDARD")
                result.Add("value.republicBonus", 120);
            else if (seal == "IGNITION_LAB" && reinforced == "NONE" && wear == "NONE" &&
                crack == "NONE" && uncovered == "NONE" && color == "HIGH_QUALITY")
                result.Add("value.ignitionBonus", 750);
            result.Finish(90);
            return result;
        }

        internal static AppraisalValueBreakdown? Ribwich(int wrap, int nativeValue,
            int? meat, int? bun, int? pickles, int? month, int? year, int? facility,
            int? davreLogo, int? ribwichLogo, int? productionMark, int? facilityMark)
        {
            if (wrap == 1 || wrap == 2) return AppraisalValueBreakdown.Fixed(40, "value.opened");
            if (wrap != 0) return null;

            List<AppraisalValueLine>? reasons = RibwichCounterfeitReasons(bun, pickles,
                month, year, facility, davreLogo, ribwichLogo, productionMark, facilityMark);
            if (nativeValue == 40)
            {
                if (reasons == null || reasons.Count == 0) return null;
                var counterfeit = AppraisalValueBreakdown.Fixed(40, "value.counterfeitFixed");
                counterfeit.Reasons.AddRange(reasons);
                return counterfeit;
            }
            if (reasons == null || reasons.Count > 0 || !meat.HasValue || !bun.HasValue ||
                !pickles.HasValue || !month.HasValue || !year.HasValue || !facility.HasValue ||
                !OneOf(meat.Value, 0, 1)) return null;

            var result = new AppraisalValueBreakdown();
            result.Add("value.base", 150);
            if (meat.Value == 1) result.Add("value.doubleMeat", 800);
            if (bun.Value == 1 && pickles.Value == 1) result.Add("value.deluxe", 300);
            if ((month.Value == 1 && year.Value == 425) || (month.Value == 12 && year.Value == 438))
                result.Add("value.edgeMonth", 50);
            if (year.Value >= 425 && year.Value <= 429) result.Add("value.firstRun", 80);
            if (OneOf(facility.Value, 1, 2, 5, 8, 21, 34))
                result.Add("value.highFacility", 50, facility.Value.ToString("D2"));
            else if (OneOf(facility.Value, 7, 15, 18, 26, 31))
                result.Add("value.lowFacility", -50, facility.Value.ToString("D2"));
            result.Finish(40);
            return result;
        }

        internal static List<AppraisalValueLine>? RibwichCounterfeitReasons(int? bun,
            int? pickles, int? month, int? year, int? facility, int? davreLogo,
            int? ribwichLogo, int? productionMark, int? facilityMark)
        {
            if (!bun.HasValue || !pickles.HasValue || !month.HasValue || !year.HasValue ||
                !facility.HasValue || !davreLogo.HasValue || !ribwichLogo.HasValue ||
                !productionMark.HasValue || !facilityMark.HasValue) return null;

            var reasons = new List<AppraisalValueLine>();
            bool first = year.Value >= 425 && year.Value <= 429;
            bool second = year.Value >= 432 && year.Value <= 438;
            bool validMonth = month.Value >= 1 && month.Value <= 12;
            if (productionMark.Value == -1) reasons.Add(new AppraisalValueLine("reason.productionMissing", 0));
            else
            {
                if (!validMonth) reasons.Add(new AppraisalValueLine("reason.invalidMonth", 0, month.Value.ToString()));
                if (!first && !second) reasons.Add(new AppraisalValueLine("reason.invalidYear", 0, year.Value.ToString()));
            }
            if (facilityMark.Value == -1) reasons.Add(new AppraisalValueLine("reason.facilityMissing", 0));
            else if (facility.Value < 1 || facility.Value > 35)
                reasons.Add(new AppraisalValueLine("reason.invalidFacility", 0, facility.Value.ToString()));
            if (davreLogo.Value != 0)
                reasons.Add(new AppraisalValueLine(davreLogo.Value == -1 ? "reason.davreMissing" : "reason.davreWrong", 0));
            if (validMonth && first && ribwichLogo.Value != -1)
                reasons.Add(new AppraisalValueLine("reason.firstRunSticker", 0));
            if (validMonth && second && ribwichLogo.Value != 0)
                reasons.Add(new AppraisalValueLine(ribwichLogo.Value == -1 ? "reason.secondRunStickerMissing" : "reason.secondRunStickerWrong", 0));
            if (bun.Value == 1 && pickles.Value == 0)
                reasons.Add(new AppraisalValueLine("reason.bunWithoutPickles", 0));
            else if (bun.Value == 0 && pickles.Value == 1)
                reasons.Add(new AppraisalValueLine("reason.picklesWithoutBun", 0));
            else if ((bun.Value != 0 && bun.Value != 1) || (pickles.Value != 0 && pickles.Value != 1))
                reasons.Add(new AppraisalValueLine("reason.invalidDeluxe", 0));
            return reasons;
        }

        private static bool OneOf(string? value, params string[] options)
        {
            if (value == null) return false;
            foreach (string option in options) if (value == option) return true;
            return false;
        }

        private static bool OneOf(int value, params int[] options)
        {
            foreach (int option in options) if (value == option) return true;
            return false;
        }
    }
}
