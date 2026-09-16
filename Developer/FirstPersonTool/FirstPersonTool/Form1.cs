namespace FirstPersonTool;

public partial class Form1 : Form
{
    private sealed class HeightSweepState
    {
        public long NextTick;
        public int Index;
        public required IReadOnlyList<int> Offsets;
        public float Lift;
        public int? PrevOffset;
        public float? PrevBaseline;
        public List<(int Offset, float Baseline, float Effect)> Hits { get; } = new();
        public (int Offset, float Baseline, float Effect)? BestHit;
    }

    private System.Windows.Forms.Timer? _timer;
    private Dos2Session? _session;
    private ControllerState _prevController;
    private bool _fpEnabled;
    private OffsetsConfig? _offsets;
    private string? _offsetsPath;
    private IntPtr _moduleBase;
    private IntPtr _cameraRoot;
    private CameraAccess? _camera;
    private CameraSnapshot? _snapshot;
    private long _lastPresetTick;
    private long _lastLookTick;
    private readonly LookControl _look = new();
    private readonly FpStateWriter _stateWriter = new();
    private FpCameraHeight? _fpHeight;
    private HashSet<int>? _heightDenylist;
    private HeightSweepState? _heightSweep;
    private FileSystemWatcher? _configWatcher;
    private DateTime _offsetsWriteTime;

    public Form1()
    {
        InitializeComponent();
        SetStatus("Not attached.");

        cmbWrap.Items.Clear();
        cmbWrap.Items.Add("(-pi, pi]");
        cmbWrap.Items.Add("[0, 2pi)");
        cmbWrap.Items.Add("No wrap (unbounded)");
        cmbWrap.SelectedIndex = 2;
        _look.WrapMode = LookWrapMode.Unbounded;
        lblDiag.Text = "Diag: (not available)";
    }

    private void btnAttach_Click(object sender, EventArgs e)
    {
        try
        {
            _session?.Dispose();
            _offsetsPath = Path.Combine(AppContext.BaseDirectory, "camera_offsets.json");
            LoadOffsetsConfig();
            StartConfigWatcher();
            _session = Dos2Session.AttachOrThrow(_offsets!.Game.ProcessName);
            _fpEnabled = false;
            chkFpEnabled.Checked = false;
            _look.EndFpSession();
            _heightSweep = null;

            _moduleBase = _session.GetModuleBaseAddressOrThrow(_offsets.Game.ModuleName);
            _cameraRoot = ResolveCameraRoot();
            if (_cameraRoot == IntPtr.Zero)
                SetStatus($"Attached (PID={_session.ProcessId}) but camera root unresolved.");
            else
            {
                _camera = new CameraAccess(_session.Memory, _offsets, _cameraRoot);
                SetStatus(AttachStatusMessage());
            }

            _timer?.Stop();
            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += (_, _) => TickLoop();
            _timer.Start();
        }
        catch (Exception ex)
        {
            _session?.Dispose();
            _session = null;
            _timer?.Stop();
            SetStatus($"Attach failed: {ex.Message}");
        }
    }

    private void LoadOffsetsConfig()
    {
        _offsets = OffsetsConfig.LoadOrThrow(_offsetsPath!);
        _heightDenylist = FpCameraHeight.BuildDenylist(_offsets);
        _fpHeight = new FpCameraHeight(_offsets.FpHeight, _heightDenylist);
        ApplyLookFromConfig();
        NoteOffsetsWriteTime();
    }

    private void NoteOffsetsWriteTime()
    {
        if (_offsetsPath is null || !File.Exists(_offsetsPath)) return;
        _offsetsWriteTime = File.GetLastWriteTimeUtc(_offsetsPath);
    }

    private void MaybeReloadOffsetsIfChanged()
    {
        if (_offsetsPath is null || !File.Exists(_offsetsPath)) return;
        var writeTime = File.GetLastWriteTimeUtc(_offsetsPath);
        if (writeTime <= _offsetsWriteTime) return;
        ReloadHeightConfigFromDisk();
    }

    private void ApplyLookFromConfig()
    {
        if (_offsets is null) return;
        var look = _offsets.FpLook;
        _look.Sensitivity = look.Sensitivity;
        _look.PitchNeutralOffset = look.PitchNeutralOffset;
        _look.PitchDownOffset = look.PitchDownOffset;
        _look.PitchUpOffset = look.PitchUpOffset;
    }

    private string SavedPivotHex()
    {
        var hex = _offsets?.FpHeight.PrimaryOffsetHex ?? "0x0";
        return hex.Equals("0x0", StringComparison.OrdinalIgnoreCase) ? "(none)" : hex;
    }

    private string FormatLift() => _fpHeight is null ? "?" : $"{_fpHeight.EffectiveLift:0.##}m";

