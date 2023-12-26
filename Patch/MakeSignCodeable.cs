namespace SuperSigns.Patch;

[HarmonyPatch]
public static class MakeSignCodeable
{
    [HarmonyPatch(typeof(Sign), nameof(Sign.UpdateText))] [HarmonyWrapSafe] [HarmonyPrefix]
    private static void Patch_SignAwake(Sign __instance) { }
}