using System;
using System.Collections.Generic;
using System.Globalization;

namespace NextOpeningVisitors
{
    internal static class AssistantAppraisalLocalization
    {
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["auth.genuine"] = "Authenticity: genuine.",
            ["auth.counterfeit"] = "Authenticity: counterfeit.",
            ["value.title"] = "True value: ${0}",
            ["value.total"] = "Total: ${0}",
            ["value.minimum"] = "The minimum value of ${0} applies.",
            ["value.unavailable"] = "A complete value breakdown is unavailable; no estimate has been guessed.",
            ["quality.title"] = "Glow berry quality: {0}",
            ["quality.normal"] = "Normal",
            ["quality.small"] = "Small",
            ["quality.moldy"] = "Moldy",
            ["quality.unripe"] = "Unripe",
            ["quality.dry"] = "Dry",
            ["quality.unknown"] = "Unknown",
            ["value.base"] = "Base",
            ["value.carbon"] = "Carbon composite",
            ["value.gold"] = "Gold plated",
            ["value.neural"] = "Neural interface",
            ["value.emgPlus"] = "EMG+ interface",
            ["value.unspecifiedInterface"] = "Unspecified interface",
            ["value.wear"] = "Wear",
            ["value.damage"] = "Damage",
            ["value.intact"] = "No wear or damage",
            ["value.armored"] = "Armored add-on",
            ["value.bladed"] = "Blade add-on",
            ["value.tacticalSet"] = "Tactical set",
            ["value.deluxeSet"] = "Deluxe set",
            ["value.lightWear"] = "Light wear",
            ["value.heavyWear"] = "Heavy wear",
            ["value.cracked"] = "Cracked casing",
            ["value.exposedScrews"] = "Exposed screws",
            ["value.reinforced"] = "Reinforced",
            ["value.lowColor"] = "Low-grade color",
            ["value.highColor"] = "High-grade color",
            ["value.wildBonus"] = "Wild's Rides bonus",
            ["value.zhonghuaBonus"] = "ZhongHua bonus",
            ["value.republicBonus"] = "Republic Naval bonus",
            ["value.ignitionBonus"] = "Ignition Lab bonus",
            ["value.doubleMeat"] = "Double meat patty",
            ["value.deluxe"] = "Deluxe edition",
            ["value.edgeMonth"] = "First / last production month",
            ["value.firstRun"] = "First production run",
            ["value.highFacility"] = "High-quality facility {0}",
            ["value.lowFacility"] = "Low-quality facility {0}",
            ["value.opened"] = "Packaging opened: fixed value",
            ["value.counterfeitFixed"] = "Counterfeit: fixed value",
            ["reason.indicatorExpired"] = "Indicator is orange: the injector has expired.",
            ["reason.indicatorEmpty"] = "Indicator is red: the injector is spent.",
            ["reason.indicatorUnknown"] = "Indicator failed inspection; its detailed state is unavailable.",
            ["reason.injectorLabel"] = "Label failed: company or product name is missing, misspelled, or uses the wrong font.",
            ["reason.serialLength"] = "Serial number must contain exactly seven digits.",
            ["reason.serialRange"] = "Serial number is outside the valid range of 1250000–4559999.",
            ["reason.serialRecalled"] = "Serial number belongs to recalled batch {0}.",
            ["reason.serialUnknown"] = "Serial number failed inspection; its value is unavailable.",
            ["reason.bodyCrack"] = "Injector body has a visible crack.",
            ["reason.bodyBottom"] = "Injector body has a damaged bottom.",
            ["reason.bodyChip"] = "Injector body has a chipped upper edge.",
            ["reason.bodyUnknown"] = "Injector body is damaged.",
            ["reason.injectorYellow"] = "Injector body has an incorrect yellow-green tint.",
            ["reason.injectorBlue"] = "Injector body has an incorrect blue-cyan tint.",
            ["reason.injectorColor"] = "Injector body color does not match the standard swatch.",
            ["reason.cigaretteLettering"] = "Brand lettering failed: missing or replaced letters, or incorrect font.",
            ["reason.cigaretteColor"] = "Packaging color does not match the genuine Hemingway swatch.",
            ["reason.sealMissing"] = "Security seal is missing.",
            ["reason.sealDamaged"] = "Security seal is torn or damaged.",
            ["reason.sealUnknown"] = "Security seal failed inspection.",
            ["reason.logoMissing"] = "Logo is missing.",
            ["reason.logoSimplified"] = "Logo is simplified and lacks standard details.",
            ["reason.logoDecorated"] = "Logo has decorations absent from the genuine design.",
            ["reason.logoRotated"] = "Logo is upside down.",
            ["reason.logoUnknown"] = "Logo does not match the genuine design.",
            ["reason.boxUnknown"] = "Packaging appearance does not match the genuine example.",
            ["reason.stampLettering"] = "Stamp lettering failed: incorrect wording, spelling, font, color, or print quality.",
            ["reason.stampLogo"] = "Stamp logo is missing or differs from the genuine example.",
            ["reason.stampSeal"] = "Security seal does not match the rule for the printed amount.",
            ["reason.stampAmount"] = "Printed amount {0} is not an issued denomination (40, 80, 120, 160, 200, or 240).",
            ["reason.stampAmountUnknown"] = "Printed amount is not an issued denomination.",
            ["reason.chipMissing"] = "Chip is missing; the food stamp cannot be used.",
            ["reason.chipDamaged"] = "Chip is damaged; the food stamp cannot be used.",
            ["reason.chipCracked"] = "Chip has a crack through it; the food stamp cannot be used.",
            ["reason.chipColor"] = "Chip color does not match the standard.",
            ["reason.chipUnknown"] = "Chip failed inspection.",
            ["reason.issueUnknown"] = "This check failed; its detailed cause is unavailable.",
            ["reason.productionMissing"] = "Production date marking is missing.",
            ["reason.invalidMonth"] = "Production month {0} is invalid; it must be 1–12.",
            ["reason.invalidYear"] = "Production year {0} is invalid; it must be 425–429 or 432–438.",
            ["reason.facilityMissing"] = "Packaging facility marking is missing.",
            ["reason.invalidFacility"] = "Packaging facility {0} is invalid; it must be 01–35.",
            ["reason.davreMissing"] = "Davre logo is missing.",
            ["reason.davreWrong"] = "Davre logo is incorrect.",
            ["reason.firstRunSticker"] = "First production run must not have a Ribwich sticker.",
            ["reason.secondRunStickerMissing"] = "Second production run is missing its Ribwich sticker.",
            ["reason.secondRunStickerWrong"] = "Second production run has an incorrect Ribwich sticker.",
            ["reason.bunWithoutPickles"] = "Sesame bun without extra pickles; deluxe edition requires both.",
            ["reason.picklesWithoutBun"] = "Extra pickles without a sesame bun; deluxe edition requires both.",
            ["reason.invalidDeluxe"] = "Bun and pickles do not match a standard or deluxe edition.",
            ["arrival.aug"] = "ALERT: augmented customer",
            ["arrival.wanted"] = "ALERT: wanted customer",
            ["arrival.both"] = "ALERT: augmented and wanted customer",
            ["arrival.name"] = "Customer: {0}",
            ["arrival.realName"] = "Real name: {0}",
            ["arrival.tier"] = "Augmentation tier: {0}",
            ["arrival.reward"] = "Report reward: ${0}",
            ["arrival.unknown"] = "Unknown"
        };

