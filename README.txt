================================================================================
  DIVINITY: ORIGINAL SIN 2 - FIRST PERSON TOOL
  Quick start (read this first)
================================================================================

REQUIREMENTS
  - Windows PC
  - Divinity: Original Sin 2 - Definitive Edition (Steam)
  - Xbox / compatible controller (required for toggle and look)
  - .NET 8 Desktop Runtime (Windows x64)
      https://dotnet.microsoft.com/download/dotnet/8.0
      (Download "Desktop Runtime" if the tool won't start.)

OPTIONAL (recommended - hides your character's body/hands in FP)
  - Norbyte Script Extender for DOS2
  - Files in "Optional - Hide Body Mod" folder (see its README.txt)

SOURCE CODE (for reviewers / rebuild)
  https://github.com/Alteoziel/DOS2-First-Person-Mod-Source-Code

--------------------------------------------------------------------------------
HOW TO PLAY (5 steps)
--------------------------------------------------------------------------------

1. Start Divinity 2 and load into the game world (not main menu).

2. Open the "Play" folder and run FirstPersonTool.exe
   (If Windows SmartScreen warns you, choose "More info" -> "Run anyway"
    or unzip to a folder you trust.)

3. Click "Attach (EoCApp.exe)" in the tool window.
   Status should say attached. If it fails, run the tool as Administrator
   (right-click exe -> Run as administrator).

4. (First time only) Click "Set FP height (sweep)" once while NOT in FP mode.
   Watch the screen flicker briefly, then click Yes to save when asked.
   This sets eye height for first person.

5. In-game: hold LB + press B to turn first person ON or OFF.
   (Left bumper + B on an Xbox controller.)

   Right stick: look up / down (two resting views after you release the stick).

--------------------------------------------------------------------------------
TUNING (optional)
--------------------------------------------------------------------------------

Edit Play\camera_offsets.json in Notepad. Save the file, then toggle FP off/on
(LB+B) or click Attach again.

See CONFIG.txt in this folder for what each setting does.

--------------------------------------------------------------------------------
FOLDER LAYOUT
--------------------------------------------------------------------------------

  Play\                         <- Run FirstPersonTool.exe from here
  Optional - Hide Body Mod\     <- Script Extender files (optional)
  CONFIG.txt                    <- Settings reference
  Developer\                    <- Source code & dev files (ignore if just playing)

--------------------------------------------------------------------------------
TROUBLESHOOTING
--------------------------------------------------------------------------------

  Attach failed        -> Run game first, then tool. Try Administrator.
  Gray screen / spin   -> LB+B off to exit FP. Lower nearMinDistance carefully.
  Hands in view        -> Install optional hide-body mod + Script Extender.
  Camera too low/high  -> Run "Set FP height (sweep)" again on flat ground.

================================================================================
