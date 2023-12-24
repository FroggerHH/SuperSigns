namespace SuperSigns;

public class SSCommandBranch
{
    public string callName { get; protected set; }
    public string displayName { get; protected set; }
    public string description { get; protected set; }
    public bool canBeCalled => branches.Count == 0;
    public List<SSCommandBranch> branches { get; protected set; } = new();
    public List<SSCommandParameter> parameters { get; protected set; } = new();
    //public SSCommandBranch parentBranch { get; protected set; } = null;

    public virtual (CommandStatus, string) Execute(Dictionary<string, object> parameters) =>
        (CommandStatus.Error, "NotImplemented");

    protected SSCommandBranch() { }

    public static implicit operator bool(SSCommandBranch branch) => branch is not null;

    public override string ToString() =>
        $"callName: {callName}, displayName: {displayName}, "
        + $"branches: {branches?.Select(x => x.callName).GetString() ?? "null"}, "
        + $"parameters: {parameters?.Select(x => x.name).GetString() ?? "null"}";
}