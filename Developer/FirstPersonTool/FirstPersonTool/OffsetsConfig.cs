using System.Globalization;
using System.Text.Json;

namespace FirstPersonTool;

internal sealed class OffsetsConfig
{
    public sealed class GameSection
    {
        public string ProcessName { get; set; } = "EoCApp";
        public string ModuleName { get; set; } = "EoCApp.exe";
    }

    public sealed class CameraRootSection
    {
        public string Type { get; set; } = "pointerChain";
        public string ModuleOffsetHex { get; set; } = "0x0";
        public List<string> OffsetsHex { get; set; } = new();
    }

    public sealed class FieldSection
    {
        public string OffsetHex { get; set; } = "0x0";
        public string Type { get; set; } = "float";
    }

    public sealed class FpHeightSection
    {
        public string PrimaryOffsetHex { get; set; } = "0x0";
        /// <summary>Vanilla value at primaryOffsetHex when height was saved (NaN = capture on FP enable).</summary>
        public float PivotBaseline { get; set; } = float.NaN;
        public float EyeLiftMeters { get; set; } = 0.8f;
        public float LiftScale { get; set; } = 1.0f;
        /// <summary>Also lift every offset in the pivot band (more reliable if primary alone does nothing).</summary>
        public bool ApplyBandLift { get; set; } = true;
    }

    public sealed class FpLookSection
    {
        /// <summary>Right-stick yaw speed (radians/sec at full deflection) via CameraAngle.</summary>
        public float Sensitivity { get; set; } = 13.0f;
        /// <summary>BetterCamera ScrollSpeed (mouse edge pan). 0 = leave unchanged.</summary>
        public float ScrollSpeed { get; set; } = 0f;
        /// <summary>BetterCamera ZoomSpeed. 0 = leave unchanged.</summary>
        public float ZoomSpeed { get; set; } = 0f;
        public float PitchNeutralOffset { get; set; } = -0.06f;
        public float PitchDownOffset { get; set; } = -0.02f;
        public float PitchUpOffset { get; set; } = 0.02f;
        /// <summary>Field of view degrees (Fov @ 0xC74). 0 = use built-in default.</summary>
        public float Fov { get; set; }
        /// <summary>Camera near zoom (MinDistance + MinDistance2). 0 = use built-in default.</summary>
        public float NearMinDistance { get; set; }
        /// <summary>Camera far zoom cap (MaxDistance + MaxDistance2). 0 = use built-in default.</summary>
        public float NearMaxDistance { get; set; }
        /// <summary>Tactical view endpoints; 0 = copy near min/max distances.</summary>
        public float TacticalViewMin { get; set; }
        public float TacticalViewMax { get; set; }
    }

    public GameSection Game { get; set; } = new();
    public CameraRootSection CameraRoot { get; set; } = new();
    public Dictionary<string, FieldSection> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public FpHeightSection FpHeight { get; set; } = new();
    public FpLookSection FpLook { get; set; } = new();

    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static OffsetsConfig LoadOrThrow(string path)
    {
        var json = File.ReadAllText(path);
        var cfg = JsonSerializer.Deserialize<OffsetsConfig>(json, JsonOptions);
        if (cfg is null) throw new InvalidOperationException("Failed to parse camera_offsets.json.");
        return cfg;
    }

    public nint CameraRootModuleOffset() => ParseHexNint(CameraRoot.ModuleOffsetHex);

    public IReadOnlyList<nint> CameraRootOffsets() =>
        CameraRoot.OffsetsHex.Select(ParseHexNint).ToArray();

    public nint FieldOffset(string name)
    {
        if (!Fields.TryGetValue(name, out var f))
            throw new KeyNotFoundException($"Field '{name}' missing in camera_offsets.json.");
        return ParseHexNint(f.OffsetHex);
    }

    public int FieldOffsetFromHex(string offsetHex) => checked((int)ParseHexNint(offsetHex));

    public static void SaveFpHeight(
        string offsetsPath,
        string primaryOffsetHex,
        float pivotBaseline,
        float eyeLiftMeters,
        float liftScale)
    {
        var cfg = JsonSerializer.Deserialize<OffsetsConfig>(File.ReadAllText(offsetsPath), JsonOptions)
                  ?? throw new InvalidOperationException("Failed to parse camera_offsets.json.");
        cfg.FpHeight.PrimaryOffsetHex = primaryOffsetHex;
        cfg.FpHeight.PivotBaseline = pivotBaseline;
        cfg.FpHeight.EyeLiftMeters = eyeLiftMeters;
        cfg.FpHeight.LiftScale = liftScale;
        File.WriteAllText(offsetsPath, JsonSerializer.Serialize(cfg, JsonOptions));
    }

    private static nint ParseHexNint(string s)
    {
        s = s.Trim();
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            s = s[2..];
        return (nint)long.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }
}

