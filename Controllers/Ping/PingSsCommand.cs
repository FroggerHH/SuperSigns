namespace SuperSigns.Controllers.Ping;

public class PingSsCommand : SSCommandController
{
    public PingSsCommand()
    {
        Debug($"Init of PingSsCommand");
        callName = "ping";
        displayName = "Ping";
        description = "Fun ping-pong command";
        // parentBranch = null;
        branches.Add(new PongBranch());
        branches.Add(new NoPongBranch());
    }
}