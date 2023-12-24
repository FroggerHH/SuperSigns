namespace SuperSigns.Controllers.Longer;

public class FirstPodBranch : SSCommandBranch
{
    public FirstPodBranch()
    {
        callName = "first_pod";
        displayName = "First Pod";
        description = "g_g";
        // this.parentBranch = parentBranch;
    }

    public override (CommandStatus, string) Execute(Dictionary<string, object> parameters)
    {
        m_localPlayer.Message(Center, displayName);
        return (CommandStatus.Ok, default);
    }
}