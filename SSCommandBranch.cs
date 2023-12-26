namespace SuperSigns;

public class SSCommandBranch
{
    protected SSCommandBranch() { }
    public string callName { get; protected set; }
    public string displayName { get; protected set; }
    public string description { get; protected set; }
    public bool canBeCalled => branches.Count == 0;
    public List<SSCommandBranch> branches { get; protected set; } = new();

    public List<SSCommandParameter> parameters { get; protected set; } = new();
    //public SSCommandBranch parentBranch { get; protected set; } = null;

    public virtual (CommandStatus, string) Execute(Dictionary<string, object> parameters)
    {
        return (CommandStatus.Error, "NotImplemented");
    }

    public static implicit operator bool(SSCommandBranch branch) { return branch is not null; }

    public override string ToString()
    {
        return $"callName: {callName}, displayName: {displayName}, "
               + $"branches: {branches?.Select(x => x.callName).GetString() ?? "null"}, "
               + $"parameters: {parameters?.Select(x => x.callName).GetString() ?? "null"}";
    }
}