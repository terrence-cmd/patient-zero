# Game Flow — Patient Zero spine

Diff of the Claude Design **2D Fighting Game Flow** PDF against this repo,
plus the basic collection that was added to close the gap.

Last updated: 2026-08-12.

## Diff summary

| PDF state | Before | After (this pass) |
|---|---|---|
| Boot / Splash (`GameState.Boot`, 2.0s) | Missing | `GameFlowDirector` + `Assets/Scenes/Boot.unity` |
| Title (`GameState.Title`) | Missing | `Assets/Scenes/Title.unity` + Submit → Join |
| Player Join (`TwoPlayerJoinController`) | Existed on Main only | Gated as flow state; still uses existing join controller |
| Character Select | Libraries only | `CharacterSelectSession` + stub HUD cursors/locks |
| Opening / VS (4.0s, skip after 1.0s) | `ApplyStagePresentation` only at Main Awake | Timed Opening state; calls `FightDirector.ApplyStagePresentationPublic()` |
| Round Start READY→FIGHT | Missing | Timed banners; gameplay map locked until FIGHT |
| Fight Loop + 99s clock | Combat yes, no clock / match end | Round clock + KO / Time Over → Match End |
| Match End (2.5s freeze) | Missing | Match End → Results |
| Results rematch / quit | Missing | Rematch → Round Start; Cancel → Title |
| Pause (`timeScale = 0`) | Missing | Overlay on Fight; device-lost → pause while Fighting |
| UI action map | Missing | Still stub (keyboard/gamepad helpers) |
| Real HUD art | IMGUI HP text | Stub OnGUI banners (`GameFlowHud`) |

**Unchanged / preserved:** Gate 0 cold-start into `Main` still works without the flow spine. `TwoPlayerJoinController` mid-session disconnect destroy behavior remains when not in `GameState.Fight`.

## How to run

1. In Unity: **Patient Zero → Flow → Setup Game Flow** (idempotent; wires libraries + build order).
2. Play from **Boot** (build index 0): Boot → Title → Join → Select → VS → READY/FIGHT → Fight → Results.
3. Or open **Main** directly for the old Gate 0 join-and-fight path.

### Stub controls

| Context | Input |
|---|---|
| Title / skip Opening / Results rematch | Enter / Space / Gamepad South |
| Results quit / Pause quit | Esc / Gamepad East |
| Pause toggle | Esc / Gamepad Start |
| Char select P1 | A/D + F |
| Char select P2 | ←/→ + `.` (period) |

## Key types

- `Assets/Scripts/Flow/GameState.cs`
- `Assets/Scripts/Flow/GameFlowTimings.cs`
- `Assets/Scripts/Flow/GameFlowDirector.cs`
- `Assets/Scripts/Flow/CharacterSelectSession.cs`
- `Assets/Scripts/UI/GameFlowHud.cs`
- `Assets/Editor/GameFlowSetup.cs`