    private string AttachStatusMessage() =>
        $"Attached. Config: {_offsetsPath}  pivot={SavedPivotHex()}  lift={FormatLift()}  sens={_look.Sensitivity:0.####}";

    private void TickLoop()
    {
        if (_session is null)
        {
            lblController.Text = "Controller: (none)";
            return;
        }

        lblController.Text = XInput.TryGetState(0, out _) ? "Controller: connected" : "Controller: (none)";
        TickHeightSweep();

        if (!XInput.TryGetState(0, out var cur))
            return;

        var lbDown = (cur.Buttons & XInputButtons.LeftShoulder) != 0;
        var bDown = (cur.Buttons & XInputButtons.B) != 0;
        var bWasDown = (_prevController.Buttons & XInputButtons.B) != 0;

        if (lbDown && bDown && !bWasDown)
            ToggleFirstPerson();

        if (_fpEnabled && _camera is not null && _offsets is not null)
        {
            MaybeReloadOffsetsIfChanged();

            if (!TryRefreshCameraRoot())
            {
                lblDiag.Text = "Diag: camera root lost";
                _prevController = cur;
                return;
            }

            var now = Environment.TickCount64;
            _fpHeight?.Apply(_camera);

            if (now - _lastPresetTick >= 75)
            {
                _camera.ApplyFpPreset(_offsets.FpLook);
                _lastPresetTick = now;
            }

            _camera.ApplyFpTurnRates(_offsets.FpLook);
            _fpHeight?.Apply(_camera);

            var dt = (now - _lastLookTick) / 1000f;
            if (_lastLookTick == 0) dt = 0.016f;
            _lastLookTick = now;
            try
            {
                _look.Apply(_camera, cur, Math.Clamp(dt, 0.0f, 0.05f));
            }
            catch
            {
                // skip
            }

            _fpHeight?.Apply(_camera);
            _camera.ApplyFpTurnRates(_offsets.FpLook);

            try
            {
                var yaw = _camera.GetFloat(_look.YawField);
                var pitch = _camera.GetFloat(_look.PitchField);
                lblAngles.Text = $"Angles: yaw={yaw:0.000} pitch={pitch:0.000}";
                var heightDiag = _fpHeight?.GetDiag(_camera) ?? "n/a";
                lblDiag.Text = $"Diag: FP lift={FormatLift()}  {heightDiag}";
            }
            catch
            {
                lblAngles.Text = "Angles: (read failed)";
            }
        }
        else
        {
            lblAngles.Text = "Angles: (not available)";
            if (_heightSweep is null)
                lblDiag.Text = "Diag: (not available)";
            _lastLookTick = 0;
        }

        _prevController = cur;
    }

    private void ToggleFirstPerson()
    {
        _fpEnabled = !_fpEnabled;
        chkFpEnabled.Checked = _fpEnabled;

        if (_fpEnabled)
        {
            if (_camera is not null)
            {
                LoadOffsetsConfig();
                _snapshot = _camera.Snapshot();
                _camera.ApplyFpPreset(_offsets.FpLook);
                _look.BeginFpSession();
                _look.HardResyncFromGame(_camera);
                _look.SetYawImmediate(_camera, 1000f);
                _fpHeight?.OnFpEnable(_camera);
                for (var i = 0; i < 5; i++)
                    _fpHeight?.Apply(_camera);
                _lastPresetTick = Environment.TickCount64;
                _lastLookTick = 0;

                if (_fpHeight is not null && _fpHeight.HasConfiguredOffset && _fpHeight.TrackedCount > 0)
                    SetStatus($"FP on — height {FormatLift()} at {SavedPivotHex()}");
                else
                    SetStatus("FP on — no saved height. Use Set FP height (sweep) first, click Yes to save.");
            }
            _stateWriter.WriteEnabled(true);
        }
        else
        {
            if (_camera is not null)
            {
                _fpHeight?.OnFpDisable(_camera);
                if (_snapshot is not null)
                    _camera.Restore(_snapshot.Value);
                _look.EndFpSession();
            }
            _snapshot = null;
            _stateWriter.WriteEnabled(false);
            SetStatus("FP off — normal camera height restored.");
        }
    }

