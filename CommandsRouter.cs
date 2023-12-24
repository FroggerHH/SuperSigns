using System.Text;
using SuperSigns.Controllers;
using SuperSigns.Controllers.Longer;
using SuperSigns.Controllers.Message;
using SuperSigns.Controllers.Ping;

namespace SuperSigns;

public static class CommandsRouter
{
    public static readonly Dictionary<string, SSCommandController> SS_commands;
    public static readonly List<string> commandNames;
    public static string currentCommand;

    static CommandsRouter()
    {
        SS_commands = new();
        SS_commands.Add("ping", new PingSsCommand());
        SS_commands.Add("longer", new LongerSSCommand());
        SS_commands.Add("longer", new MessageSSCommand());
        commandNames = SS_commands.Keys.ToList();
    }

    public static (CommandStatus status, string exceptionMessage) TryRunCommand(string str)
    {
        str = str.Replace("ss ", "");
        Debug($"Got a ss command for execution -> '{str}'");
        var args = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        var commandName = args[0];
        Debug($"commandName = '{commandName}'");
        (CommandStatus status, string exceptionMessage) logic = Logic();
        Debug($"status = '{logic.status}', exceptionMessage = '{logic.exceptionMessage}'");
        if (logic.status != CommandStatus.Ok)
        {
            var fail = "Command execution failed with error: ";
            if (logic.status == CommandStatus.None) logic.exceptionMessage = fail + "Command not found";
            else logic.exceptionMessage = fail + logic.exceptionMessage;
            return (logic.status, logic.exceptionMessage);
        }

        return logic;

        (CommandStatus status, string exceptionMessage) Logic()
        {
            if (!SS_commands.TryGetValue(commandName, out SSCommandController baseCommand))
            {
                Debug($"SS command {commandName} not found");
                return (CommandStatus.None, $"SS command {commandName} not found");
            }

            args.Remove(commandName);
            string exceptionMessage;

            if (!GetParameters(str, out var parameters, out exceptionMessage))
                return (CommandStatus.Error, exceptionMessage);
            if (!GetBranch(str, out var lastBranch, out exceptionMessage))
                return (CommandStatus.Error, exceptionMessage);
            if (!CheckParameters(parameters, lastBranch, out exceptionMessage))
                return (CommandStatus.Error, exceptionMessage);

            if (lastBranch!.parameters.Count != 0 && lastBranch.branches.Count != 0)
                throw new Exception(
                    "SSCommandBranch can not have both parameters and branches at the same time. "
                    + $"Thrown by {lastBranch.callName}.");


            Debug($"Logic 4, lastBranch = '{lastBranch?.ToString() ?? "null"}', parameters = '{parameters.Select(pair
                => pair.Key + ":" + pair.Value).GetString()}'");

            return lastBranch.Execute(parameters);
        }
    }
    //ss longer second message:lol postfix:--

    public static bool GetParameters(string commandline, out Dictionary<string, object> result, out string errorMessage)
    {
        commandline = commandline.Replace("ss ", "");
        errorMessage = string.Empty;
        var args = commandline.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.Contains(':')).ToList();
        result = new();

        foreach (var arg in args)
        {
            var list_ = arg.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (list_.Count != 2)
            {
                errorMessage = $"Command parameter {arg} is not in valid format. Should be 'parameterName:value'";
                Debug($"GetParameters -> {errorMessage}");
                return false;
            }

            string paramName = list_[0];
            string paramValueStr = list_[1];
            result.Add(paramName, paramValueStr);
        }

        Debug($"GetParameters result = {result.GetString()}");

        return true;
    }

    public static bool GetBranch(string commandline, [CanBeNull] out SSCommandBranch lastBranch,
        out string errorMessage)
    {
        commandline = commandline.Replace("ss ", "");
        lastBranch = null;
        errorMessage = string.Empty;
        var args = commandline.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !x.Contains(':'))
            .ToList();

        if (!SS_commands.TryGetValue(args[0], out var baseCommand))
        {
            errorMessage = $"Command {args[0]} not found";
            Debug($"GetBranch -> {errorMessage}");
            return false;
        }

        args.Remove(args[0]);

        Debug($"GetBranch, baseCommand = {baseCommand.displayName}");
        lastBranch = baseCommand;
        foreach (var arg in args)
        {
            var command = lastBranch.branches.Find(x => x.callName == arg);
            if (!command)
            {
                errorMessage = $"Command branch with name {arg} do not exists in {lastBranch.callName} command";
                Debug($"GetBranch -> {errorMessage}");
                return false;
            }

            lastBranch = command;
        }

        if (lastBranch.canBeCalled == false)
        {
            errorMessage = $"Command {lastBranch.callName} can not be called directly. Use one of its branches";
            return false;
        }

        Debug($"GetBranch result = {lastBranch.displayName}");

        return true;
    }

    public static bool CheckParameters(Dictionary<string, object> args, SSCommandBranch command,
        out string errorMessage)
    {
        errorMessage = string.Empty;
        if (args.Count < command.parameters.Where(x => x.isOptional == false).Count())
        {
            errorMessage = $"Branch {command.callName} do not have all required parameters";
            return false;
        }

        foreach (var arg in args)
        {
            string paramName = arg.Key;
            object paramValueStr = arg.Value;
            SSCommandParameter parameter = command.parameters.Find(x => x.name == paramName);
            if (!parameter)
            {
                errorMessage = $"Branch {command.callName} do not have parameter called '{paramName}'";
                return false;
            }

            try
            {
                paramValueStr = Convert.ChangeType(paramValueStr, parameter.type);
            }
            catch (InvalidCastException e)
            {
                errorMessage = $"{paramName} value is not in valid cast format, "
                               + $"can not cast {paramValueStr} to {parameter.type}.\n{e.Message}";
                return false;
            }
            catch (FormatException e)
            {
                errorMessage = $"{paramName} value is not in valid format of {parameter.type}.\n{e.Message}";
                return false;
            }
            catch (OverflowException e)
            {
                errorMessage =
                    $"{paramName} value represents a number that is out of the range of {parameter.type}.\n{e.Message}";
                return false;
            }
            catch (Exception e)
            {
                errorMessage = $"Unknown error casting {paramValueStr} to {parameter.type}.\n{e.Message}";
                return false;
            }
        }

        return true;
    }


    public static bool GetTooltip(out string result)
    {
        result = string.Empty;
        if (currentCommand.Replace(" ", "").Replace("ss", "").Length == 0) 
        {
            result = $"<u>Available commands:</u>\n{CommandsRouter.commandNames.GetString()}";
            return true;
        }

        if (!GetBranch(currentCommand, out var lastBranch, out var errorMessage))
        {
            result = errorMessage;
            return false;
        }

        var sb = new StringBuilder();
        sb.Append("<u>");
        sb.Append(lastBranch.displayName);
        sb.Append("</u>");
        sb.Append(" - ");
        sb.Append(lastBranch.description);
        if (lastBranch.branches.Count > 0)
        {
            sb.AppendLine(" Branches:");
            foreach (var branch in lastBranch.branches)
            {
                sb.Append("  <b>");
                sb.Append(branch.displayName);
                sb.Append("</b>");
                sb.Append(" - ");
                sb.Append(branch.description);
            }
        }

        if (lastBranch.parameters.Count > 0)
        {
            sb.AppendLine(" Parameters:");
            foreach (var parameter in lastBranch.parameters)
            {
                sb.Append("  <b>");
                sb.Append(parameter.displayName);
                sb.Append("</b>");
                sb.Append(parameter.isOptional ? " (optional)" : "");
                sb.Append(" - ");
                sb.Append(parameter.description);
            }
        }

        result = sb.ToString();
        return true;
    }

    public static List<string> GetOptions() { return new(); }
}