## Install notes (prototype)

### Requirements
- Norbyte Script Extender installed
- Companion EXE running while you play

### What the EXE writes
The EXE writes FP enabled state to:

`%APPDATA%\Larian Studios\Divinity Original Sin 2 Definitive Edition\Script Extender\FirstPersonTool\state.json`

The extender Lua script polls this to decide when to hide/show the possessed character.

### Lua bootstrap
This repo includes:
- `WorkshopMod/Story/RawFiles/Lua/BootstrapServer.lua`
- `WorkshopMod/Story/RawFiles/Lua/Server/FirstPersonServer.lua`

You’ll still need to package these into a proper DOS2 mod (UUID + `meta.lsx`) before uploading to Workshop.

