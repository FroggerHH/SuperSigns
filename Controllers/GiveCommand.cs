namespace SuperSigns.Controllers;

public class GiveCommand : CommandController
{
    public override ConsoleCommandException Execute(List<string> args)
    {
        var itemPrefabName = args[0];
        int count = 0;
        if (!int.TryParse(args[1], out count)) return new ConsoleCommandException($"{args[1]} is not a valid number");
        ZNet.PlayerInfo targetPlayer = ZNet.instance.GetPlayerList().Find(x => x.m_name.Replace(" ", "") == args[2]);
        if (!targetPlayer.m_name.IsGood())
            return new ConsoleCommandException($"Player with name {args[2]} not found");
        if (!ObjectDB.instance.m_itemByHash.ContainsKey(itemPrefabName.GetStableHashCode()))
            return new ConsoleCommandException($"Item with name {itemPrefabName} does not exist");

        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SS_GiveItem",
            targetPlayer.m_name, itemPrefabName, count);

        Debug($"GiveCommand executed");
        return null;
    }

    internal static void GiveItem(long _, string targetPlayerName, string itemPrefabName, int count)
    {
        if (Player.m_localPlayer.GetPlayerName() != targetPlayerName) return;
        m_localPlayer.PickupPrefab(ObjectDB.instance.GetItemPrefab(itemPrefabName), stackSize: count);
    }
}

[HarmonyPatch]
file class GiveCommandPatch
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    private static void Patch()
    {
        ZRoutedRpc.instance.Register<string, string, int>("SS_GiveItem", GiveCommand.GiveItem);
    }
}