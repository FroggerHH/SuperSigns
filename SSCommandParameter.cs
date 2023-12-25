namespace SuperSigns;

[Serializable]
public class SSCommandParameter
{
    public readonly Type type;
    public readonly string name;
    public readonly string displayName;
    public readonly string description;
    public readonly bool isOptional = false;
    public readonly bool hardChoose = false;
    private readonly List<object> acceptableValues;

    public SSCommandParameter(string name, string displayName, string description, Type type = null,
        bool isOptional = false, bool hardChoose = false, List<object> acceptableValues = null)
    {
        this.name = name;
        this.type = type ?? typeof(string);
        this.displayName = displayName;
        this.description = description;
        this.isOptional = isOptional;
        this.hardChoose = hardChoose;
        this.acceptableValues = acceptableValues ?? new();
    }

    public override string ToString()
    {
        var s = $"Type: {type}, Name: {name}";
        if (isOptional) s += $", IsOptional: {isOptional}";
        return s;
    }

    public static implicit operator bool(SSCommandParameter parameter) => parameter is not null;

    // [Serializable]
    // struct ParameterValue<T>
    // {
    //     public T value;
    // }
}