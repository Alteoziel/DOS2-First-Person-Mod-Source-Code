namespace FirstPersonTool;

internal sealed class CameraAccess
{
    private readonly Memory _mem;
    private readonly OffsetsConfig _cfg;
    private IntPtr _root;

    public IntPtr Root => _root;

    public CameraAccess(Memory mem, OffsetsConfig cfg, IntPtr root)
    {
        _mem = mem;
        _cfg = cfg;
        SetRoot(root);
    }

    public void SetRoot(IntPtr root)
    {
        if (root == IntPtr.Zero) throw new ArgumentException("Camera root pointer was null.", nameof(root));
        _root = root;
    }

    private IntPtr FieldAddr(string name) => IntPtr.Add(_root, checked((int)_cfg.FieldOffset(name)));

    public float GetFloat(string name) => _mem.ReadFloat(FieldAddr(name));
    public void SetFloat(string name, float value) => _mem.WriteFloat(FieldAddr(name), value);

    public float GetFloatAt(int offset) => _mem.ReadFloat(IntPtr.Add(_root, offset));
    public void SetFloatAt(int offset, float value) => _mem.WriteFloat(IntPtr.Add(_root, offset), value);

    public byte GetByte(string name) => _mem.ReadByte(FieldAddr(name));
    public void SetByte(string name, byte value) => _mem.WriteByte(FieldAddr(name), value);

    public CameraSnapshot Snapshot() => new(
        MinDistance: GetFloat("MinDistance"),
        MinDistance2: GetFloat("MinDistance2"),
        MaxDistance: GetFloat("MaxDistance"),
        MaxDistance2: GetFloat("MaxDistance2"),
        ScrollSpeed: GetFloat("ScrollSpeed"),
        ZoomSpeed: GetFloat("ZoomSpeed"),
        Fov: GetFloat("Fov"),
        PitchMax: GetFloat("PitchMax"),
        PitchMin: GetFloat("PitchMin"),
        CameraAngle: GetFloat("CameraAngle"),
        CameraAngle2: GetFloat("CameraAngle2"),
        PitchMaxC: GetFloat("PitchMaxC"),
        PitchMinC: GetFloat("PitchMinC"),
        TacticalViewMin: GetFloat("TacticalViewMin"),
        TacticalViewMax: GetFloat("TacticalViewMax"),
        HudByte: GetByte("HudByte")
    );

    public void Restore(in CameraSnapshot s)
    {
        SetFloat("MinDistance", s.MinDistance);
        SetFloat("MinDistance2", s.MinDistance2);
        SetFloat("MaxDistance", s.MaxDistance);
        SetFloat("MaxDistance2", s.MaxDistance2);
        SetFloat("ScrollSpeed", s.ScrollSpeed);
        SetFloat("ZoomSpeed", s.ZoomSpeed);
        SetFloat("Fov", s.Fov);
        SetFloat("PitchMax", s.PitchMax);
        SetFloat("PitchMin", s.PitchMin);
        SetFloat("CameraAngle", s.CameraAngle);
        SetFloat("CameraAngle2", s.CameraAngle2);
        SetFloat("PitchMaxC", s.PitchMaxC);
        SetFloat("PitchMinC", s.PitchMinC);
        SetFloat("TacticalViewMin", s.TacticalViewMin);
        SetFloat("TacticalViewMax", s.TacticalViewMax);
        SetByte("HudByte", s.HudByte);
    }

    public void ApplyFpPreset(OffsetsConfig.FpLookSection look)
    {
        const float defaultNearMin = 0.06f;
        const float defaultNearMax = 0.07f;
        const float defaultFov = 70f;

        var nearMin = look.NearMinDistance > 0f ? look.NearMinDistance : defaultNearMin;
        var nearMax = look.NearMaxDistance > 0f ? look.NearMaxDistance : defaultNearMax;
        if (nearMax <= nearMin)
            nearMax = nearMin + 0.01f;

        SetFloat("MinDistance", nearMin);
        SetFloat("MinDistance2", nearMin);
        SetFloat("MaxDistance", nearMax);
        SetFloat("MaxDistance2", nearMax);

        var tactMin = look.TacticalViewMin > 0f ? look.TacticalViewMin : nearMin;
        var tactMax = look.TacticalViewMax > 0f ? look.TacticalViewMax : nearMax;
        if (tactMax <= tactMin)
            tactMax = tactMin + 0.01f;
        SetFloat("TacticalViewMin", tactMin);
        SetFloat("TacticalViewMax", tactMax);

        ApplyFpTurnRates(look);

        var fov = look.Fov > 0f ? look.Fov : defaultFov;
        SetFloat("Fov", fov);

        ApplyFpPitchLimits();
    }

    /// <summary>
    /// PitchMin/Max must bracket the live pitch (~-1.6 in FP). Hard-coded ±1.25 clamped neutral
    /// pitch and made tiny offset changes feel like floor-stare / uncomfortable look-up.
    /// </summary>
    public void ApplyFpPitchLimits()
    {
        float pitch;
        try
        {
            pitch = GetFloat("CameraAngle2");
        }
        catch
        {
            try
            {
                pitch = GetFloat("CameraAngle");
            }
            catch
            {
                pitch = -1.5f;
            }
        }

        const float marginBelow = 0.85f;
        const float marginAbove = 0.45f;
        var pitchMin = pitch - marginBelow;
        var pitchMax = pitch + marginAbove;
        SetFloat("PitchMin", pitchMin);
        SetFloat("PitchMax", pitchMax);
        SetFloat("PitchMinC", pitchMin);
        SetFloat("PitchMaxC", pitchMax);
    }

    /// <summary>Optional BetterCamera mouse/tactical speeds (not right-stick yaw).</summary>
    public void ApplyFpTurnRates(OffsetsConfig.FpLookSection look)
    {
        if (look.ScrollSpeed > 0f)
            SetFloat("ScrollSpeed", look.ScrollSpeed);
        if (look.ZoomSpeed > 0f)
            SetFloat("ZoomSpeed", look.ZoomSpeed);
    }
}

internal readonly record struct CameraSnapshot(
    float MinDistance,
    float MinDistance2,
    float MaxDistance,
    float MaxDistance2,
    float ScrollSpeed,
    float ZoomSpeed,
    float Fov,
    float PitchMax,
    float PitchMin,
    float CameraAngle,
    float CameraAngle2,
    float PitchMaxC,
    float PitchMinC,
    float TacticalViewMin,
    float TacticalViewMax,
    byte HudByte
);

