Developer folder — not needed to play

  FirstPersonTool/          Source code + build outputs (dotnet publish)
  Backup - Current Setup/   Your saved config + source snapshot
  WorkshopMod/              Original mod sources (copy for release is in Optional folder)
  Safe State - First Person Mod/   Old snapshot
  EoCApp_v11.CT             Cheat Engine table (offset reference)
  PLAN.md, CLAUDE.md, etc.  Design / research notes

To rebuild the Play folder:
  cd FirstPersonTool\FirstPersonTool
  dotnet publish -c Release -r win-x64 -o ..\..\Play
  Copy your camera_offsets.json into Play\