    private void TickHeightSweep()
    {
        if (_camera is null || _heightSweep is not { } sw) return;

        var now = Environment.TickCount64;
        if (now < sw.NextTick) return;

        if (sw.PrevOffset is int prev && sw.PrevBaseline is float prevBase)
        {
            try { _camera.SetFloatAt(prev, prevBase); }
            catch { /* ignore */ }
        }

        if (sw.Index >= sw.Offsets.Count)
        {
            _heightSweep = null;
            FinishSweepAndPrompt(sw);
            return;
        }

        var off = sw.Offsets[sw.Index];
        float baseline = 0;
        try
        {
            baseline = _camera.GetFloatAt(off);
            var target = baseline + sw.Lift;
            _camera.SetFloatAt(off, target);
            var readback = _camera.GetFloatAt(off);
            var effect = MathF.Abs(readback - baseline);
            if (MathF.Abs(readback - target) <= MathF.Max(0.05f, sw.Lift * 0.25f) && effect > 0.001f)
            {
                var hit = (off, baseline, effect);
                sw.Hits.Add(hit);
                if (sw.BestHit is null || effect >= sw.BestHit.Value.Effect)
                    sw.BestHit = hit;
            }

            SetStatus($"Finding height… {sw.Index + 1}/{sw.Offsets.Count}");
        }
        catch
        {
            SetStatus($"Finding height… {sw.Index + 1}/{sw.Offsets.Count} (skip)");
        }

        sw.PrevOffset = off;
        sw.PrevBaseline = baseline;
        sw.Index++;
        sw.NextTick = now + 350;
    }

