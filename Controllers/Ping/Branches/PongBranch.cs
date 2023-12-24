namespace SuperSigns.Controllers.Ping;

public class PongBranch : SSCommandBranch
{
    public PongBranch()
    {
        callName = "pong";
        displayName = "Pong";
        description = "Returns pong";
        // this.parentBranch = CommandsRouter.SS_commands["ping"];
    }

    public override (CommandStatus, string) Execute(Dictionary<string, object> parameters)
    {
        m_localPlayer.Message(Center, displayName);
        return (CommandStatus.Ok, default);
    }
}