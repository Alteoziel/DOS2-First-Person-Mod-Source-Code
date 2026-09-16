namespace FirstPersonTool;

internal enum LookWrapMode
{
    SignedPi = 0,
    ZeroToTwoPi = 1,
    Unbounded = 2
}

internal sealed class LookControl
{
    public float Sensitivity { get; set; } = 13.0f;
    public float Deadzone { get; set; } = 0.08f;
    public bool InvertY { get; set; } = false;
    public bool FreezePitch { get; set; } = false;
    public LookWrapMode WrapMode { get; set; } = LookWrapMode.SignedPi;
    public float YawAvoidZeroThreshold { get; set; } = 0.50f;
    /// <summary>Tiny offset (radians) once at FP enable when yaw is near 0; 0 disables.</summary>
    public float YawSeamNudgeRadians { get; set; } = 0.35f;

    /// <summary>Radians added to game neutral for default (neutral stick) view.</summary>
    public float PitchNeutralOffset { get; set; } = -0.06f;
    /// <summary>Extra radians below biased neutral when stick is fully down.</summary>
    public float PitchDownOffset { get; set; } = -0.02f;
    /// <summary>Radians above biased neutral when stick is fully up.</summary>
    public float PitchUpOffset { get; set; } = 0.02f;
    public float PitchModeThreshold { get; set; } = 0.30f;
    public float PitchModeHysteresis { get; set; } = 0.08f;
    /// <summary>Seconds to ease pitch toward stick target (avoids snapping the game camera).</summary>
    public float PitchSmoothTau { get; set; } = 0.10f;

    public string YawField { get; set; } = "CameraAngle";
    public string PitchField { get; set; } = "CameraAngle2";

    private float _sx;
    private float _sy;
    private bool _initialized;
    private bool _fpSessionSeamApplied;

    // Wrapped modes: base + delta. Unbounded: one continuous angle (never π/-π flip).
    private float _yawBase;
    private float _yawDelta;
    private float _yawContinuous;
    private float _yawAnchor;
    private float _yawOrbit;
    private bool _yawHasAnchor;

    private float _neutralPitch;
    private float _pitch;
    private PitchMode _pitchMode;

    private enum PitchMode
    {
        Neutral = 0,
        Up = 1,
        Down = 2
    }

    public void BeginFpSession() => _fpSessionSeamApplied = false;

    public void EndFpSession()
    {
        _initialized = false;
        _fpSessionSeamApplied = false;
        _yawHasAnchor = false;
        _yawOrbit = 0;
    }

    public void SetSwapYawPitch(bool swap)
    {
        YawField = swap ? "CameraAngle2" : "CameraAngle";
        PitchField = swap ? "CameraAngle" : "CameraAngle2";
        _initialized = false;
    }

    public void HardResyncFromGame(CameraAccess cam)
    {
        var gameYaw = cam.GetFloat(YawField);
        if (UsesContinuousYaw())
        {
            _yawContinuous = UnwrapNearest(gameYaw, 0f);
            ApplySeamNudgeIfNeeded();
        }
        else
        {
            _yawBase = gameYaw;
            _yawDelta = 0;
        }

        _neutralPitch = cam.GetFloat(PitchField);
        _pitch = _neutralPitch + PitchNeutralOffset;
        _initialized = true;
        _sx = 0;
        _sy = 0;
        _pitchMode = PitchMode.Neutral;
    }

    public void RelinkFromGame(CameraAccess cam)
    {
        if (UsesContinuousYaw())
        {
            _initialized = true;
            return;
        }

        var totalYaw = _yawBase + _yawDelta;
        _yawBase = cam.GetFloat(YawField);
        _yawDelta = totalYaw - _yawBase;
        _initialized = true;
    }

    public void ResetFromCamera(CameraAccess cam) => HardResyncFromGame(cam);

    public void SetYawImmediate(CameraAccess cam, float yaw)
    {
        if (UsesContinuousYaw())
        {
            _yawHasAnchor = true;
            _yawAnchor = yaw;
            _yawOrbit = 0;
            _yawContinuous = yaw;
            _fpSessionSeamApplied = true;
        }
        else
        {
            _yawBase = yaw;
            _yawDelta = 0;
        }

        cam.SetFloat(YawField, yaw);
    }

