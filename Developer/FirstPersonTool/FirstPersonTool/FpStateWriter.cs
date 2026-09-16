using System.Text.Json;

namespace FirstPersonTool;

internal sealed class FpStateWriter
{
    private readonly string _path;
    private bool? _lastWritten;

    public FpStateWriter()
    {
        // %APPDATA%\Larian Studios\Divinity Original Sin 2 Definitive Edition\Script Extender\FirstPersonTool\state.json
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _path = Path.Combine(appData, "Larian Studios", "Divinity Original Sin 2 Definitive Edition", "Script Extender", "FirstPersonTool", "state.json");
    }

    public void WriteEnabled(bool enabled)
    {
        if (_lastWritten.HasValue && _lastWritten.Value == enabled) return;
        _lastWritten = enabled;

        var dir = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(dir);

        var payload = new
        {
            enabled,
            updatedUtc = DateTime.UtcNow.ToString("O")
        };

        var json = JsonSerializer.Serialize(payload);
        File.WriteAllText(_path, json);
    }
}

