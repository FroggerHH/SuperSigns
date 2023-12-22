namespace SuperSigns.Controllers;

public abstract class SSCommandController
{
    public abstract ConsoleCommandException Execute(List<string> args);
}