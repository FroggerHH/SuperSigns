// ReSharper disable VariableHidesOuterVariable

namespace SuperSigns;

[HarmonyPatch]
public static class TerminalCommands
{
    private static string currentCommand;
    private static ConsoleCommandException currentCommandException;

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal)), HarmonyPostfix]
    private static void AddCommands()
    {
        new ConsoleCommand("ss",
            $"Prefix for all new commands {ModName} adds", args =>
            {
                RunCommand(args =>
                {
                    if (!IsAdmin) throw new ConsoleCommandException("You are not an admin on this server");
                    if (args.Length < 2) throw new ConsoleCommandException("First argument must be ss commands name");
                    if (currentCommandException != null) throw currentCommandException;
                    var runCommandException = CommandsRouter.RunCommand(currentCommand);
                    if (runCommandException != null) throw runCommandException;
                }, args);
            }, true, optionsFetcher: GetCommandOptions);
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.UpdateInput)), HarmonyPostfix]
    private static void UpdateCurrentCommand(Terminal __instance)
    {
        currentCommand = __instance.m_input.text;
        if (!currentCommand.StartsWith("ss ")) return;
        string[] strArray1 = __instance.m_input.text.Split(' ');
        var word = strArray1[strArray1.Length - 1];
        __instance.updateSearch(word, GetCommandOptions(), false);
        __instance.m_search.text = GetCommandTooltip() + __instance.m_search.text;
    }

    [HarmonyPatch(typeof(ConsoleCommand), nameof(ConsoleCommand.GetTabOptions)), HarmonyPostfix]
    private static void UpdateCommandTabOptions(ConsoleCommand __instance, ref List<string> __result)
    {
        if (Terminal.commands["ss"] != __instance) return;
        __result = GetCommandOptions();
        __instance.m_tabOptions = __result;
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.tabCycle)), HarmonyPrefix]
    private static void Fix_tabCycle(Terminal __instance, ref string word, ref List<string> options)
    {
        if (!currentCommand.StartsWith("ss")) return;
        options = GetCommandOptions();
        string[] strArray1 = __instance.m_input.text.Split(' ');
        word = strArray1[strArray1.Length - 1];
    }

    private static string GetCommandTooltip()
    {
        currentCommandException = null;
        if (!currentCommand.IsGood()) return string.Empty;
        if (!currentCommand.StartsWith("ss")) return string.Empty;
        string available = $"<u>Available commands</u>:\n";
        if (currentCommand.Replace(" ", "") == "ss")
            return available;

        var command = currentCommand.Replace("ss ", "");
        var args = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower())
            .ToList();
        var startOfNewCommand = args[0].ToLower();
        switch (startOfNewCommand)
        {
            case "give":
            {
                if (args.Count == 1 && !currentCommand.EndsWith(" "))
                    return "<b>Give</b> - adds the specified item directly to the player's inventory\n"
                           + "Parameters:\n"
                           + " Item name: Specifies which item to give to the player\n"
                           + " Count: How many items to give\n"
                           + " Player nickname(optional): Which player to give. Without spaces. Leave it empty for the local player\n"
                           + CommandsRouter.commandNames.GetString();
                if ((args.Count == 4 && currentCommand.EndsWith(" ")) || args.Count > 4)
                {
                    currentCommandException =
                        new ConsoleCommandException("Too many arguments. Expected 3 or 4 arguments");
                    return $"<color=red>{currentCommandException.Message}</color>\n";
                }

                if ((args.Count == 2 && !currentCommand.EndsWith(" "))
                    || (args.Count == 1 && currentCommand.EndsWith(" ")))
                    return "Item name: What item to give\n";
                if ((args.Count == 3 && !currentCommand.EndsWith(" "))
                    || (args.Count == 2 && currentCommand.EndsWith(" ")))
                    return "Count: How many items to give\n";
                if ((args.Count == 4 && !currentCommand.EndsWith(" "))
                    || (args.Count == 3 && currentCommand.EndsWith(" ")))
                    return "Player nickname(optional): Which player to give. <color=yellow>Without spaces.</color>\n";

                currentCommandException =
                    new ConsoleCommandException("Unknown error occurred. Inform the developer about this");
                return $"<color=red>{currentCommandException.Message}</color>";
            }
            case "highlight":
            {
                return "<b>Highlight</b> - highlights a specific object\n"
                       + "Parameters:\n"
                       + " Item name: Specifies which item to give to the player\n"
                       + " Player nickname: Which player to give. Without spaces\n"
                       + " Count: How many items to give\n";
            }
            case "ping":
                return "<b>ping</b> - fun Ping-Pong command\n";
        }

        if (CommandsRouter.commandNames.Any(x => x.StartsWith(startOfNewCommand))) return available;

        currentCommandException = new ConsoleCommandException("Unknown ss command");
        return "Unknown ss command\n";
    }

    private static List<string> GetCommandOptions()
    {
        if (!currentCommand.IsGood()) return new();
        var noSpaces = currentCommand.Replace(" ", "");
        if (!currentCommand.StartsWith("ss")) return new();
        if (noSpaces == "ss")
            return CommandsRouter.commandNames;

        var command = currentCommand.Replace("ss ", "");
        var args = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower())
            .ToList();
        var startOfNewCommand = args[0].ToLower();
        switch (startOfNewCommand)
        {
            case "give":
            {
                if ((args.Count == 4 && currentCommand.EndsWith(" ")) || args.Count > 4) return new();
                if ((args.Count == 2 && !currentCommand.EndsWith(" "))
                    || (args.Count == 1 && currentCommand.EndsWith(" ")))
                    return ObjectDB.instance.m_items.Select(x => x.name).ToList();
                if ((args.Count == 3 && !currentCommand.EndsWith(" "))
                    || (args.Count == 2 && currentCommand.EndsWith(" ")))
                    return Enumerable.Range(1, 100).Select(x => x.ToString()).ToList();
                if ((args.Count == 4 && !currentCommand.EndsWith(" "))
                    || (args.Count == 3 && currentCommand.EndsWith(" ")))
                    return ZNet.instance.GetPlayerList().Select(x => x.m_name.Replace(" ", "")).ToList();

                if (args.Count == 1) return new();
                return new() { "ERROR" };
            }
            case "highlight":
            {
                return new();
            }
            case "ping":
                if (args.Count > 2) return new();
                return new() { "Pong1", "Pong2" };
        }

        return CommandsRouter.commandNames;
    }
}