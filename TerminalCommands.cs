// ReSharper disable VariableHidesOuterVariable

namespace SuperSigns;

[HarmonyPatch]
public static class TerminalCommands
{
    private static string currentCommand;

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
                    if (CommandsRouter.currentCommandException != null) throw CommandsRouter.currentCommandException;
                    if (!CommandsRouter.commandNames.Contains(args[1]))
                        throw new ConsoleCommandException("Unknown ss command");
                    var runCommandException = CommandsRouter.RunCommand(currentCommand);
                    if (runCommandException.status != CommandStatus.Ok)
                        throw new ConsoleCommandException(runCommandException.exceptionMessage);
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
        bool isCommandValid = CommandsRouter.currentCommandException is null;
        string validStr = isCommandValid ? "<color=#00FF00>✔</color>" : "<color=#F8733C>❌</color>";
        __instance.m_search.text = validStr + GetCommandTooltip() + __instance.m_search.text;
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
        CommandsRouter.currentCommandException = null;
        if (!currentCommand.IsGood()) return string.Empty;
        if (!currentCommand.StartsWith("ss")) return string.Empty;
        string available = $"<u>Available commands</u>:\n";
        string optionsToChoose = $"<u>Options to choose</u>:\n";
        if (currentCommand.Replace(" ", "") == "ss")
            return available;