    private void FinishSweepAndPrompt(HeightSweepState sw)
    {
        try
        {
            if (_camera is null || _offsets is null) return;

            if (sw.Hits.Count == 0)
            {
                MessageBox.Show(
                    "Could not find a height field automatically.\n\nTry again on flat ground, or move the camera slightly.",
                    "Set FP height",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                SetStatus("Sweep found no height field.");
                return;
            }

            var best = sw.BestHit ?? PickBestSweepHit(sw.Hits, sw.Lift);

        var hex = $"0x{best.Offset:X}";
        var lift = _fpHeight?.EffectiveLift ?? sw.Lift;

        try
        {
            _camera.SetFloatAt(best.Offset, best.Baseline + lift);
        }
        catch
        {
            // ignore
        }

        _offsets.FpHeight.PrimaryOffsetHex = hex;

        var msg =
            $"Height was set automatically.\n\n" +
            $"Pivot: {hex}\n" +
            $"Lift: {lift:0.##} meters in first person\n\n" +
            $"Save these settings for next time?\n" +
            $"(LB+B toggles first person; off restores normal height.)";

        var save = MessageBox.Show(
            msg,
            "Save FP height?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1);

        if (save == DialogResult.Yes)
        {
            try
            {
                OffsetsConfig.SaveFpHeight(
                    _offsetsPath!,
                    hex,
                    best.Baseline,
                    _offsets.FpHeight.EyeLiftMeters,
                    _offsets.FpHeight.LiftScale);
                LoadOffsetsConfig();
                try { _camera.SetFloatAt(best.Offset, best.Baseline); }
                catch { /* ignore */ }
                lblHeight.Text = $"Saved {hex} ({FormatLift()}). LB+B = first person on/off.";
                SetStatus($"Saved FP height {hex} baseline={best.Baseline:0.###} lift={FormatLift()}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save: {ex.Message}", "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        else
        {
            try
            {
                _camera.SetFloatAt(best.Offset, best.Baseline);
            }
            catch
            {
                // ignore
            }

            LoadOffsetsConfig();
            lblHeight.Text = "Not saved. Press Set FP height again to retry.";
            SetStatus("Height not saved.");
        }
        }
        finally
        {
            FinishSweepEnableButton();
        }
    }

    private void StopSweepRestoreVanilla()
    {
        if (_camera is null || _heightSweep is not { } sw) return;
        if (sw.PrevOffset is int prev && sw.PrevBaseline is float prevBase)
        {
            try { _camera.SetFloatAt(prev, prevBase); }
            catch { /* ignore */ }
        }
        _heightSweep = null;
        FinishSweepEnableButton();
    }

    private void StartConfigWatcher()
    {
        _configWatcher?.Dispose();
        if (_offsetsPath is null || !File.Exists(_offsetsPath)) return;

        var dir = Path.GetDirectoryName(_offsetsPath)!;
        _configWatcher = new FileSystemWatcher(dir, Path.GetFileName(_offsetsPath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _configWatcher.Changed += (_, _) =>
        {
            try
            {
                BeginInvoke(ReloadHeightConfigFromDisk);
            }
            catch
            {
                // ignore if form is closing
            }
        };
        _configWatcher.EnableRaisingEvents = true;
    }

    private void ReloadHeightConfigFromDisk()
    {
        if (_offsetsPath is null || !File.Exists(_offsetsPath)) return;

        try
        {
            LoadOffsetsConfig();
            if (_camera is not null && _fpEnabled && _fpHeight is not null)
            {
                _fpHeight.OnFpDisable(_camera);
                _fpHeight.OnFpEnable(_camera);
                for (var i = 0; i < 3; i++)
                    _fpHeight.Apply(_camera);
            }

            lblHeight.Text = $"Config reloaded: lift={FormatLift()} sens={_look.Sensitivity:0.##} pivot={SavedPivotHex()}";
            SetStatus($"Reloaded {Path.GetFileName(_offsetsPath)} — lift={FormatLift()} sens={_look.Sensitivity:0.##}");
        }
        catch (Exception ex)
        {
            SetStatus($"Config reload failed: {ex.Message}");
        }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        _configWatcher?.Dispose();
        StopSweepRestoreVanilla();
        if (_camera is not null && _fpEnabled)
            _fpHeight?.OnFpDisable(_camera);
        _look.EndFpSession();
        _timer?.Stop();
        _timer?.Dispose();
        _session?.Dispose();
    }

    private void SetStatus(string msg) => lblStatus.Text = $"Status: {msg}";

    private IntPtr ResolveCameraRoot()
    {
        if (_session is null || _offsets is null || _moduleBase == IntPtr.Zero) return IntPtr.Zero;
        return PointerChain.ResolveFromModule(
            _session.Memory,
            _moduleBase,
            _offsets.CameraRootModuleOffset(),
            _offsets.CameraRootOffsets());
    }

    private bool TryRefreshCameraRoot()
    {
        if (_session is null || _offsets is null || _camera is null) return false;

        var root = ResolveCameraRoot();
        if (root == IntPtr.Zero) return false;
        if (root == _cameraRoot) return true;

        _cameraRoot = root;
        _camera.SetRoot(root);
        _look.RelinkFromGame(_camera);
        _fpHeight?.Rebaseline(_camera);
        if (_offsets is not null)
            _camera.ApplyFpPreset(_offsets.FpLook);
        _fpHeight?.Apply(_camera);
        _lastPresetTick = Environment.TickCount64;
        _lastLookTick = 0;
        SetStatus($"Camera re-linked (root=0x{root.ToInt64():X}).");
        return true;
    }

    private void cmbWrap_SelectedIndexChanged(object sender, EventArgs e)
    {
        _look.WrapMode = cmbWrap.SelectedIndex switch
        {
            1 => LookWrapMode.ZeroToTwoPi,
            2 => LookWrapMode.Unbounded,
            _ => LookWrapMode.SignedPi
        };
    }

    private void chkSwapYawPitch_CheckedChanged(object sender, EventArgs e)
    {
        _look.SetSwapYawPitch(chkSwapYawPitch.Checked);
        if (_camera is not null) _look.HardResyncFromGame(_camera);
    }

    private void chkFreezePitch_CheckedChanged(object sender, EventArgs e)
    {
        _look.FreezePitch = chkFreezePitch.Checked;
    }

    private void btnProbe_Click(object sender, EventArgs e)
    {
        if (_camera is null) { SetStatus("Probe: not attached."); return; }
        try
        {
            var min = _camera.GetFloat("MinDistance");
            var max = _camera.GetFloat("MaxDistance");
            SetStatus($"Probe: Min={min:0.000} Max={max:0.000} FP={_fpEnabled}");
        }
        catch (Exception ex)
        {
            SetStatus($"Probe failed: {ex.Message}");
        }
    }

    private void btnSweepPivot_Click(object sender, EventArgs e)
    {
        if (_camera is null) { SetStatus("Attach to the game first."); return; }
        if (_fpEnabled)
        {
            SetStatus("Turn off first person (LB+B), then run Set FP height.");
            return;
        }

        if (_heightSweep is not null)
        {
            SetStatus("Already finding height…");
            return;
        }

        var deny = _heightDenylist ?? FpCameraHeight.BuildDenylist(_offsets!);
        var band = PivotSweep.BandOffsets().Where(o => !deny.Contains(o)).ToList();
        var lift = _offsets?.FpHeight.EyeLiftMeters ?? 1.2f;

        StopSweepRestoreVanilla();
        _heightSweep = new HeightSweepState
        {
            NextTick = Environment.TickCount64,
            Index = 0,
            Offsets = band,
            Lift = lift
        };

        btnSweepPivot.Enabled = false;
        SetStatus("Finding FP height… watch the view briefly jump.");
        lblHeight.Text = "Scanning… the view may flicker. You will be asked to save when done.";
    }

    private void FinishSweepEnableButton()
    {
        btnSweepPivot.Enabled = true;
    }

    private static (int Offset, float Baseline, float Effect) PickBestSweepHit(
        List<(int Offset, float Baseline, float Effect)> hits,
        float probeLift)
    {
        var strong = hits.Where(h => h.Effect >= probeLift * 0.2f).ToList();
        if (strong.Count > 0)
        {
            return strong
                .OrderBy(h => MathF.Abs(h.Effect - probeLift))
                .ThenByDescending(h => h.Effect)
                .First();
        }

        return hits.OrderByDescending(h => h.Effect).First();
    }
}
