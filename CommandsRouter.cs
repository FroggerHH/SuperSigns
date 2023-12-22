using SuperSigns.Controllers;
using SuperSigns.Controllers.Ping;

namespace SuperSigns;

public static class CommandsRouter
{
    public static readonly Dictionary<string, SSCommandController> SS_commands = new();
    public static readonly List<string> commandNames;
    public static ConsoleCommandException currentCommandException;

    static CommandsRouter()
    {
        SS_commands = new();
        SS_commands.Add("ping", new PingSsCommand());
        commandNames = SS_commands.Keys.ToList();
    }

    public static (CommandStatus status, string exceptionMessage) RunCommand(string str)
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
            Debug($"Logic 0");
            if (!SS_commands.TryGetValue(commandName, out SSCommandController baseCommand))
            {
                Debug($"Logic 1, not found");
                return (CommandStatus.None, $"SS command {commandName} not found");
            }

            args.Remove(commandName);
            string exceptionMessage;
            var status = CommandStatus.None;

            // if (baseCommand.branches.Count == 0) (status, exceptionMessage) = baseCommand.Execute(str);
            // else
            // {
            Debug($"Logic 2");
            SSCommandBranch lastBranch = baseCommand;
            Dictionary<string, object> parameters = null;
            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                Debug($"Logic 2, arg = '{arg}'");
                //parameters are compatible with branches
                if (lastBranch!.parameters.Count != 0 && lastBranch.branches.Count != 0)
                    throw new Exception(
                        "SSCommandBranch can not have both parameters and branches at the same time. "
                        + $"Thrown by {lastBranch.callName}.");
                //If branch has parameters skip finding next branch and instead start collecting parameters
                if (lastBranch.parameters.Count != 0)
                {
                    parameters ??= new();
                    //Collecting parameters
                    var isParameter = arg.Contains(':');
                    var list_ = arg.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (!isParameter || list_.Count != 2)
                        return (CommandStatus.Error, $"Command parameter {arg} is not in valid format. "
                                                     + $"Should be 'parameterName:value'");
                    string paramName = list_[0];
                    string paramValueStr = list_[1];
                    var parameter = lastBranch.parameters.Find(x => x.name == paramName);
                    if (!parameter)
                        return (CommandStatus.Error,
                            $"Branch {lastBranch.callName} do not have parameter {paramName}");
                    if (parameters.ContainsKey(paramName))
                        return (CommandStatus.Error,
                            $"Parameter {paramName} has been given to the command {lastBranch.callName}");
                    object paramValue;

                    try
                    {
                        paramValue = Convert.ChangeType(paramValueStr, parameter.type);
                    }
                    catch (InvalidCastException e)
                    {
                        return (CommandStatus.Error,
                            $"{paramName} value is not in valid cast format, "
                            + $"can not cast {paramValueStr} to {parameter.type}.\n{e.Message}");
                    }
                    catch (FormatException e)
                    {
                        return (CommandStatus.Error,
                            $"{paramName} value is not in valid format of {parameter.type}.\n{e.Message}");
                    }
                    catch (OverflowException e)
                    {
                        return (CommandStatus.Error,
                            $"{paramName} value represents a number that is"
                            + $" out of the range of {parameter.type}.\n{e.Message}");
                    }
                    catch (Exception e)
                    {
                        return (CommandStatus.Error,
                            $"Unknown error casting {paramValueStr} to {parameter.type}.\n{e.Message}");
                    }

                    parameters.Add(paramName, paramValue);

                    //When we finally collected all parameters execute the branch
                    if (parameters.Count == lastBranch.parameters.Count)
                        return lastBranch.Execute(parameters);
                }

                //Finding the last branch
                if (lastBranch.branches.Count != 0)
                {
                    var command = lastBranch.branches.Find(x => x.callName == arg);
                    Debug($"Looking for nex branch, command = {command?.ToString() ?? "null"}");
                    if (!command) return (CommandStatus.None, $"Command branch with name {arg} do not exists");
                    lastBranch = command;
                    continue;
                }
                //If branch have neither branches and neither parameters just execute it
                else return lastBranch.Execute(new());
            }

            Debug($"Logic 3, lastBranch = '{lastBranch?.ToString() ?? "null"}'");

            return (CommandStatus.Error, "Unknown ERROR");
            // }
        }
    }


    public static string GetTooltip(string commandline) { return ""; }

    public static List<string> GetOptions(string commandline) { return new(); }
}