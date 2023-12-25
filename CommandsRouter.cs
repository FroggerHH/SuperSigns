using System.Text;
using SuperSigns.Controllers;
using SuperSigns.Controllers.Longer;
using SuperSigns.Controllers.Message;
using SuperSigns.Controllers.Ping;

namespace SuperSigns;

public static class CommandsRouter
{
    private static readonly List<SSCommandController> commands;
    public static readonly Dictionary<string, SSCommandController> SS_commands;
    public static readonly List<string> commandNames;
    public static string currentCommand;

    static CommandsRouter()
    {
        SS_commands = new();
        commands = new();

        commands.Add(new PingSsCommand());
        commands.Add(new LongerSSCommand());
        commands.Add(new MessageSSCommand());

        commandNames = commands.Select(x => x.callName).ToList();
        SS_commands = commands.ToDictionary(x => x.callName, x => x);
    }

    public static (CommandStatus status, string exceptionMessage) TryRunCommand(string str)
    {
        str = str.Replace("ss ", "");
        Debug($"Got a ss command for execution -> '{str}'");
        var args = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        var commandName = args[0];
        Debug($"commandName = '{commandName}'");
        var logic = Logic();
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
            if (!SS_commands.TryGetValue(commandName, out var baseCommand))
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
            if (!CheckParameters(ref parameters, lastBranch, out exceptionMessage))
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
        result = new Dictionary<string, object>();

        foreach (var arg in args)
        {
            var list_ = arg.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (list_.Count != 2)
            {
                errorMessage = $"Command parameter {arg} is not in valid format. Should be 'parameterName:value'";
                Debug($"GetParameters -> {errorMessage}");
                return false;
            }

            var paramName = list_[0];
            var paramValueStr = list_[1];
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

    public static bool CheckParameters(ref Dictionary<string, object> args, SSCommandBranch command,
        out string errorMessage)
    {
        errorMessage = string.Empty;
        if (args.Count < command.parameters.Where(x => x.isOptional == false).Count())
        {
            errorMessage = $"Branch {command.callName} do not have all required parameters";
            return false;
        }

        var newArgs = new Dictionary<string, object>(args);

        foreach (var arg in args)
        {
            var paramName = arg.Key;
            var paramValue = arg.Value;
            var parameter = command.parameters.Find(x => x.name == paramName);
            if (!parameter)
            {
                errorMessage = $"Branch {command.callName} do not have parameter called '{paramName}'";
                return false;
            }

            try
            {
                paramValue = Convert.ChangeType(paramValue, parameter.type);
            }
            catch (InvalidCastException e)
            {
                errorMessage = $"{paramName} value is not in valid cast format, "
                               + $"can not cast {paramValue} to {parameter.type}.\n{e.Message}";
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
                errorMessage = $"Unknown error casting {paramValue} to {parameter.type}.\n{e.Message}";
                return false;
            }

            newArgs.Add(paramName, paramValue);
            Debug($"Parameter {paramName} = {paramValue}, type = {paramValue.GetType()}");
        }

        args = newArgs;

        return true;
    }


    public static bool GetTooltip(out string result)
    {
        result = string.Empty;
        var sb = new StringBuilder();
        var available = "<u>Available commands:</u>\n";
        if (currentCommand.Replace(" ", "").Replace("ss", "").Length == 0)
        {
            result = available + $"{commandNames.GetString()}";
            return true;
        }

        SSCommandBranch lastBranch;
        string errorMessage;
        var branchFound = GetBranch(currentCommand, out lastBranch, out errorMessage);
        if (currentCommand.EndsWith(" "))
        {
            if (!branchFound)
            {
                result = errorMessage;
                return false;
            }

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
                    sb.Append(branch.description + "\n");
                }
            } else if (lastBranch.parameters.Count > 0)
            {
                sb.AppendLine(" Parameters:");
                foreach (var parameter in lastBranch.parameters)
                {
                    sb.Append("  <b>");
                    sb.Append(parameter.displayName);
                    sb.Append("</b>");
                    sb.Append(parameter.isOptional ? " (optional)" : "");
                    sb.Append(" - ");
                    sb.Append(parameter.description + "\n");
                }
            }
        } else
        {
            var args = currentCommand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (args.Count == 0)
            {
                result = "Error 257";
                return false;
            }

            args.Remove(args.Last());
            var newCommand = string.Join(" ", args);
            if (!GetBranch(newCommand, out lastBranch, out errorMessage))
            {
                result = errorMessage;
                return false;
            }

            sb.AppendLine(available);
        }

        result = sb.ToString();
        return true;
    }

    public static List<string> GetOptions() { return new List<string>(); }
}