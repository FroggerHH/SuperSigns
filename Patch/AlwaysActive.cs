namespace SuperSigns.Patch;

[HarmonyPatch]
public static class AlwaysActive
{
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.FindDistantObjects)), HarmonyWrapSafe, HarmonyPostfix]
    private static void Patch_ZDOManZDOSectorInvalidated(List<ZDO> objects) =>
        objects.RemoveAll(x => x.GetPrefab() == signHash);
}