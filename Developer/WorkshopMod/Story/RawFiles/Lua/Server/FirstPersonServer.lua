-- Script Extender Lua (Server)
-- MVP: read FP state from disk and apply hide/show to the currently possessed character.

local STATE_RELATIVE = "Script Extender\\FirstPersonTool\\state.json"

local lastEnabled = false
local lastChar = nil

local function getStatePath()
  -- %APPDATA%\Larian Studios\Divinity Original Sin 2 Definitive Edition\Script Extender\
  local base = Ext.IO.GetPath("AppData")
  return base .. "\\" .. STATE_RELATIVE
end

local function readState()
  local path = getStatePath()
  local json = Ext.IO.LoadFile(path, "data")
  if not json then return nil end
  local ok, obj = pcall(Ext.Json.Parse, json)
  if not ok then return nil end
  return obj
end

local function getPossessedCharacter()
  -- Best-effort: use the host player character.
  -- This may need refinement (true possessed character) once we test in-game.
  local host = CharacterGetHostCharacter()
  if host and host ~= "" then return host end
  return nil
end

local function setHidden(char, hidden)
  if not char then return end
  -- Placeholder: The exact visual-hide API varies by extender version / available Osiris calls.
  -- We'll finalize after first in-game test. For now, we use a conservative Osiris fallback.
  if hidden then
    CharacterSetInvisible(char, 1)
  else
    CharacterSetInvisible(char, 0)
  end
end

local function tick()
  local st = readState()
  if not st then return end

  local enabled = st.enabled == true
  local possessed = getPossessedCharacter()

  if enabled then
    if possessed and possessed ~= lastChar then
      if lastChar then setHidden(lastChar, false) end
      setHidden(possessed, true)
      lastChar = possessed
    elseif possessed and lastChar == nil then
      setHidden(possessed, true)
      lastChar = possessed
    end
  else
    if lastChar then
      setHidden(lastChar, false)
      lastChar = nil
    end
  end

  lastEnabled = enabled
end

Ext.Events.Tick:Subscribe(tick)