    public void Apply(CameraAccess cam, ControllerState cur, float dtSeconds)
    {
        var nx = NormalizeStick(cur.ThumbRX);
        var ny = NormalizeStick(cur.ThumbRY);
        if (InvertY) ny = -ny;

        nx = ApplyDeadzone(nx, Deadzone);
        ny = ApplyDeadzone(ny, Deadzone);

        var alpha = 1f - MathF.Exp(-dtSeconds / 0.07f);
        _sx = Lerp(_sx, nx, alpha);
        _sy = Lerp(_sy, ny, alpha);

        if (!_initialized)
            HardResyncFromGame(cam);

        // Yaw: fixed at FP enable (SetYawImmediate). Do not drive CameraAngle from the stick.

        if (!FreezePitch)
        {
            var upOn = PitchModeThreshold;
            var upOff = PitchModeThreshold - PitchModeHysteresis;
            var downOn = -PitchModeThreshold;
            var downOff = -PitchModeThreshold + PitchModeHysteresis;

            if (_pitchMode == PitchMode.Up)
            {
                if (_sy < upOff) _pitchMode = PitchMode.Neutral;
            }
            else if (_pitchMode == PitchMode.Down)
            {
                if (_sy > downOff) _pitchMode = PitchMode.Neutral;
            }
            else
            {
                if (_sy >= upOn) _pitchMode = PitchMode.Up;
                else if (_sy <= downOn) _pitchMode = PitchMode.Down;
            }

            var biasedNeutral = _neutralPitch + PitchNeutralOffset;
            var pitchTarget = _pitchMode switch
            {
                PitchMode.Up => biasedNeutral + PitchUpOffset,
                PitchMode.Down => biasedNeutral + PitchDownOffset,
                _ => biasedNeutral
            };

            var pitchAlpha = 1f - MathF.Exp(-dtSeconds / MathF.Max(0.02f, PitchSmoothTau));
            _pitch = Lerp(_pitch, pitchTarget, pitchAlpha);
        }

        CommitAngles(cam);
        try
        {
            cam.ApplyFpPitchLimits();
            cam.SetFloat(PitchField, _pitch);
        }
        catch
        {
            // ignore
        }
    }

    private bool UsesContinuousYaw() => WrapMode == LookWrapMode.Unbounded;

    private float YawTotal()
    {
        if (UsesContinuousYaw())
            return _yawHasAnchor ? _yawAnchor + _yawOrbit : _yawContinuous;
        return _yawBase + _yawDelta;
    }

    private void CommitAngles(CameraAccess cam)
    {
        cam.SetFloat(PitchField, _pitch);
        if (_yawHasAnchor)
            cam.SetFloat(YawField, _yawAnchor);
    }

    private void ApplySeamNudgeIfNeeded()
    {
        if (_fpSessionSeamApplied || !UsesContinuousYaw() || YawSeamNudgeRadians <= 0f) return;

        var avoid = MathF.Max(1.5f, YawAvoidZeroThreshold);
        if (MathF.Abs(_yawContinuous) >= avoid)
        {
            _fpSessionSeamApplied = true;
            return;
        }

        _yawContinuous += MathF.Sign(_yawContinuous == 0f ? 1f : _yawContinuous) * YawSeamNudgeRadians;
        _fpSessionSeamApplied = true;
    }

    /// <summary>Same angle as <paramref name="angle"/> but closest to <paramref name="reference"/> (no π flip).</summary>
    private static float UnwrapNearest(float angle, float reference)
    {
        const float twoPi = MathF.PI * 2f;
        var best = angle;
        var bestDist = MathF.Abs(angle - reference);
        foreach (var k in new[] { -1f, 1f })
        {
            var candidate = angle + k * twoPi;
            var dist = MathF.Abs(candidate - reference);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = candidate;
            }
        }

        return best;
    }

    private static float NormalizeStick(short v) => v < 0 ? (v / 32768f) : (v / 32767f);

    private static float ApplyDeadzone(float v, float dz)
    {
        var a = MathF.Abs(v);
        if (a <= dz) return 0f;
        var scaled = (a - dz) / (1f - dz);
        return MathF.CopySign(scaled, v);
    }

    private static float ResponseCurve(float v) => MathF.CopySign(v * v, v);

    private static float WrapSignedPi(float r)
    {
        const float twoPi = MathF.PI * 2f;
        r = (r + MathF.PI) % twoPi;
        if (r < 0) r += twoPi;
        return r - MathF.PI;
    }

    private static float WrapZeroToTwoPi(float r)
    {
        const float twoPi = MathF.PI * 2f;
        r %= twoPi;
        if (r < 0) r += twoPi;
        return r;
    }

    private static float AvoidWrapEdge(LookWrapMode mode, float r)
    {
        const float eps = 0.02f;
        if (float.IsNaN(r) || float.IsInfinity(r)) return 0f;

        if (mode == LookWrapMode.ZeroToTwoPi)
        {
            const float twoPi = MathF.PI * 2f;
            if (r <= 0f) return eps;
            if (r < eps) return eps;
            if (r >= twoPi) return twoPi - eps;
            if (r > twoPi - eps) return twoPi - eps;
            return r;
        }

        if (r < -MathF.PI + eps) return -MathF.PI + eps;
        if (r > MathF.PI - eps) return MathF.PI - eps;
        return r;
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}

