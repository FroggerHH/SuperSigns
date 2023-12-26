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
        SS_commands = new Dictionary<string, SSCommandController>();
        commands = new List<SSCommandController>();

        commands.Add(new PingSsCommand());
        commands.Add(new LongerSSCommand());
        commands.Add(new MessageSSCommand());

        commandNames = commands.Select(x => x.callName).ToList();
        SS_commands = commands.ToDictionary(x => x.callName, x => x);
    }

    public static (CommandStatus status, string exceptionMessage) TryRunCommand(string str)
    {
        str = str.Replace("ss ", "");
        var args = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        var commandName = args[0];
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
        if (commandline.Replace(" ", "") == "ss")
        {
            errorMessage = "SS is not a command itself. Use one of its branches";
            return false;
        }

        if (!SS_commands.TryGetValue(args[0], out var baseCommand))
        {
            errorMessage = $"Command {args[0]} not found";
            return false;
        }

        args.Remove(args[0]);

        lastBranch = baseCommand;
        foreach (var arg in args)
        {
            var command = lastBranch.branches.Find(x => x.callName == arg);
            if (!command)
            {
                errorMessage = $"Command branch with callName {arg} do not exists in {lastBranch.callName} command";
                return false;
            }

            lastBranch = command;
        }

        if (lastBranch.canBeCalled == false)
        {
            errorMessage = $"Command {lastBranch.callName} can not be called directly. Use one of its branches";
            return false;
        }

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
            var parameter = command.parameters.Find(x => x.callName == paramName);
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

            newArgs[paramName] = paramValue;
        }

        args = newArgs;

        return true;
    }


    public static bool GetTooltip(out string result)
    {
        result = string.Empty;
        var sb = new StringBuilder();
        var available = "<u>Available commands:</u>";
        var noSS = currentCommand.Replace("ss ", "").Replace("ss", "");
        var noSpace = noSS.Replace(" ", "");
        if (noSpace.Length == 0)
        {
            sb.Append(available).AppendLine();
            result = sb.ToString();
            return true;
        }

        SSCommandBranch lastBranch;
        string errorMessage;
        var branchFound = GetBranch(currentCommand, out lastBranch, out errorMessage);
        var parameters = GetParameters();
        if (noSS.EndsWith(" "))
        {
            if (!branchFound)
            {
                result = errorMessage;
                return false;
            }

            sb.Append("<u>").Append(lastBranch.displayName).Append("</u>").Append(" - ").Append(lastBranch
                .description).AppendLine();
            result = sb.ToString();
            return true;
            // if (lastBranch.branches.Count > 0)
            // {
            //     sb.AppendLine(" Branches:");
            //     foreach (var branch in lastBranch.branches)
            //     {
            //         sb.Append("  <b>");
            //         sb.Append(branch.displayName);
            //         sb.Append("</b>");
            //         // sb.Append(" - ");
            //         // sb.Append(branch.description + "\n");
            //     }
            //
            //     sb.AppendLine();
            // } else if (lastBranch.parameters.Count > 0)
            // {
            //     sb.AppendLine(" Parameters:");
            //     sb.AppendLine(lastBranch.parameters
            //             .Select(x => $"  <b>{x.displayName}</b> {(x.isOptional ? "(optional)" : "")}").GetString())
            //         .AppendLine();
            //     // foreach (var parameter in lastBranch.parameters)
            //     // {
            //     //     sb.Append("  <b>");
            //     //     sb.Append(parameter.displayName);
            //     //     sb.Append("</b>");
            //     //     sb.Append(parameter.isOptional ? " (optional)" : "");
            //     //     // sb.Append(" - ");
            //     //     // sb.Append(parameter.description + "\n");
            //     // }
            // }
        }

        if (parameters.Count != 0 && branchFound)
        {
            var par = parameters.Last();
            var commandParameter = lastBranch.parameters.Find(x => x.callName == par.Key);

            if (!commandParameter)
            {
                result = $"Parameter {par.Key} not found";
                return false;
            }

            sb.Append(commandParameter.displayName).Append(" - ").Append(commandParameter.description)
                .Append(commandParameter.isOptional ? "(optional)" : "").AppendLine();
            if (!commandParameter.hardChoose)
                sb.Append("Accepts any value type of ").AppendLine(commandParameter.type.Name);

            result = sb.ToString();
            return true;
        }

        var newCommand = GetCommandWithoutLastBranch(out result);
        if (!newCommand.IsGood())
            return false;
        if (newCommand.Replace(" ", "") == "ss")
        {
            result = available + "\n";
            return true;
        }

        branchFound = GetBranch(newCommand, out lastBranch, out errorMessage);
        if (!branchFound)
        {
            result = errorMessage;
            return false;
        }

        sb.Append("<u>").Append(lastBranch.displayName).Append("</u>").Append(" - ").Append(lastBranch.description)
            .AppendLine();
        result = sb.ToString();
        return true;
    }

    private static string GetCommandWithoutLastBranch(out string error)
    {
        error = string.Empty;
        var args = currentCommand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (args.Count == 0)
        {
            error = "Error else split args";
            return string.Empty;
        }

        args.Remove(args.Last());
        var newCommand = string.Join(" ", args);
        return newCommand;
    }

    //"ss message text:someMsg "
    //"message text:someMsg "

    private static Dictionary<string, string> GetParameters()
    {
        return currentCommand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.Contains(':')).ToDictionary(x => x.Split(':')[0], x =>
            {
                var split = x.Split(':');
                return split.Length > 1 ? split[1] : "";
            });
    }

    public static List<string> GetOptions()
    {
        var result = new List<string>();
        var noSS = currentCommand.Replace("ss ", "").Replace("ss", "");
        var noSpace = noSS.Replace(" ", "");
        if (currentCommand.Replace(" ", "") == "ss")
            return commandNames;
        if (noSpace.Length == 0)
            return result;

        SSCommandBranch lastBranch;
        string errorMessage;
        var branchFound = GetBranch(currentCommand, out lastBranch, out errorMessage);
        var parameters = GetParameters();
        if (noSS.EndsWith(" "))
        {
            if (!branchFound) return result;
            if (lastBranch.branches.Count > 0) return lastBranch.branches.Select(x => x.callName).ToList();
            if (lastBranch.parameters.Count > 0) return lastBranch.parameters.Select(x => x.callName).ToList();
            return result;
        }

        if (parameters.Count != 0 && branchFound)
        {
            var par = parameters.Last();
            var commandParameter = lastBranch.parameters.Find(x => x.callName == par.Key);
            if (!commandParameter) return result;

            if (commandParameter.hardChoose)
                return commandParameter.acceptableValues.Select(x => x.ToString()).ToList();

            return result;
        }

        var newCommand = GetCommandWithoutLastBranch(out var error);
        if (!newCommand.IsGood())
            return result;

        if (newCommand.Replace(" ", "") == "ss")
            return commandNames;

        branchFound = GetBranch(newCommand, out lastBranch, out errorMessage);
        if (!branchFound)
            return result;

        if (lastBranch.branches.Count > 0) return lastBranch.branches.Select(x => x.callName).ToList();
        if (lastBranch.parameters.Count > 0)
            return lastBranch.parameters.Select(x => $"{x.callName}:").ToList();

        return result;
    }
}