# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

This repo is two things in one: the **Board SDK** (v3.3.0) and a **Combat demo** game built with it. The Board SDK is a Unity package (`fun.board`) for the [Board tabletop gaming platform](https://docs.dev.board.fun/), which tracks physical game pieces on a touch screen. The Combat demo is a 2-player space shooter where physical Board pieces control ships.

- **Unity version:** 6000.0.73f1
- **Target platform:** Android arm64, IL2CPP, API 33

**Unity 6 only:** Set Application Entry Point to "Activity" (not Game Activity) in **Player Settings > Android > Other Settings**.

## Required using statements

```csharp
using Board.Core;      // BoardApplication, BoardPlayer, BoardPauseScreenContext
using Board.Input;     // BoardInput, BoardContact, BoardContactType, BoardContactPhase
using Board.Session;   // BoardSession, BoardSessionPlayer
using Board.Save;      // BoardSaveGameManager, BoardSaveGameMetadata
```

Do NOT use `using Board;` — each namespace must be imported individually.

## Project setup

Run **Board > Configure Unity Project...** to automatically configure all required settings (platform switching, API levels, scripting backend, Input System). Settings assets (`BoardGeneralSettings`, `BoardInputSettings`) are auto-created on SDK import.

Download Piece Set Models via **Edit > Project Settings > Board > Input Settings** → "Load Available Models".

## Build & deploy

Builds are triggered from Unity Editor via **File > Build Settings**. Deploy to Board hardware using `bdb` (Board Developer Bridge) — requires Board OS 1.3.8+ and a USB-C data cable:

```sh
bdb status                          # check connection
bdb install path/to/game.apk
bdb launch com.a51.board_demo
bdb logs com.a51.board_demo         # stream logcat
bdb stop com.a51.board_demo
bdb list                            # installed apps
bdb remove com.a51.board_demo
```

## Testing in editor (without hardware)

The SDK includes an in-editor piece simulator. With the game in Play mode, open **Window > Board > Simulator** to spawn simulated contacts with the mouse. See the [Simulator Guide](https://docs.dev.board.fun/guides/simulator).

`BoardInput.GetActiveContacts()` returns an empty array on all non-Android, non-Editor platforms — all SDK calls are silently no-ops on iOS/desktop builds.

## Architecture

### Board SDK (`Runtime/` and `Editor/`)

The SDK is a Unity package with assembly definition `Runtime/Board.asmdef`.

**Key namespaces:**
- `Board.Input` — input reading and simulation
- `Board.Core` — application lifecycle, player profiles
- `Board.Save` — save game management
- `Board.Session` — multiplayer sessions

**The main entry point is `BoardInput` (`Runtime/Input/BoardInput.cs`)**, a static class that maintains contact state. On Android it calls a native JNI plugin via P/Invoke; in the Editor it uses `BoardContactSimulation` to inject synthetic events into the same queue.

`BoardContact` is a fixed-layout struct (`Runtime/Input/BoardContact.cs`):
- `contactId` — unique per contact; use this to track pieces across frames
- `glyphId` — which piece type (0 to N-1 in the piece set); -1 for fingers; non-unique
- `screenPosition` — pixel coords on the Board screen
- `orientation` — radians counter-clockwise from vertical
- `phase` — `BoardContactPhase`: Began / Moved / Stationary / Ended / Canceled / None
- `isTouched` — whether a finger is touching this piece
- `isNoneEndedOrCanceled` — convenience bool for "contact is gone"

`BoardContactType` distinguishes `Glyph` (physical pieces) from `Finger` (bare touches).

### Combat demo (`Assets/Combat/`)

Single scene: `Assets/Combat/Scenes/Combat.unity`

**Script call chain:**

```
ShipManager (Update)
  └─ BoardInput.GetActiveContacts(Glyph)   ← reads physical pieces
  └─ ShipController.ApplyContact(contact)  ← one per ship
        ├─ Camera.main.ScreenToWorldPoint  ← screenPosition → world pos
        ├─ Quaternion.Euler(orientation)   ← orientation → ship rotation
        ├─ color from glyphId (4–7 → pink/yellow/purple/orange)
        ├─ PlayerDashboard.FirePressed      ← UI fire button state
        └─ Instantiate(bulletPrefab) → Bullet.Launch()

Bullet (Update)
  └─ manual velocity + gravity (no Rigidbody physics)
  └─ OnTriggerEnter2D → ShipHealth.HitShield() / HitShip()

ShipHealth
  └─ 3 shield hits → shield disabled; next hull hit → GameManager.ShipDestroyed()

GameManager (singleton)
  └─ ShipDestroyed() → freeze time, show game-over panel
  └─ Restart() → reload scene
```

**Tags required in Unity:** `Ship` and `Shield` (on colliders, checked by `Bullet.OnTriggerEnter2D`).

### SDK configuration

Settings live in `Assets/Board/Settings/` as ScriptableObjects:
- `BoardInputSettings` — smoothing, persistence, piece set model filename
- `BoardContactSimulationSettings` — simulator keybindings and rotation speed

## SDK quick reference

### Touch input

```csharp
// Get all active contacts
BoardContact[] contacts = BoardInput.GetActiveContacts();

// Filter by type
BoardContact[] pieces  = BoardInput.GetActiveContacts(BoardContactType.Glyph);
BoardContact[] fingers = BoardInput.GetActiveContacts(BoardContactType.Finger);

// Check if a saved contactId is still active
bool isActive = false;
foreach (var c in BoardInput.GetActiveContacts())
    if (c.contactId == savedId) { isActive = true; break; }
```

### glyphId vs contactId

**Track pieces by `contactId`, not `glyphId`.**

- `glyphId` — identifies the piece *type*; multiple pieces of the same type share the same value.
- `contactId` — unique per contact; no two contacts ever share one. This is the right key for `Dictionary<int, GameObject>` tracking.

### Tracking pieces across frames

```csharp
private Dictionary<int, GameObject> trackedPieces = new();

void Update() {
    var contacts = BoardInput.GetActiveContacts(BoardContactType.Glyph);
    var activeIds = new HashSet<int>();

    foreach (var contact in contacts) {
        activeIds.Add(contact.contactId);

        if (contact.phase == BoardContactPhase.Began) {
            var piece = Instantiate(piecePrefabs[contact.glyphId]);
            trackedPieces[contact.contactId] = piece;
        }

        if (trackedPieces.TryGetValue(contact.contactId, out var obj)) {
            obj.transform.position = ScreenToWorld(contact.screenPosition);
            obj.transform.rotation = Quaternion.Euler(0, 0, -contact.orientation * Mathf.Rad2Deg);
        }
    }

    foreach (var id in trackedPieces.Keys.ToList()) {
        if (!activeIds.Contains(id)) {
            Destroy(trackedPieces[id]);
            trackedPieces.Remove(id);
        }
    }
}
```

### Players & sessions

```csharp
BoardSessionPlayer[] players  = BoardSession.players;
BoardPlayer activeProfile     = BoardSession.activeProfile;

bool added    = await BoardSession.PresentAddPlayerSelector();
bool replaced = await BoardSession.PresentReplacePlayerSelector(existingPlayer);
BoardSession.ResetPlayers();   // reset to active profile only

BoardSession.playersChanged        += OnPlayersChanged;
BoardSession.activeProfileChanged  += OnActiveProfileChanged;
```

### Save games

```csharp
var metadataChange = new BoardSaveGameMetadataChange {
    description = "Level 5 Complete",
    playedTime  = 2700,              // seconds (ulong)
    gameVersion = Application.version,
    coverImage  = screenshotTexture  // Texture2D, converted to 432x243 PNG
};

BoardSaveGameMetadata saved = await BoardSaveGameManager.CreateSaveGame(saveData, metadataChange);
byte[] data                 = await BoardSaveGameManager.LoadSaveGame(saved.id);
// LoadSaveGame also activates the save's players in BoardSession.players

BoardSaveGameMetadata[]  saves = await BoardSaveGameManager.GetSaveGamesMetadata();
Texture2D cover                = await BoardSaveGameManager.LoadSaveGameCoverImage(saved.id);
```

### Pause menu

```csharp
// Call once at startup
BoardApplication.SetPauseScreenContext(
    applicationName: "My Game",
    showSaveOptionUponExit: true
);

// Update individual fields without replacing everything
BoardApplication.UpdatePauseScreenContext(showSaveOptionUponExit: false);

BoardApplication.pauseScreenActionReceived += (action, audioTracks) => {
    switch (action) {
        case BoardPauseAction.Resume:          break;
        case BoardPauseAction.ExitGameSaved:   /* save, then */ BoardApplication.Exit(); break;
        case BoardPauseAction.ExitGameUnsaved: BoardApplication.Exit(); break;
    }
};
BoardApplication.customPauseScreenButtonPressed += (buttonId, audioTracks) => { };

BoardApplication.ShowProfileSwitcher();  // show in menus/lobby
BoardApplication.HideProfileSwitcher();  // hide during gameplay
BoardApplication.Exit();                 // returns to Board Library
```

### Board input settings

Configure via **Edit > Project Settings > Board > Input Settings**:

| Setting | Default | Description |
|---------|---------|-------------|
| Translation Smoothing | 0.5 | 0–1, higher = smoother but more lag |
| Rotation Smoothing | 0.5 | 0–1, higher = smoother but more lag |
| Persistence | 4 | Frames to hold a contact without confirmation |
| Piece Set Model | — | `.tflite` model file in StreamingAssets |

Settings properties are **read-only at runtime**. Switching settings cancels all active contacts:

```csharp
BoardInput.settings = alternateSettings;  // cancels all contacts!
float smoothing = BoardInput.settings.translationSmoothing;
```

## Important notes

- Only one Piece Set Model can be active at a time; switching causes a brief input gap.
- Always exit via `BoardApplication.Exit()` — don't call `Application.Quit()` directly.
- Session always requires at least one Profile player.
- Changing `BoardInput.settings` at runtime cancels all active contacts.

## Additional resources

- Developer docs: https://docs.dev.board.fun
- API reference: https://docs.dev.board.fun/api/
- Setup reference: https://docs.dev.board.fun/getting-started/setup-reference
- Touch input guide: https://docs.dev.board.fun/guides/touch-input
- Player management: https://docs.dev.board.fun/guides/player-management
- Save games: https://docs.dev.board.fun/guides/save-games
- Pause menu: https://docs.dev.board.fun/guides/pause-menu
- Simulator: https://docs.dev.board.fun/guides/simulator
- Changelog: https://docs.dev.board.fun/more/changelog
