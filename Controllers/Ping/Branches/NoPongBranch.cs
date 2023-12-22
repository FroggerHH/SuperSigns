namespace SuperSigns.Controllers.Ping;

public class NoPongBranch : SSCommandBranch
{
    public NoPongBranch(SSCommandBranch parentBranch) : base(parentBranch)
    {
        callName = "nopong";
        displayName = "No Pong";
        description = "Returns nothing looking like a pong";
        this.parentBranch = parentBranch;
    }

    public override (CommandStatus, string) Execute(Dictionary<string, object> parameters)
    {
        m_localPlayer.Message(Center, displayName);
        return (CommandStatus.Ok, default);
    }
}