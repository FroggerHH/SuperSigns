namespace SuperSigns;

public static class CodeSign
{
    private static readonly HashSet<Sign> Signs = new();

    public static HashSet<Sign> GetSigns()
    {
        Signs.RemoveWhere(x => x == null);
        return Signs;
    }

    public static void AddSign(Sign sign) { Signs.Add(sign); }
}