        var command = currentCommand.Replace("ss ", "");
        var args = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower())
            .ToList();
        var startOfNewCommand = args[0].ToLower();
        switch (startOfNewCommand)
        {
            case "signsettings":
            {
                if (args.Count == 1 && !currentCommand.EndsWith(" "))
                {
                    CommandsRouter.currentCommandException =
                        new ConsoleCommandException($"Got no arguments in signSettings command");
                    return "<b>SignSettings</b> - controls the super sign behaviour\n"
                           + "Parameters:\n"
                           + " Activation mode: In which case the code in the sign will be executed\n"
                           + " Editing permissions: Who can change the contents of the sign\n"
                           + " Activation Permissions: lol\n"
                           + CommandsRouter.commandNames.GetString();
                }

                // if ((args.Count == 2 && currentCommand.EndsWith(" ")) || args.Count > 4)
                // {
                //     CommandsRouter.currentCommandException =
                //         new ConsoleCommandException("Too many arguments. Expected 3 or 4 arguments");
                //     return $"<color=#F8733C>{CommandsRouter.currentCommandException.Message}</color>\n";
                // }

                if (args.Count == 1 && currentCommand.EndsWith(" "))
                {
                    CommandsRouter.currentCommandException =
                        new ConsoleCommandException($"Got no arguments in signSettings command");
                    return available;
                }

                if (args.Count == 2 && args[1] != "activationmode" && args[1] != "editingpermissions"
                    && args[1] != "activationpermissions")
                {
                    CommandsRouter.currentCommandException =
                        new ConsoleCommandException($"Unknown signSettings second argument {args[1]}");
                    return available + $"<color=yellow>{CommandsRouter.currentCommandException.Message}</color>\n";
                }

                if (args.Count == 2 && !currentCommand.EndsWith(" "))
                {
                    var secondArg = args[1].ToLower();
                    if (secondArg == "activationmode")
                    {
                        CommandsRouter.currentCommandException =
                            new ConsoleCommandException($"Got no activationMode argument in signSettings command");
                        return "Activation mode: In which case the code in the sign will be executed\n"
                               + " Expected values: BasicInteract / Hover / InRange / Loaded\n";
                    } else if (secondArg == "editingpermissions")
                    {
                        CommandsRouter.currentCommandException =
                            new ConsoleCommandException($"Got no editingPermissions argument in signSettings command");
                        return "Editing permissions: Who can change the contents of the sign\n"
                               + " Expected values: Admin / Permitted / Anyone / SteamIds\n";
                    } else if (secondArg == "activationpermissions")
                    {
                        CommandsRouter.currentCommandException =
                            new ConsoleCommandException(
                                $"Got no activationPermissions argument in signSettings command");
                        return "Activation Permissions: Who can trigger the sign code\n"
                               + " Expected values: Admin / Permitted / Anyone / SteamIds\n";
                    } else
                    {
                        CommandsRouter.currentCommandException =
                            new ConsoleCommandException($"Unknown signSettings arg second argument {secondArg}");
                        return $"<color=yellow>{CommandsRouter.currentCommandException.Message}</color>\n";
                    }
                }

                if (args.Count == 2 && currentCommand.EndsWith(" "))
                {
                    var secondArg = args[1].ToLower();
                    CommandsRouter.currentCommandException =
                        new ConsoleCommandException($"Got no {secondArg} argument in signSettings command");
                    return available;
                }

                if (args.Count == 3 && !currentCommand.EndsWith(" "))
                {
                    var secondArg = args[1].ToLower();
                    var thirdArg = args[2].ToLower();
                    if (secondArg == "activationmode")
                    {
                        if (thirdArg != "basicinteract"
                            && thirdArg != "hover"
                            && thirdArg != "inrange"
                            && thirdArg != "loaded")
                        {
                            CommandsRouter.currentCommandException =
                                new ConsoleCommandException($"Unknown signSettings third argument {thirdArg}");
                            return available
                                   + $"<color=yellow>{CommandsRouter.currentCommandException.Message}</color>\n";
                        }

                        if (thirdArg == "basicinteract")
                            return "BasicInteract: Code is executed when the player interacts with the sign\n";
                        if (thirdArg == "hover")
                            return "Hover: Code is executed when the player hovers over the sign\n";
                        if (thirdArg == "inrange")
                            return "InRange: Code is executed when the player is in some range of the sign\n"
                                   + $" Parameters:\n"
                                   + " Range: Minimum distance between the player and the sign to execute the code\n";
                        if (thirdArg == "loaded")
                            return "Loaded: Code is executed when the sign is loaded\n";

                        CommandsRouter.currentCommandException =
                            new ConsoleCommandException($"Unknown signSettings third argument {thirdArg}");
                        return $"<color=#F8733C>{CommandsRouter.currentCommandException.Message}</color>\n";
                    } else if (secondArg == "editingpermissions")
                    {
                        return "Editing permissions: Who can change the contents of the sign\n"
                               + " Expected values: Admin / Permitted / Anyone / SteamIds\n";
                    } else if (secondArg == "activationpermissions")
                    {
                    } else
                    {
                        CommandsRouter.currentCommandException =
                            new ConsoleCommandException($"Unknown signSettings arg second argument {secondArg}");
                        return $"<color=#F8733C>{CommandsRouter.currentCommandException.Message}</color>\n";
                    }
                }

                if (args.Count == 3 && currentCommand.EndsWith(" "))
                {
                    var secondArg = args[1].ToLower();
                    var thirdArg = args[2].ToLower();
                    var fourthArg = args[3].ToLower();

                    if (secondArg == "activationmode")
                    {
                        CommandsRouter.currentCommandException = new("Too many arguments");
                        return $"<color=#F8733C>{CommandsRouter.currentCommandException.Message}</color>\n";
                    }

                    if (secondArg == "editingpermissions")
                    {
                        if (thirdArg != "steamids")
                        {
                            CommandsRouter.currentCommandException = new("Too many arguments");
                            return $"<color=#F8733C>{CommandsRouter.currentCommandException.Message}</color>\n";
                        }

                        return "SteamIds: Who can change the contents of the sign\n";
                    }
                }

                var secondArg_ = args[1].ToLower();
                var thirdArg_ = args[2].ToLower();
                if (thirdArg_ != "steamids" &&
                    (secondArg_ == "editingpermissions" || secondArg_ == "activationpermissions"))
                    return available;
                else
                {
                    CommandsRouter.currentCommandException = new("Too many arguments");
                    return $"<color=#F8733C>{CommandsRouter.currentCommandException.Message}</color>\n";
                }

                CommandsRouter.currentCommandException =
                    new ConsoleCommandException("Unknown error occurred. Inform the developer about this");
                return $"<color=#F8733C>{CommandsRouter.currentCommandException.Message}</color>\n";
            }
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
                    CommandsRouter.currentCommandException =
                        new ConsoleCommandException("Too many arguments. Expected 3 or 4 arguments");
                    return $"<color=#F8733C>{CommandsRouter.currentCommandException.Message}</color>\n";
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

                CommandsRouter.currentCommandException =
                    new ConsoleCommandException("Unknown error occurred. Inform the developer about this");
                return $"<color=#F8733C>{CommandsRouter.currentCommandException.Message}</color>\n";
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

        if (CommandsRouter.commandNames.Any(x => x.ToLower().StartsWith(startOfNewCommand)))
        {
            CommandsRouter.currentCommandException = new ConsoleCommandException("Unknown ss command");
            return available;
        }

        CommandsRouter.currentCommandException = new ConsoleCommandException("Unknown ss command");
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
        if (command != startOfNewCommand)
            switch (startOfNewCommand)
            {
                case "signsettings":
                {
                    if ((args.Count == 2 && !currentCommand.EndsWith(" "))
                        || (args.Count == 1 && currentCommand.EndsWith(" ")))
                        return new() { "activationMode", "editingPermissions", "activationPermissions" };

                    if (
                        (args.Count == 2 && args[1].ToLower() == "activationmode" && currentCommand.EndsWith(" "))
                        || (args.Count == 3 && args[1].ToLower() == "activationmode"
                                            && !currentCommand.EndsWith(" ")))
                        return new() { "basicInteract", "hover", "inRange", "loaded" };

                    return new() { "\n<color=#F8733C>Options not found</color>" };
                }
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

        if (args.Any(x => CommandsRouter.commandNames.Contains(x))) return new();
        return CommandsRouter.commandNames;
    }
}