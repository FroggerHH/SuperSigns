namespace SuperSigns.Controllers;

public class HighlightSsCommand : SSCommandController
{
    public override ConsoleCommandException Execute(List<string> args)
    {
        Debug($"HighlightSsCommand executed");
        return null;
    }
}