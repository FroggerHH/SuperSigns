namespace SuperSigns.Controllers;

public class HighlightCommand : CommandController
{
    public override ConsoleCommandException Execute(List<string> args)
    {
        Debug($"HighlightCommand executed");
        return null;
    }
}