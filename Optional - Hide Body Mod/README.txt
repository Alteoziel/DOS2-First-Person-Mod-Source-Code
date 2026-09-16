================================================================================
  OPTIONAL: Hide your character in first person
================================================================================

Why: The companion tool moves the camera close, but your character model
(arms/hands) can still draw in front of the camera. This small Script
Extender mod hides the possessed character while FP is on.

REQUIREMENTS
  - Norbyte Script Extender for DOS2 DE (install separately first)
  - FirstPersonTool.exe running and FP toggled at least once (writes state file)

--------------------------------------------------------------------------------
INSTALL (personal / local mod)
--------------------------------------------------------------------------------

1. Install Script Extender if you haven't:
   https://www.nexusmods.com/divinityoriginalsin2/mods/227

2. In Divinity, create or pick a mod folder, for example:
   Documents\Larian Studios\Divinity Original Sin 2 Definitive Edition\
     Mods\FirstPerson_HideBody_Personal\

3. Copy this entire "Story" folder into that mod folder so you have:
     Mods\FirstPerson_HideBody_Personal\Story\RawFiles\Lua\...

4. Enable the mod in Divinity's main menu mod manager.

5. If you already have a BootstrapServer.lua in another mod, you can instead:
   - Copy only Server\FirstPersonServer.lua into that mod's Lua\Server\
   - Add this line to your existing BootstrapServer.lua:
       Ext.Require("Server/FirstPersonServer.lua")

6. Launch game with Script Extender (Extender launches the game).

7. Run FirstPersonTool, Attach, toggle FP with LB+B — body should hide in FP.

The EXE writes:
  %APPDATA%\Larian Studios\Divinity Original Sin 2 Definitive Edition\
    Script Extender\FirstPersonTool\state.json

The Lua script reads that file each tick.

--------------------------------------------------------------------------------
UNINSTALL
--------------------------------------------------------------------------------

  Disable or delete the mod in the mod manager. Toggle FP off in-game.

================================================================================
