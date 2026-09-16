using System.Diagnostics;

namespace FirstPersonTool;

internal sealed class Dos2Session : IDisposable
{
    public int ProcessId => _process?.Id ?? 0;
    public IntPtr ProcessHandle => _handle;
    public Memory Memory { get; }

    private readonly Process? _process;
    private IntPtr _handle;

    private Dos2Session(Process process, IntPtr handle)
    {
        _process = process;
        _handle = handle;
        Memory = new Memory(_handle);
    }

    public static Dos2Session AttachOrThrow(string processNameWithoutExe)
    {
        var candidates = Process.GetProcessesByName(processNameWithoutExe);
        if (candidates.Length == 0)
            throw new InvalidOperationException($"Process '{processNameWithoutExe}.exe' not found. Start the game and load a save first.");

        // Prefer the first; if multiple, user can close extras.
        var p = candidates[0];
        var handle = Win32.OpenProcess(
            Win32.PROCESS_VM_READ | Win32.PROCESS_VM_WRITE | Win32.PROCESS_VM_OPERATION | Win32.PROCESS_QUERY_INFORMATION,
            false,
            p.Id);

        if (handle == IntPtr.Zero)
            throw new InvalidOperationException("OpenProcess failed. Try running the tool with same admin privileges as the game.");

        return new Dos2Session(p, handle);
    }

    public IntPtr GetModuleBaseAddressOrThrow(string moduleName)
    {
        if (_process is null) throw new ObjectDisposedException(nameof(Dos2Session));
        _process.Refresh();

        foreach (ProcessModule? m in _process.Modules)
        {
            if (m is null) continue;
            if (string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
                return m.BaseAddress;
        }

        throw new InvalidOperationException($"Module '{moduleName}' not found in process. Is the game fully started?");
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            Win32.CloseHandle(_handle);
            _handle = IntPtr.Zero;
        }
        _process?.Dispose();
    }
}

