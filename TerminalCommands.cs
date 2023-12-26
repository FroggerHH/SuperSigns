// ReSharper disable VariableHidesOuterVariable

namespace SuperSigns;

[HarmonyPatch]
public static class TerminalCommands
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal)), HarmonyPostfix]
    private static void AddCommands()
    {
        new ConsoleCommand("ss",
            $"Prefix for all new commands {ModName} adds", args =>
            {
                RunCommand(args =>
                {
                    if (args.Length < 2)
                        throw new ConsoleCommandException("First argument must be ss commands callName");
                    var exec = CommandsRouter.TryRunCommand(CommandsRouter.currentCommand);
                    if (exec.status != CommandStatus.Ok)
                        throw new ConsoleCommandException(exec.exceptionMessage);
                }, args);
            }, true, optionsFetcher: CommandsRouter.GetOptions);
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.UpdateInput)), HarmonyPostfix]
    private static void UpdateCurrentCommand(Terminal __instance)
    {
        CommandsRouter.currentCommand = __instance.m_input.text;
        if (!CommandsRouter.currentCommand.StartsWith("ss ")) return;
        bool isCommandValid = CommandsRouter.GetTooltip(out string tooltip);
        // string validStr = isCommandValid ? "<color=#00FF00>✔</color>" : "<color=#F8733C>❌</color>";
        var word = CommandsRouter.currentCommand.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault();
        if (CommandsRouter.currentCommand.EndsWith(" ") || CommandsRouter.currentCommand.EndsWith(":")) word = "";
        __instance.updateSearch(word, CommandsRouter.GetOptions(), false);
        __instance.m_search.text = tooltip + __instance.m_search.text;
    }

    [HarmonyPatch(typeof(ConsoleCommand), nameof(ConsoleCommand.GetTabOptions)), HarmonyPostfix]
    private static void UpdateCommandTabOptions(ConsoleCommand __instance, ref List<string> __result)
    {
        if (Terminal.commands["ss"] != __instance) return;

        __result = CommandsRouter.GetOptions();
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.tabCycle)), HarmonyPrefix]
    private static void Fix_tabCycle(Terminal __instance, ref string word, ref List<string> options)
    {
        if (!CommandsRouter.currentCommand.StartsWith("ss")) return;
        options = CommandsRouter.GetOptions();
        word = __instance.m_input.text.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
    }
}