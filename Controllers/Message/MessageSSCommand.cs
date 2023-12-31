﻿namespace SuperSigns.Controllers.Message;

public class MessageSSCommand : SSCommandController
{
    public MessageSSCommand()
    {
        callName = "message";
        displayName = "Message";
        description = "Displays a message on the screen";
        parameters = new List<SSCommandParameter>
        {
            new SSCommandParameter("messagetype", "MessageType", "Where the message will be displayed on the screen",
                typeof(string), false, true, new List<object> { "center", "topleft" }),
            new SSCommandParameter("text", "Text", "The text that will be displayed on the screen"),
            new SSCommandParameter("player", "Target player nick",
                "Which player will be shown the message, leave it blank for the local player", typeof(string), true),
            new SSCommandParameter("prefix", "Prefix", "It will be added to the beginning of the message",
                typeof(string), true),
            new SSCommandParameter("postfix", "Postfix", "It will be added to the end of the message", typeof(string),
                true)
        };
    }

    public override (CommandStatus, string) Execute(Dictionary<string, object> parameters)
    {
        MessageHud.MessageType messageType;
        object text;
        object playerName;
        object prefix;
        object postfix;

        parameters.TryGetValue("messagetype", out var messageTypeObj);
        parameters.TryGetValue("text", out text);
        parameters.TryGetValue("playerName", out playerName);
        parameters.TryGetValue("postfix", out postfix);
        parameters.TryGetValue("prefix", out prefix);
        messageType = messageTypeObj switch
        {
            "center" => Center,
            "topleft" => TopLeft,
            _ => Center
        };

        m_localPlayer.Message(messageType, (string)prefix + text + postfix);
        return (CommandStatus.Ok, default);
    }
}