using System.Globalization;

namespace FirstPersonTool;

internal sealed class FpCameraHeight
{
    private const float MinLift = 0f;
    private const float MaxLift = 3.0f;

    private readonly OffsetsConfig.FpHeightSection _cfg;
    private readonly HashSet<int> _denylist;
    private readonly List<TrackedField> _tracked = new();
    private bool _active;

    public FpCameraHeight(OffsetsConfig.FpHeightSection cfg, IEnumerable<int> denylistOffsets)
    {
        _cfg = cfg;
        _denylist = new HashSet<int>(denylistOffsets);
    }

    public float EffectiveLift =>
        Math.Clamp(_cfg.EyeLiftMeters * Math.Clamp(_cfg.LiftScale, 0.25f, 1f), MinLift, MaxLift);

    public bool HasConfiguredOffset =>
        TryParseOffset(_cfg.PrimaryOffsetHex, out var off) && off != 0;

    public int TrackedCount => _tracked.Count;

    public void OnFpEnable(CameraAccess cam)
    {
        _tracked.Clear();
        _active = true;

        var offsets = CollectOffsetsToTrack();
        foreach (var off in offsets)
            TryTrack(cam, off);
    }

    public void OnFpDisable(CameraAccess cam)
    {
        if (_active)
            RestoreAll(cam);
        _tracked.Clear();
        _active = false;
    }

    public void Rebaseline(CameraAccess cam)
    {
        if (!_active) return;
        _tracked.Clear();
        OnFpEnable(cam);
    }

    public void Apply(CameraAccess cam)
    {
        if (!_active || _tracked.Count == 0) return;

        var lift = EffectiveLift;
        foreach (var t in _tracked)
        {
            try
            {
                cam.SetFloatAt(t.Offset, t.Baseline + lift);
            }
            catch
            {
                // skip
            }
        }
    }

    public string GetDiag(CameraAccess cam)
    {
        if (!_active || _tracked.Count == 0)
            return "height not active";

        var lift = EffectiveLift;
        var t = _tracked[0];
        try
        {
            var target = t.Baseline + lift;
            var mem = cam.GetFloatAt(t.Offset);
            var delta = mem - t.Baseline;
            var ok = MathF.Abs(mem - target) <= MathF.Max(0.08f, lift * 0.15f);
            return $"0x{t.Offset:X} base={t.Baseline:0.###} mem={mem:0.###} d={delta:0.###} {(ok ? "OK" : "BLOCKED")} n={_tracked.Count}";
        }
        catch
        {
            return "height read failed";
        }
    }

    public void RestoreAll(CameraAccess cam)
    {
        foreach (var t in _tracked)
        {
            try
            {
                cam.SetFloatAt(t.Offset, t.Baseline);
            }
            catch
            {
                // ignore
            }
        }
    }

    private List<int> CollectOffsetsToTrack()
    {
        var list = new List<int>();

        if (TryParseOffset(_cfg.PrimaryOffsetHex, out var primary) && primary != 0 && IsAllowed(primary))
            list.Add(primary);

        if (_cfg.ApplyBandLift)
        {
            foreach (var off in PivotSweep.BandOffsets())
            {
                if (!IsAllowed(off) || list.Contains(off)) continue;
                list.Add(off);
            }
        }

        return list;
    }

    private void TryTrack(CameraAccess cam, int offset)
    {
        try
        {
            // Always capture live vanilla baseline when FP turns on (ignore stale json baseline).
            var baseline = cam.GetFloatAt(offset);
            _tracked.Add(new TrackedField(offset, baseline));
        }
        catch
        {
            // unreadable
        }
    }

    private bool IsAllowed(int offset) => !_denylist.Contains(offset);

    public static bool TryParseOffset(string? hex, out int offset)
    {
        offset = 0;
        if (string.IsNullOrWhiteSpace(hex)) return false;
        var s = hex.Trim();
        if (s.Equals("0x0", StringComparison.OrdinalIgnoreCase) || s == "0") return false;
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            s = s[2..];
        if (!int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset))
            return false;
        return offset != 0;
    }

    public static HashSet<int> BuildDenylist(OffsetsConfig cfg)
    {
        var set = new HashSet<int>();
        foreach (var f in cfg.Fields.Values)
        {
            try
            {
                set.Add(checked((int)cfg.FieldOffsetFromHex(f.OffsetHex)));
            }
            catch
            {
                // ignore
            }
        }
        return set;
    }

    private readonly record struct TrackedField(int Offset, float Baseline);
}
