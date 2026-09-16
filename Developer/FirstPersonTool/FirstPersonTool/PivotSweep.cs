namespace FirstPersonTool;

internal static class PivotSweep
{
    public static IReadOnlyList<int> BandOffsets(int start = 0xC28, int end = 0xCB0, int step = 4)
    {
        var list = new List<int>();
        for (var o = start; o <= end; o += step)
            list.Add(o);
        return list;
    }
}
