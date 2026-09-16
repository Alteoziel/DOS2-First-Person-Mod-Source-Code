# Divinity 2 First Person Mod (Option A)

This project is **two parts**:

- **(Later) Steam Workshop mod**: in-game UI/notes and optional helpers.
- **Companion tool (this repo)**: a small Windows app that toggles camera values in memory to approximate first-person.

## What exists right now (this repo)
- `FirstPersonTool/FirstPersonTool/`: .NET 8 WinForms companion tool
- `WorkshopMod/`: Script Extender Lua (prototype layout)

## MVP status (companion tool)

The companion tool toggles camera values in memory using the BetterCamera pointer:

- base pointer: `[EoCApp.exe + 0x2959898]`
- offsets: distances, FOV, pitch, angles, scroll/zoom speeds, tactical view

### Stabilization features

- **Safe FP preset**: keeps `Min < Max` to avoid gray-screen-on-rotate
- **Full snapshot restore**: captures/restores pitch, angles, scroll/zoom speeds, tactical view
- **Stabilizer loop**: re-applies preset every 75ms while FP is ON
- **Head-lock fallback**: disables pan/zoom drift and re-centers captured camera angles
- **Emergency restore**: `Ctrl+Shift+F1` writes vanilla values repeatedly for ~1 second
- **Vector probe**: optional button to scan camera struct for position triplets (head-lock research)

It does **not yet** implement true mouse-look or hard camera-position override.

## Build (Windows)

Prereq: .NET 8 SDK.

```powershell
cd .\FirstPersonTool
dotnet build -c Release
```

## Run

1. Start **Divinity: Original Sin 2 – Definitive Edition** (Steam).
2. Load a save so you’re in the world.
3. Run `FirstPersonTool/FirstPersonTool` and click **Attach**.
4. (Recommended) Click **Zoom probe** once, then **zoom in/out** in-game for 1–2 seconds, then click **Zoom probe** again.\n+   - The tool will auto-pick a candidate “current zoom distance” field.\n+5. Use **F1** to cycle modes: **Auto** → **Forced FP** → **Forced vanilla** → **Auto**.\n+   - In **Auto**, zooming in past a threshold enables FP; zooming out disables it.\n+6. If anything gets stuck (gray screen, bad camera), press **Ctrl+Shift+F1** for emergency restore.

### Current controls (implemented)
- **Attach**: attaches to `EoCApp.exe` after the game is loaded.
- **Toggle first-person**: **RT + Y** (controller).
- **Look**: **right stick** (writes `CameraAngle` + `CameraAngle2`; mapping may be swapped/tuned next).
- **FP state export**: writes `state.json` for the extender mod at:\n+  `%APPDATA%\\Larian Studios\\Divinity Original Sin 2 Definitive Edition\\Script Extender\\FirstPersonTool\\state.json`

## Notes

- If attach fails, run the tool with the **same admin privileges** as the game.
- Some antivirus products may flag any memory-reading tool. Source is included so users can build it themselves.

