using HarmonyLib;

namespace NextOpeningVisitors
{
    [HarmonyPatch(typeof(LocHelper), nameof(LocHelper.GetLocalizedDialoguePhone))]
    internal static class PhoneDialogueTextPatch
    {
        private static void Postfix(string key, object[] arguments, ref string __result)
        {
            if (PhoneDialogueTextRules.TryRewrite(
                    key,
                    arguments,
                    LocHelper.GetCurrentLocaleCode(),
                    out string? replacement) && replacement != null)
            {
                __result = replacement;
            }
        }
    }
}
