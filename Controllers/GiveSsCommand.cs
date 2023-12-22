namespace SuperSigns.Controllers;

public class GiveSsCommand : SSCommandController
{
    public override ConsoleCommandException Execute(List<string> args)
    {
        var itemPrefabName = args[0];
        if (!int.TryParse(args[1], out var count))
            return new ConsoleCommandException($"{args[1]} is not a valid number");
        string targetPlayer = m_localPlayer.GetPlayerName().Replace(" ", "");
        if (args.Count == 3) targetPlayer = args[2];
        if (!ZNet.instance.GetPlayerList().Exists(x => x.m_name.Replace(" ", "") == targetPlayer))
            return new ConsoleCommandException($"Player with name {targetPlayer} not found");
        if (!ObjectDB.instance.m_itemByHash.ContainsKey(itemPrefabName.GetStableHashCode()))
            return new ConsoleCommandException($"Item with name {itemPrefabName} does not exist");
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SS_GiveItem",
            targetPlayer, itemPrefabName, count);

        Debug($"GiveSsCommand executed");
        return null;
    }

    internal static void GiveItem(long sender, string targetPlayerName, string itemPrefabName, int count)
    {
        var myName = Player.m_localPlayer.GetPlayerName().Replace(" ", "");
        if (myName != targetPlayerName) return;
        m_localPlayer.PickupPrefab(ObjectDB.instance.GetItemPrefab(itemPrefabName), stackSize: count);
    }
}

[HarmonyPatch]
file static class GiveCommandPatch
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start)), HarmonyPostfix]
    private static void Postfix()
    {
        ZRoutedRpc.instance.Register<string, string, int>("SS_GiveItem", GiveSsCommand.GiveItem);
        DebugWarning("SS_GiveItem RPC registered", false);
    }
}