namespace SuperSigns.Controllers.Ping;

public class PongBranch : SSCommandBranch
{
    public PongBranch(SSCommandBranch parentBranch) : base(parentBranch)
    {
        Debug($"Init of PongBranch");
        callName = "pong";
        displayName = "Pong";
        description = "Returns pong";
        this.parentBranch = parentBranch;
    }

    public override (CommandStatus, string) Execute(Dictionary<string, object> parameters)
    {
        m_localPlayer.Message(Center, displayName);
        return (CommandStatus.Ok, default);
    }
}