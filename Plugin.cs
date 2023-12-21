using BepInEx;
using BepInEx.Configuration;
using Valheim.UI;
using static Minimap;
using static Minimap.PinType;

namespace SuperSigns;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    public static readonly int Hash = "Player_tombstone".GetStableHashCode();
    public static readonly int playerHash = "Player".GetStableHashCode();

    private const string ModName = "SuperSigns",
        ModVersion = "0.1.0",
        ModGUID = $"com.{ModAuthor}.{ModName}",
        ModAuthor = "JustAFrogger";

    private static ConfigEntry<bool> doubleSize;
    public static readonly int signHash = "sign".GetStableHashCode();


    private void Awake()
    {
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGUID);
        doubleSize = config("General", "_", true, "");
    }
}