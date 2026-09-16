# DOS2 First Person (Workshop Mod)

This folder is the **Script Extender** side of the project.

The companion EXE writes FP state to a file, and the Lua script polls it so the mod can:
- hide the currently-possessed character while FP is enabled
- restore visibility on FP disable or when possession changes

> Note: the actual Workshop packaging (`meta.lsx`, proper mod UUID, etc.) is intentionally left for the final packaging step once the Lua behavior is verified in-game.

