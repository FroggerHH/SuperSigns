using static Minimap;
using static Minimap.PinType;

namespace SuperSigns;

[HarmonyPatch]
public class AddPin
{
    [HarmonyWrapSafe, HarmonyPostfix, HarmonyPatch(typeof(TombStone), nameof(TombStone.Awake))]
    private static void PatchTombStoneAwake()
    {
        Debug($"PatchTombStoneAwake called");
        UpdateTombstonePins();
    }
}