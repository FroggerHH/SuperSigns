namespace SuperSigns.Patch;

[HarmonyPatch]
public static class InspectSign
{
    [HarmonyPatch(typeof(Player), nameof(Player.Start))] [HarmonyWrapSafe]
    private static class Patch_SignAwake
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            var zdos = ZDOMan.instance?.m_objectsBySector[119026];
            DebugWarning($"zdos == {zdos?.Count.ToString() ?? "null"}", false);
            if (zdos == null)
            {
                DebugError("Old zone 119026 == null");
            } else
            {
                var signs = zdos.Where(x => x.GetPrefab() == signHash).ToList();
                DebugWarning($"signs == {signs.Count}", false);
            }
        }
    }
}