using BepInEx;

namespace SuperSigns;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string ModName = "SuperSigns",
        ModVersion = "0.1.0",
        ModGUID = $"com.{ModAuthor}.{ModName}",
        ModAuthor = "JustAFrogger";

    public static readonly int Hash = "Player_tombstone".GetStableHashCode();
    public static readonly int playerHash = "Player".GetStableHashCode();

    public static readonly int signHash = "sign".GetStableHashCode();

    private void Awake()
    {
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGUID);
        // exampleConfigEntry = config("General", "_", true, "");
    }
}