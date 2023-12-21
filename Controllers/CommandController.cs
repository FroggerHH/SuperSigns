namespace SuperSigns.Controllers;

public abstract class CommandController
{
    public abstract ConsoleCommandException Execute(List<string> args);
}