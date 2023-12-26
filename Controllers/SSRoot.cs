namespace SuperSigns.Controllers;

public class SSRoot: SSCommandController
{
    public SSRoot()
    {
        callName = "ss";
        displayName = "ss";
        description = "The root command for all ss commands";
    }
}