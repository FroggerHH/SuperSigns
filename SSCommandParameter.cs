namespace SuperSigns;

[Serializable]
public class SSCommandParameter
{
    public readonly Type type = typeof(string);
    public readonly string name;
    public readonly string description;
    public readonly bool isOptional = false;

    public SSCommandParameter(string name, string description, bool isOptional)
    {
        this.name = name;
        this.description = description;
        this.isOptional = isOptional;
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