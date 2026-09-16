# DOS2 First Person Mod — Source

Source for a Divinity: Original Sin 2 (Definitive Edition) companion tool that moves the camera into a first-person view.

This repository is **source only**. The playable Nexus package is a zip with `FirstPersonTool.exe` plus the optional hide-body Lua files.

## What it does

`FirstPersonTool` attaches to the running game (`EoCApp.exe`) and reads/writes camera fields in memory (distance, FOV, pitch, height). It does not inject a DLL, does not use the network, and does not persist except for:

- `Play/camera_offsets.json` (your FP height / look settings)
- `%APPDATA%\Larian Studios\Divinity Original Sin 2 Definitive Edition\Script Extender\FirstPersonTool\state.json` (on/off flag for the optional hide-body mod)

## Build

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). To run the published exe you also need the **.NET 8 Desktop Runtime**.

```powershell
cd Developer\FirstPersonTool\FirstPersonTool
dotnet publish -c Release -r win-x64 --self-contained false -o ..\..\..\Play
```

Then run `Play\FirstPersonTool.exe` while the game is loaded.

## Layout

| Path | What it is |
| --- | --- |
| `Developer/FirstPersonTool/FirstPersonTool/` | C# WinForms tool |
| `Optional - Hide Body Mod/` | Script Extender Lua (hides the body in FP) |
| `Play/camera_offsets.json` | Camera pointer/field offsets and FP settings |
| `README.txt` | Player install / controls |

## Controls

In-game, with an Xbox-compatible controller: **LB + B** toggles first person. Right stick looks up/down.
