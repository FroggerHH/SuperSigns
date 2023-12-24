namespace SuperSigns.Controllers.Longer;

public class LongerSSCommand : SSCommandController
{
    public LongerSSCommand()
    {
        callName = "longer";
        displayName = "Longer";
        description = "💦";
        // parentBranch = null;
        branches.Add(new FirstBranch());
        branches.Add(new SecondBranch());
    }
}