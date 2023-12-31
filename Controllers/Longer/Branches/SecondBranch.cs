﻿namespace SuperSigns.Controllers.Longer;

public class SecondBranch : SSCommandBranch
{
    public SecondBranch()
    {
        callName = "second";
        displayName = "Second (has params)";
        description = "g_g";
        // this.parentBranch = CommandsRouter.SS_commands["longer"];
        parameters = new List<SSCommandParameter>
        {
            new SSCommandParameter("message", "Message", "What to show on screen"),
            new SSCommandParameter("postfix", "", "", typeof(string), true)
        };
    }

    public override (CommandStatus, string) Execute(Dictionary<string, object> parameters)
    {
        object message;
        object postfix;
        parameters.TryGetValue("message", out message);
        parameters.TryGetValue("postfix", out postfix);

        m_localPlayer.Message(Center, message + (string)postfix);
        return (CommandStatus.Ok, default);
    }
}