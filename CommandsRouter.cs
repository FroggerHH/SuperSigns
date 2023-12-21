using SuperSigns.Controllers;

namespace SuperSigns;

public static class CommandsRouter
{
    public static readonly Dictionary<string, CommandController> SS_commands = new();
    public static readonly List<string> commandNames;

    static CommandsRouter()
    {
        SS_commands = new()
        {
            { "give", new GiveCommand() },
            { "highlight", new HighlightCommand() },
            { "ping", new PingCommand() }
        };
        commandNames = SS_commands.Keys.ToList();
    }

    public static ConsoleCommandException RunCommand(string str)
    {
        str = str.Replace("ss ", "");
        var args = str.Split(' ').ToList();
        if (SS_commands.TryGetValue(args[0], out var command))
        {
            args.Remove(args[0]);
            var consoleCommandException = command.Execute(args);
            consoleCommandException =
                new ConsoleCommandException("Command execution failed with error: " + consoleCommandException.Message);
            return consoleCommandException;
        }

        return null;
    }
}