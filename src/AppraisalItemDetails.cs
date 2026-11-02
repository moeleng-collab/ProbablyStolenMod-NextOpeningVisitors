using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NextOpeningVisitors
{
    internal static class AppraisalItemDetails
    {
        private static readonly string[] InjectorIssues =
            { "ISSUE_INDICATOR", "ISSUE_LABEL", "ISSUE_SERIAL", "ISSUE_BODY", "ISSUE_COLOR" };
        private static readonly string[] CigaretteIssues =
            { "CIGARETTE_LETTERING_ISSUE", "CIGARETTE_COLOR_ISSUE", "CIGARETTE_SEAL_ISSUE", "CIGARETTE_LOGO_ISSUE", "CIGARETTE_BOX_ISSUE" };
        private static readonly string[] StampIssues =
            { "STAMP_LETTERING_ISSUE", "STAMP_LOGO_ISSUE", "STAMP_SEAL_ISSUE", "STAMP_AMOUNT_ISSUE", "STAMP_CHIP_ISSUE" };

        internal static string Build(GameItem item, string locale)
        {
            if (item == null || item.modifiedState == null || item.modifiedState.dict == null) return string.Empty;
            var text = new StringBuilder();
            if (item.IsTag("INS_INJECTOR_TAG")) AppendAuthenticity(text, item, InjectorIssues, 0, locale);
            else if (item.IsTag("CIGARETTE_TAG")) AppendAuthenticity(text, item, CigaretteIssues, 1, locale);
            else if (item.IsTag("FOOD_STAMP_TAG")) AppendAuthenticity(text, item, StampIssues, 2, locale);

            if (item.IsTag("HAND_TAG") || item.IsTag("RIBWICH_TAG") || item.IsTag("PLASMA_CELL_TAG"))
            {
                int nativeValue = AppraisalHelper.GetCorrectBaseValue(item);
                AppraisalValueBreakdown? breakdown = Calculate(item, nativeValue);
                if (item.IsTag("RIBWICH_TAG"))
                {
                    bool counterfeit = RibwichHelper.IsCounterfeit(item);
                    AppendLine(text, AssistantAppraisalLocalization.Get(counterfeit ? "auth.counterfeit" : "auth.genuine", locale));
                    if (counterfeit && breakdown != null)
                        foreach (AppraisalValueLine reason in breakdown.Reasons)
                            AppendLine(text, "• " + AssistantAppraisalLocalization.Get(reason.Label, locale, reason.Detail ?? string.Empty));
                }
                AppendLine(text, AssistantAppraisalLocalization.Get("value.title", locale, nativeValue));
                AppendLine(text, breakdown != null && breakdown.MatchesNative(nativeValue)
                    ? AssistantAppraisalLocalization.FormatBreakdown(breakdown, locale)
                    : AssistantAppraisalLocalization.Get("value.unavailable", locale));
            }
            else if (item.IsTag("BERRY_TAG"))
            {
                int? condition = Number(item, "BERRY_CONDITION_INT");
                BerryQuality quality = condition.HasValue
                    ? AppraisalAssistantRules.GetBerryQuality(condition.Value, Number(item, "BERRY_VISUAL_VARIANT_INT"))
                    : BerryQuality.Unknown;
                AppendLine(text, AssistantAppraisalLocalization.Get("quality.title", locale,
                    AssistantAppraisalLocalization.Berry(quality, locale)));
            }
            return text.ToString().TrimEnd();
        }

        private static AppraisalValueBreakdown? Calculate(GameItem item, int nativeValue)
        {
            if (item.IsTag("HAND_TAG"))
                return AppraisalAssistantRules.Hand(Text(item, "HAND_MATERIAL"), Text(item, "HAND_INTERFACE"),
                    Text(item, "HAND_CONDITION"), Text(item, "HAND_ADDON_ARMORED"), Text(item, "HAND_ADDON_BLADED"));
            if (item.IsTag("PLASMA_CELL_TAG"))
                return AppraisalAssistantRules.Plasma(Text(item, "PLASMA_CASING_CONDITION_WEAR"),
                    Text(item, "PLASMA_CASING_CONDITION_CRACK"), Text(item, "PLASMA_CASING_CONDITION_UNCOVERED"),
                    Text(item, "PLASMA_CASING_CONDITION_REINFORCED"), Text(item, "PLASMA_COLOR"), Text(item, "PLASMA_SEAL"));
            if (item.IsTag("RIBWICH_TAG"))
                return AppraisalAssistantRules.Ribwich(Number(item, "RIBWICH_WRAP_TAG") ?? -1, nativeValue,
                    Number(item, "RIBWICH_MEAT_TAG"), Number(item, "RIBWICH_BUN_TAG"), Number(item, "RIBWICH_PICKLES_TAG"),
                    Number(item, "RIBWICH_PRODUCTION_MONTH_TAG"), Number(item, "RIBWICH_PRODUCTION_YEAR_TAG"),
                    Number(item, "RIBWICH_FACILITY_NUMBER_TAG"), Number(item, "RIBWICH_DAVRE_LOGO_TAG"),
                    Number(item, "RIBWICH_RIBWICH_LOGO_TAG"), Number(item, "RIBWICH_PRODUCTION_TAG"), Number(item, "RIBWICH_FACILITY_TAG"));
            return null;
        }

        private static void AppendAuthenticity(StringBuilder text, GameItem item, string[] issueKeys, int type, string locale)
        {
            var failed = new List<string>();
            foreach (string issue in issueKeys) if (item.IsTag(issue)) failed.Add(issue);
            AppendLine(text, AssistantAppraisalLocalization.Get(failed.Count == 0 ? "auth.genuine" : "auth.counterfeit", locale));
            foreach (string issue in failed)
                AppendLine(text, "• " + Explain(item, type, issue, locale));
        }

        private static string Explain(GameItem item, int type, string issue, string locale)
        {
            string key = "reason.issueUnknown";
            object? detail = null;
            if (type == 0)
            {
                switch (issue)
                {
                    case "ISSUE_INDICATOR":
                        int? indicator = Number(item, "INJECTOR_INDICATOR");
                        key = indicator == 1 ? "reason.indicatorExpired" : indicator == 3 ? "reason.indicatorEmpty" : "reason.indicatorUnknown";
                        break;
                    case "ISSUE_LABEL": key = "reason.injectorLabel"; break;
                    case "ISSUE_SERIAL": key = InjectorSerialReason(Text(item, "INJECTOR_SERIAL"), out detail); break;
                    case "ISSUE_BODY":
                        int? body = Number(item, "INJECTOR_BODY");
                        key = body == 1 ? "reason.bodyCrack" : body == 2 ? "reason.bodyBottom" : body == 3 ? "reason.bodyChip" : "reason.bodyUnknown";
                        break;
                    case "ISSUE_COLOR":
                        int? color = Number(item, "INJECTOR_COLOR");
                        key = color == 0 ? "reason.injectorYellow" : color == 1 ? "reason.injectorBlue" : "reason.injectorColor";
                        break;
                }
            }
            else if (type == 1)
            {
                switch (issue)
                {
                    case "CIGARETTE_LETTERING_ISSUE": key = "reason.cigaretteLettering"; break;
                    case "CIGARETTE_COLOR_ISSUE": key = "reason.cigaretteColor"; break;
                    case "CIGARETTE_SEAL_ISSUE":
                        int? seal = Number(item, "CIGARETTE_SEAL");
                        key = seal == -1 ? "reason.sealMissing" : seal == 1 ? "reason.sealDamaged" : "reason.sealUnknown";
                        break;
                    case "CIGARETTE_LOGO_ISSUE":
                        int? logo = Number(item, "CIGARETTE_LOGO");
                        key = logo == -1 ? "reason.logoMissing" : logo == 1 ? "reason.logoSimplified" :
                            logo == 2 || logo == 3 ? "reason.logoDecorated" : logo == 4 ? "reason.logoRotated" : "reason.logoUnknown";
                        break;
                    case "CIGARETTE_BOX_ISSUE": key = "reason.boxUnknown"; break;
                }
            }
            else
            {
                switch (issue)
                {
                    case "STAMP_LETTERING_ISSUE": key = "reason.stampLettering"; break;
                    case "STAMP_LOGO_ISSUE": key = "reason.stampLogo"; break;
                    case "STAMP_SEAL_ISSUE": key = "reason.stampSeal"; break;
                    case "STAMP_AMOUNT_ISSUE":
                        int? amount = Number(item, "STAMP_AMOUNT");
                        key = amount.HasValue ? "reason.stampAmount" : "reason.stampAmountUnknown";
                        detail = amount;
                        break;
                    case "STAMP_CHIP_ISSUE":
                        int? chip = Number(item, "STAMP_CHIP");
                        key = chip == -1 ? "reason.chipMissing" : chip == 1 ? "reason.chipDamaged" :
                            chip == 2 ? "reason.chipCracked" : chip == 3 ? "reason.chipColor" : "reason.chipUnknown";
                        break;
                }
            }
            return detail == null
                ? AssistantAppraisalLocalization.Get(key, locale)
                : AssistantAppraisalLocalization.Get(key, locale, detail);
        }

        private static string InjectorSerialReason(string? serial, out object? detail)
        {
            detail = null;
            if (string.IsNullOrEmpty(serial) || serial!.Length != 7) return "reason.serialLength";
            foreach (char character in serial!) if (character < '0' || character > '9') return "reason.serialLength";
            int value = int.Parse(serial!, CultureInfo.InvariantCulture);
            if (value < 1250000 || value > 4559999) return "reason.serialRange";
            int batch = value / 10000;
            if (batch == 250 || batch == 251 || batch == 301 || batch == 355 || batch == 385 || batch == 386 || batch == 421)
            {
                detail = batch;
                return "reason.serialRecalled";
            }
            return "reason.serialUnknown";
        }

        private static TagState? Entry(GameItem item, string key)
        {
            if (item.modifiedState == null || item.modifiedState.dict == null) return null;
            return item.modifiedState.dict.TryGetValue(key, out TagState? state) ? state : null;
        }

        private static string? Text(GameItem item, string key) => Entry(item, key)?.GetString();
        private static int? Number(GameItem item, string key) => Entry(item, key)?.GetInt();

        private static void AppendLine(StringBuilder text, string line)
        {
            if (text.Length > 0) text.Append('\n');
            text.Append(line);
        }
    }
}