        private static readonly Dictionary<string, string> Chinese = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["auth.genuine"] = "真伪鉴定：正品。",
            ["auth.counterfeit"] = "真伪鉴定：赝品。",
            ["value.title"] = "真实价值：${0}",
            ["value.total"] = "总价：${0}",
            ["value.minimum"] = "已按最低价值 ${0} 计算。",
            ["value.unavailable"] = "完整价值拆解暂不可用，未填入猜测结果。",
            ["quality.title"] = "荧光莓品质：{0}",
            ["quality.normal"] = "正常", ["quality.small"] = "较小", ["quality.moldy"] = "发霉",
            ["quality.unripe"] = "未熟", ["quality.dry"] = "干枯", ["quality.unknown"] = "未知",
            ["value.base"] = "基础", ["value.carbon"] = "碳复合材料", ["value.gold"] = "镀金",
            ["value.neural"] = "神经接口", ["value.emgPlus"] = "EMG+ 接口",
            ["value.unspecifiedInterface"] = "规格不明接口", ["value.wear"] = "磨损",
            ["value.damage"] = "受损", ["value.intact"] = "无磨损或受损",
            ["value.armored"] = "装甲组件", ["value.bladed"] = "刀刃组件",
            ["value.tacticalSet"] = "战术型组合", ["value.deluxeSet"] = "豪华型组合",
            ["value.lightWear"] = "轻度磨损", ["value.heavyWear"] = "重度磨损",
            ["value.cracked"] = "外壳破裂", ["value.exposedScrews"] = "螺丝外露",
            ["value.reinforced"] = "加固处理", ["value.lowColor"] = "低级颜色",
            ["value.highColor"] = "高级颜色", ["value.wildBonus"] = "王尔德载具加成",
            ["value.zhonghuaBonus"] = "中华船舶加成", ["value.republicBonus"] = "共和国海军加成",
            ["value.ignitionBonus"] = "点火实验室加成", ["value.doubleMeat"] = "双层肉饼",
            ["value.deluxe"] = "豪华版", ["value.edgeMonth"] = "历史首月／末月",
            ["value.firstRun"] = "第一生产批次", ["value.highFacility"] = "高品质包装厂 {0}",
            ["value.lowFacility"] = "低品质包装厂 {0}", ["value.opened"] = "包装已开封，按固定价计",
            ["value.counterfeitFixed"] = "判为赝品，按固定价计",
            ["reason.indicatorExpired"] = "指示灯为橙色：注射器已过期。",
            ["reason.indicatorEmpty"] = "指示灯为红色：注射器已空。",
            ["reason.indicatorUnknown"] = "指示灯检验不合格，详细状态暂不可用。",
            ["reason.injectorLabel"] = "标签不合格：公司或产品名称缺失、错拼或字体错误。",
            ["reason.serialLength"] = "序列号必须为七位数字。",
            ["reason.serialRange"] = "序列号超出 1250000—4559999 的有效范围。",
            ["reason.serialRecalled"] = "序列号属于已召回批次 {0}。",
            ["reason.serialUnknown"] = "序列号检验不合格，具体号码暂不可用。",
            ["reason.bodyCrack"] = "瓶体有明显裂纹。", ["reason.bodyBottom"] = "瓶体底部破损。",
            ["reason.bodyChip"] = "瓶体上缘有缺口。", ["reason.bodyUnknown"] = "瓶体存在损坏。",
            ["reason.injectorYellow"] = "瓶体呈异常的黄绿色，与标准色卡不符。",
            ["reason.injectorBlue"] = "瓶体呈异常的蓝青色，与标准色卡不符。",
            ["reason.injectorColor"] = "瓶体主色与标准色卡不符。",
            ["reason.cigaretteLettering"] = "品牌字样不合格：字母缺失、被替换或字体错误。",
            ["reason.cigaretteColor"] = "包装颜色与海明威正品色卡不符。",
            ["reason.sealMissing"] = "防伪标缺失。", ["reason.sealDamaged"] = "防伪标已撕开或破损。",
            ["reason.sealUnknown"] = "防伪标检验不合格。", ["reason.logoMissing"] = "商标缺失。",
            ["reason.logoSimplified"] = "商标被简化，缺少标准图案细节。",
            ["reason.logoDecorated"] = "商标添加了不属于正品的装饰。", ["reason.logoRotated"] = "商标倒置。",
            ["reason.logoUnknown"] = "商标与正品图案不符。", ["reason.boxUnknown"] = "包装外观与正品不一致。",
            ["reason.stampLettering"] = "票面文字不合格：内容、拼写、字体、颜色或印刷质量错误。",
            ["reason.stampLogo"] = "商标缺失或与正品图案不同。",
            ["reason.stampSeal"] = "防伪标与票面面值对应规则不符。",
            ["reason.stampAmount"] = "面值 {0} 不属于发行面值（40、80、120、160、200、240）。",
            ["reason.stampAmountUnknown"] = "印制面值不属于发行面值。",
            ["reason.chipMissing"] = "芯片缺失，食品券无法使用。", ["reason.chipDamaged"] = "芯片破损，食品券无法使用。",
            ["reason.chipCracked"] = "芯片有贯穿裂纹，食品券无法使用。", ["reason.chipColor"] = "芯片颜色与标准不符。",
            ["reason.chipUnknown"] = "芯片检验不合格。", ["reason.issueUnknown"] = "本项检验不合格，具体原因暂不可用。",
            ["reason.productionMissing"] = "生产日期标记缺失。",
            ["reason.invalidMonth"] = "生产月份 {0} 无效，应为 1—12 月。",
            ["reason.invalidYear"] = "生产年份 {0} 无效，应为 425—429 或 432—438 年。",
            ["reason.facilityMissing"] = "包装工厂标记缺失。",
            ["reason.invalidFacility"] = "包装工厂编号 {0} 无效，应为 01—35。",
            ["reason.davreMissing"] = "达夫雷标志缺失。", ["reason.davreWrong"] = "达夫雷标志图案错误。",
            ["reason.firstRunSticker"] = "第一生产批次不应有 Ribwich 贴纸。",
            ["reason.secondRunStickerMissing"] = "第二生产批次缺少 Ribwich 贴纸。",
            ["reason.secondRunStickerWrong"] = "第二生产批次的 Ribwich 贴纸图案错误。",
            ["reason.bunWithoutPickles"] = "有芝麻面包但没有加量酸黄瓜，豪华版须同时具备两者。",
            ["reason.picklesWithoutBun"] = "有加量酸黄瓜但没有芝麻面包，豪华版须同时具备两者。",
            ["reason.invalidDeluxe"] = "面包与酸黄瓜组合不符合普通版或豪华版。",
            ["arrival.aug"] = "提醒：改造体顾客到店",
            ["arrival.wanted"] = "提醒：通缉犯到店",
            ["arrival.both"] = "提醒：改造体兼通缉犯到店",
            ["arrival.name"] = "顾客：{0}", ["arrival.realName"] = "真名：{0}",
            ["arrival.tier"] = "改造等级：{0}", ["arrival.reward"] = "举报奖励：${0}",
            ["arrival.unknown"] = "未知"
        };

        internal static string Get(string key, string localeCode, params object?[] args)
        {
            string locale = ModLocalization.NormalizeLocale(localeCode);
            Dictionary<string, string> table = locale == "zh" ? Chinese : English;
            if (!table.TryGetValue(key, out string? text)) text = English.TryGetValue(key, out string? fallback) ? fallback : key;
            return args.Length == 0 ? text : string.Format(CultureInfo.InvariantCulture, text, args);
        }

        internal static string Berry(BerryQuality quality, string localeCode)
        {
            return Get("quality." + quality.ToString().ToLowerInvariant(), localeCode);
        }

        internal static string FormatBreakdown(AppraisalValueBreakdown breakdown, string localeCode)
        {
            if (breakdown.FixedLabel != null) return Get(breakdown.FixedLabel, localeCode);
            var parts = new List<string>();
            foreach (AppraisalValueLine line in breakdown.Lines)
                parts.Add(Get(line.Label, localeCode, line.Detail) + " " + line.Amount.ToString(CultureInfo.InvariantCulture));
            string formula = string.Join(" ＋ ", parts) + " ＝ " + breakdown.Total.ToString(CultureInfo.InvariantCulture);
            if (breakdown.Subtotal < breakdown.Minimum)
                formula += "\n" + Get("value.minimum", localeCode, breakdown.Minimum);
            return formula;
        }
    }
}
