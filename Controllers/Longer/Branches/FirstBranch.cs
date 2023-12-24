namespace SuperSigns.Controllers.Longer;

public class FirstBranch : SSCommandBranch
{
    public FirstBranch()
    {
        callName = "first";
        displayName = "First";
        description = "g_g";
        // this.parentBranch = CommandsRouter.SS_commands["longer"];
    }

    public override (CommandStatus, string) Execute(Dictionary<string, object> parameters)
    {
        m_localPlayer.Message(Center, displayName);
        return (CommandStatus.Ok, default);
    }
}