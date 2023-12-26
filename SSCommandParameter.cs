namespace SuperSigns;

[Serializable]
public class SSCommandParameter
{
    public readonly List<object> acceptableValues;
    public readonly string callName;
    public readonly string description;
    public readonly string displayName;
    public readonly bool hardChoose;
    public readonly bool isOptional;
    public readonly Type type;

    public SSCommandParameter(string callName, string displayName, string description, Type type = null,
        bool isOptional = false, bool hardChoose = false, List<object> acceptableValues = null)
    {
        this.callName = callName;
        this.type = type ?? typeof(string);
        this.displayName = displayName;
        this.description = description;
        this.isOptional = isOptional;
        this.hardChoose = hardChoose;
        this.acceptableValues = acceptableValues ?? new List<object>();
    }

    public override string ToString()
    {
        var s = $"callName: {callName}, type: {type.FullName}, hardChoose: {hardChoose}";
        if (isOptional) s += $", IsOptional: {isOptional}";
        return s;
    }

    public static implicit operator bool(SSCommandParameter parameter) { return parameter is not null; }

    // [Serializable]
    // struct ParameterValue<T>
    // {
    //     public T value;
    // }
}