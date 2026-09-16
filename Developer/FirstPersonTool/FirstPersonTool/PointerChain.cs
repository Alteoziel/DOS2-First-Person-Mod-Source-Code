namespace FirstPersonTool;

internal static class PointerChain
{
    public static IntPtr Resolve(Memory mem, IntPtr baseAddress, IReadOnlyList<nint> offsets)
    {
        var p = baseAddress;
        for (var i = 0; i < offsets.Count; i++)
        {
            p = mem.ReadPtr(p);
            if (p == IntPtr.Zero) return IntPtr.Zero;
            p = IntPtr.Add(p, checked((int)offsets[i]));
        }
        return p;
    }

    public static IntPtr ResolveFromModule(Memory mem, IntPtr moduleBase, nint moduleOffset, IReadOnlyList<nint> offsets)
    {
        return Resolve(mem, IntPtr.Add(moduleBase, checked((int)moduleOffset)), offsets);
    }
}

