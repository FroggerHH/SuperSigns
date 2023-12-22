namespace SuperSigns.Controllers;

public class SignSettingsSsCommand : SSCommandController
{
    public override ConsoleCommandException Execute(List<string> args)
    {
       
        
        Debug($"SignSettingsSsCommand executed");
        return null;
    }
}

[HarmonyPatch]
file static class SignSettingsCommandPatch
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start)), HarmonyPostfix]
    private static void Postfix()
    {
        
    }
}