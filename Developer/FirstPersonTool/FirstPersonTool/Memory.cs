using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FirstPersonTool;

internal sealed class Memory
{
    private readonly IntPtr _processHandle;

    public Memory(IntPtr processHandle)
    {
        if (processHandle == IntPtr.Zero) throw new ArgumentException("Process handle was null.", nameof(processHandle));
        _processHandle = processHandle;
    }

    public IntPtr ReadPtr(IntPtr address)
    {
        var size = IntPtr.Size;
        var buf = ReadBytes(address, size);
        return size == 8
            ? new IntPtr(BitConverter.ToInt64(buf, 0))
            : new IntPtr(BitConverter.ToInt32(buf, 0));
    }

    public float ReadFloat(IntPtr address)
    {
        var buf = ReadBytes(address, 4);
        return BitConverter.ToSingle(buf, 0);
    }

    public byte ReadByte(IntPtr address)
    {
        var buf = ReadBytes(address, 1);
        return buf[0];
    }

    public void WriteFloat(IntPtr address, float value)
    {
        WriteBytes(address, BitConverter.GetBytes(value));
    }

    public void WriteByte(IntPtr address, byte value)
    {
        WriteBytes(address, new[] { value });
    }

    public byte[] ReadBytes(IntPtr address, int count)
    {
        var buf = new byte[count];
        if (!Win32.ReadProcessMemory(_processHandle, address, buf, count, out var read) || read.ToInt64() != count)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"ReadProcessMemory failed at 0x{address.ToInt64():X} ({count} bytes).");
        return buf;
    }

    public void WriteBytes(IntPtr address, byte[] bytes)
    {
        if (!Win32.WriteProcessMemory(_processHandle, address, bytes, bytes.Length, out var written) || written.ToInt64() != bytes.Length)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"WriteProcessMemory failed at 0x{address.ToInt64():X} ({bytes.Length} bytes).");
    }
}

