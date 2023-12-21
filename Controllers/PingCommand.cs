namespace SuperSigns.Controllers;

public class PingCommand : CommandController
{
    public override ConsoleCommandException Execute(List<string> args)
    {
        Debug($"PingCommand executed");
        m_localPlayer.Message(Center, "Pong");
        return null;
    }
}