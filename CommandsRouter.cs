using SuperSigns.Controllers;

namespace SuperSigns;

public static class CommandsRouter
{
    public static readonly Dictionary<string, SSCommandController> SS_commands = new();
    public static readonly List<string> commandNames;

    static CommandsRouter()
    {
        SS_commands = new()
        {
            { "signSettings", new SignSettingsSsCommand() },
            { "give", new GiveSsCommand() },
            { "highlight", new HighlightSsCommand() },
            { "ping", new PingSsCommand() }
        };
        commandNames = SS_commands.Keys.ToList();
    }

    public static ConsoleCommandException RunCommand(string str)
    {
        str = str.Replace("ss ", "");
        Debug($"Got a ss command for execution -> {str}");
        var args = str.Split(' ').ToList();
        if (SS_commands.TryGetValue(args[0], out var command))
        {
            args.Remove(args[0]);
            var consoleCommandException = command.Execute(args);
            if (consoleCommandException is not null)
                consoleCommandException =
                    new ConsoleCommandException("Command execution failed with error: "
                                                + consoleCommandException.Message);
            return consoleCommandException;
        }

        return null;
    }
}