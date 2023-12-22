namespace SuperSigns.Controllers;

public class PingSsCommand : SSCommandController
{
    public override ConsoleCommandException Execute(List<string> args)
    {
        Debug($"PingSsCommand executed");
        m_localPlayer.Message(Center, "Pong");
        return null;
    }
}