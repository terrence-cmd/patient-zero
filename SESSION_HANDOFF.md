# Session Handoff

Authoritative record of where this project stands, for picking this back
up in a future session with zero memory of how it got here. If this
conflicts with anything else in the repo, this file wins — it's more
current.

Last updated: 2026-08-12 (match-flow spine added from Claude Design Game
Flow PDF — see [`docs/08-game-flow.md`](docs/08-game-flow.md). Boot → Title
→ Join → Character Select → Opening/VS → Round Start → Fight → Match End →
Results + Pause. `Main.unity` itself unchanged, but **the Desktop build's
default launch scene changed — see scene-order note below.** Headless
build verified (0 errors/0 warnings); not yet live-verified in the Unity
Editor / by play on this machine.)

**Landing status (2026-08-12, this session):** the match-flow spine landed
on `master` via [PR #1](https://github.com/terrence-cmd/patient-zero/pull/1)
(merge commit `19344af`), merged by Terrence. It got there via
`cursor/game-flow-spine-03e4`, built by a Cursor background agent that
couldn't push to `patient-zero` directly (403) and handed the patch off
via a draft PR (`terrence-cmd/rig-redesign#14`) with a raw-file download
link; Claude Code fetched and reviewed that patch, applied it with
`git am`, pushed the branch, opened PR #1, and Terrence merged it.
**`rig-redesign` PR #14 and its `cursor/patient-zero-flow-patch-handoff-03e4`
branch are closed/deleted** — that repo is not part of patient-zero's
history, it was only a one-time handoff vehicle. The
`cursor/game-flow-spine-03e4` branch is now merged and safe to delete.
**Still not live-verified in the Unity Editor on this machine** — that's
the next real gate before trusting the flow, not the merge itself.

**Headless verification done (2026-08-12), and one real finding from it:**
ran `.\scripts\build-manager.ps1 -PersonName "terrence" -Target Desktop`
on the merged `master` — build **succeeded, 0 errors, 0 warnings**
(`Builds/Desktop/PatientZero.exe`, fresh `PatientZero_Data`). That's as
far as headless checking goes — no tool here can drive the Editor's Game
view or a built exe interactively, so the actual screens (Title, Join
prompts, Character Select, 99s clock, Results+Pause) are still unseen.

**Scene-order change worth knowing before running Gate 0's controller
procedure again:** the patch inserted `Boot.unity` and `Title.unity`
*ahead of* `Main.unity` in `ProjectSettings/EditorBuildSettings.asset`:

```
0: Assets/Scenes/Boot.unity
1: Assets/Scenes/Title.unity
2: Assets/Scenes/Main.unity
```

Scene index 0 is what a Desktop build cold-launches into. So
double-clicking `PatientZero.exe` now boots into **Boot → Title → Join
→ …**, not straight into `Main` like before. The merge commit's claim
that "Gate 0 Main cold-start stays intact" is true only in the sense
that `Main.unity`'s own contents weren't edited — the **default launch
path changed**. `docs/07-cursor-qa-report.md` §1's official controller
procedure was written assuming a cold launch drops straight into `Main`;
that assumption no longer holds against this build. Terrence is playing
through the new flow directly to confirm/adjust from here.

Previously: 2026-08-10, later still the same day (Cursor built a first
side-scroller fight system — characters, stage, health — on top of Gate 1
combat; live-verified P1 gamepad + P2 keyboard join and per-character
moves; damage/KO not yet confirmed live and the work is **uncommitted** —
see "Side-scroller fight system" below). Gate 0 itself is unchanged: still
waiting on a second pad for the official two-controller Pass.

---

## Where things actually stand

**Match flow (2026-08-12):** Basic collection is in tree — `GameState`,
`GameFlowDirector`, Boot/Title scenes, stub HUD, 99s clock, rematch/quit,
pause. Run **Patient Zero → Flow → Setup Game Flow**, then Play from Boot.
Details and PDF diff: [`docs/08-game-flow.md`](docs/08-game-flow.md).

**Gate 0 is functionally done, blocked on exactly one official item:** a
**second** physical Xbox controller so the scorecard's item 1 procedure
can be finished end-to-end. Everything else in
[`cursor-qa-brief-gate0.md`](cursor-qa-brief-gate0.md) is done, verified,
committed, and pushed.

Current verdict line in
[`docs/07-cursor-qa-report.md`](docs/07-cursor-qa-report.md):

`Gate 0 blocked on: physical Xbox two-gamepad hardware test (Needs Human Execution)`

That status is still correct for the *official* Pass/Fail (two Xbox
pads). One wired Xbox Series controller has since been proven as P1 on
the Desktop build — see "Hardware test session" below. That does **not**
flip item 1 to Pass by itself.

---

## Cursor Gate 0 QA pass — full detail

This was Piece 3 of the build pipeline ([`docs/05-build-pipeline.md`](docs/05-build-pipeline.md)):
independent reviewer role per [`cursor-qa-brief-gate0.md`](cursor-qa-brief-gate0.md),
not a builder inventing Gate 1 scope. Standing rules came from
[`.cursor/rules/patient-zero-qa.mdc`](.cursor/rules/patient-zero-qa.mdc)
(read `INTEGRITY.md` + the QA brief; no scope creep; verify don't trust;
`Co-Authored-By:` on Cursor commits, never `Authorship:`).

### Scorecard outcomes

| Item | Status | One-line evidence |
|---|---|---|
| 1 Physical Xbox 2-pad test | **Needs Human Execution** (partial progress: P1 wired OK) | Official procedure needs two pads; see hardware section |
| 2 Code review | **Pass** | Review + one fix (`PlayerMover`), re-verified in Play Mode |
| 3 Mid-session disconnect | **Pass** | Failed once → root-caused → fixed → Passed |
| 4 Public-repo safety / scope | **Pass** | Credential-pattern history scan clean; Gate 0 boundaries hold |
| Confirmation builds | **Pass** | Desktop + WebGL via `build-manager.ps1 -PersonName kenshi`, exit 0, artifacts checked independently |

### Item 3 — disconnect (the main engineering finding)

**Bug:** After two players joined, removing a device did not clean up the
player or re-open the join slot. Orphaned capsule + stuck `DisableJoining`.

**First Play Mode run (real failure, not assumed):**
`MidSessionDisconnect_CleansUpPlayer_AndReopensJoinSlot` **Failed** —
`onPlayerLeft` count stayed 0; console only showed the two join lines, no
device-lost log. Original `TwoGamepads_BothJoin_...` **Passed** on the
same run (Unity exit code 2 overall).

**Root cause:** An early fix subscribed to `PlayerInput.onDeviceLost`.
That C# event **only fires when** `PlayerInput.notificationBehavior ==
InvokeCSharpEvents`. The player prefab has `m_NotificationBehavior: 0`
(**SendMessages**, Unity's default). `PlayerInput.HandleDeviceLost()`
switches on that enum and never touches the C# callback array in
SendMessages mode. So the subscription was silently a no-op.

**Fix (what's in tree now):** `TwoPlayerJoinController` listens once to
the **static** `InputUser.onChange` in `OnEnable`, filters
`InputUserChange.DeviceLost`, finds `PlayerInput` where `player.user ==
user`, `Destroy`s that GameObject (which makes `PlayerInputManager` fire
`onPlayerLeft` → existing cap logic re-enables joining), and unsubscribes
in `OnDisable`. Not a per-player handler — one controller-lifetime
subscription; filter by user so it doesn't thrash the wrong player.

**Why not flip the prefab to InvokeCSharpEvents?** Would also work, but
ties cleanup to a prefab notification setting that `ProjectSetup` never
set on `PlayerInput` (it only set C# events on `PlayerInputManager`).
`InputUser.onChange` matches the actual device-removal path Unity uses
(`InputDeviceChange.Removed` → DeviceLost on the user) and stays correct
even if the prefab stays on SendMessages.

**Test:**
`Assets/Tests/PlayMode/TwoPlayerJoinTests.cs` →
`MidSessionDisconnect_CleansUpPlayer_AndReopensJoinSlot`
(join two virtual pads → `RemoveDevice` → assert left event, destroyed
GO, count 1 → third pad takes the free slot). Do **not** assume
`PlayerInput.all` order after a mid-list disconnect — that assertion
failed once after the fix worked; pairing is checked by scanning all
players for the new device.

**Final verification:** both Play Mode tests **Passed**, Unity exit
code **0**.

### Item 2 — code review + PlayerMover

Reviewed `Assets/Scripts/`, `Assets/Editor/`, `scripts/*.ps1`.

**Fix:** `PlayerMover` used `rend.material.color = ...`, which
instantiates a material per join (leaks under join/leave/disconnect).
Switched to `MaterialPropertyBlock` + `_Color` (Built-in capsule shader).

**Re-verification (same bar as item 3, not "low risk so skip"):** after
the change, Play Mode batch again — both tests **Passed**, exit 0, no
`error CS` in `test-run-playmode-playermover.log`.

**Flags left as judgment (not changed):** same-frame join-cap race;
`ProjectSetup.ApplyAll` overwrites Main/prefab; `BuildScript` success
relies on `-quit`; `build-manager` doesn't check Editor lock; spawn/color
follow `playerIndex` after rejoin. Details in the scorecard.

### Item 4 — safety / scope

- Working tree + full history: no hits for AKIA/ASIA, aws_secret_access_key=,
  PEM/OpenSSH private key headers, ghp_/github_pat_/sk- shapes.
- `PROVISIONED_TARGETS.md` lists account id / bucket / CloudFront ids /
  URLs on purpose — **not** access keys; keys live outside the repo.
- Scope: only `PlayerMover.cs` + `TwoPlayerJoinController.cs` under
  Scripts; Combat/Netcode empty placeholders; Backend only `.gitkeep`;
  `BuildScript.BuildWebGL` / `BuildDesktop` names unchanged.

### Confirmation builds (verify-don't-trust lesson)

```
.\scripts\build-manager.ps1 -PersonName "kenshi" -Target Desktop
.\scripts\build-manager.ps1 -PersonName "kenshi" -Target WebGL
```

Both returned exit **0**. Independent checks:

- **Desktop:** stub `PatientZero.exe` / `UnityPlayer.dll` may keep older
  timestamps (identical native binaries often not rewritten). Trust
  **player content** instead — `PatientZero_Data\boot.config`, `level0`,
  `globalgamemanagers*` refreshed in the build window (~12:06 PM local).
  Unity log: `[BuildScript] StandaloneWindows64 build SUCCEEDED ... 0 errors`.
- **WebGL:** `index.html` timestamp may stay old (template); trust
  `Builds/WebGL/Build/WebGL.data|.wasm|.framework.js|.loader.js`
  (~12:19 PM). Deployed; Play URL
  `https://d22jn5ymxt1ztg.cloudfront.net`.

Logs produced locally (untracked on purpose): `qa-confirm-desktop.log`,
`qa-confirm-webgl.log`, `build-log-Desktop.txt`, `build-log-WebGL.txt`,
`TestResults-PlayMode.xml`, `test-run-playmode*.log`.

### Commits pushed (atomic, three commits — do not squash)

Pushed to `origin/master` (`b33579f..17f7fa2`):

| SHA | What |
|---|---|
| `5f44fe5` | Disconnect fix + Play Mode test — root cause SendMessages vs onDeviceLost |
| `aee2a02` | PlayerMover MaterialPropertyBlock — material leak; re-verified |
| `17f7fa2` | `docs/07-cursor-qa-report.md` scorecard |

Attribution: `Co-Authored-By: Cursor <cursoragent@cursor.com>` only — **no**
`Authorship:` trailer (that trailer is for human learner commits per
`INTEGRITY.md` / the QA rule).

God Mode path in `UNITY_INSTALL.md` needs code on GitHub — these are
pushed, so that help path can see this work.

---

## Hardware test session — the part beyond Cursor's automated QA

A real Xbox controller showed up the same day: **model 1914 (Xbox Series
X|S wireless controller)**. What actually happened, in order:

1. **Bluetooth pairing succeeded at Windows** (`Xbox Wireless Controller`,
   Status OK) but **the game saw no input.**
2. Unity Input Debugger: device `045E:0B13` (Microsoft VID, Bluetooth PID)
   landed in unsupported/raw HID — not matched to a `Gamepad` layout.
3. **Known cross-platform gap**, not a Patient Zero bug — Xbox Series over
   Bluetooth (`045E:0B13`) has documented recognition/mapping issues.
4. **Fix: wired USB-C.** Same pad over USB reports `045E:0B12` /
   `XboxComposite` / XINPUT-compatible path.
5. **P1 verified with real Desktop `Player.log` evidence:**
   ```
   [Join] Player 1 joined (devices: XInputControllerWindows:/XInputControllerWindows)
   ```

**Still not done for official item 1:** second physical Xbox controller
for P2 join, dual-stick isolation, 3rd-pad rejection, and
disconnect/rejoin on real hardware. Automated Play Mode tests already
cover that logic without a second pad.

---

## Gate 1 combat system — built, one real bug found and fixed, verified live

Later the same day, Cursor built the first slice of Gate 1 (previously
explicitly out of scope for Gate 0) and a real bug in it got diagnosed
jointly with Claude Code and fixed by Cursor. Full narrative transcript in
[`SESSION_TRANSCRIPT_2026-08-10.md`](SESSION_TRANSCRIPT_2026-08-10.md).

**What's in tree, three commits (`34fdd07`, `f009129`, `e53ad21`),
pushed to `origin/master`:**

- **Data-driven move/stage libraries** — `MoveDefinition`/`MoveLibrary`/
  `StageDefinition`/`StageLibrary` `ScriptableObject`s, 5 demo moves
  (`light_punch`, `heavy_punch`, `light_kick`, `heavy_kick`, `crouch_jab`,
  plus `hadoken` as a "special"), 3 demo stages, an Editor setup tool.
- **`FighterCombat`** — executes a move's startup→active→recovery at a
  fixed 60 FPS, shows the active hitbox, applies hitstun on overlap.
  Wired onto the player prefab with gamepad attack bindings
  (`buttonWest/North/South/East` → punches/kicks, shoulders →
  crouch/special) **and keyboard bindings** (WASD move, J/I/K/L
  punches/kicks, U/O crouch/special) — the keyboard binding means a
  second player can be tested solo, without a second physical controller.
  `PlayerMover` now locks movement while `FighterCombat.LocksMovement` is
  true (mid-move / hitstun).

**The bug: join succeeded, but no action ever fired — not combat-specific,
the whole `Player` action map (Move included) was dead after a real
hardware join.** Root-caused jointly, not guessed at:

- Static wiring was all provably correct first (prefab component,
  `MoveLibrary` asset GUID, `PlayerControls.inputactions` GUID and
  bindings, no exceptions anywhere in `Player.log`) — ruled out a dangling
  reference or wrong asset.
- `Assets/Tests/PlayMode/FighterCombatTests.cs` **passed cleanly but never
  actually tested this path** — both tests have `// Bypass join grace +
  input map: start by id` and call `TryStartMove()` directly, never
  simulating a real button press through the join→action-map handoff.
  "Tests passed" gave zero real coverage here.
- Confirmed live, twice, with a controller power-cycle between attempts to
  rule out a USB/wake artifact: after a real hardware join, **movement
  didn't work either** — proof this wasn't combat-specific, it was the
  entire `Player` map never coming alive post-join. The `Player.prefab`
  diff that added `FighterCombat` never touched the pre-existing
  `PlayerInput`/`PlayerMover` config, meaning this was very likely a
  **pre-existing gap Gate 0's hardware test never actually caught** — its
  only success criterion was ever the `[Join]` log line, never real
  post-join movement.

**Fix (`e53ad21`, Cursor):** bind the attack `InputAction` references in
`OnEnable`/`Start` instead of `Awake`, and log a verdict line —
`[FighterCombat] P{n} ready (LightPunch enabled=..., devices=...)` —
right after join, so enable/pairing state is directly observable in
`Player.log` without needing the Unity Input Debugger.

**Verified live, real hardware, after a full controller power-cycle:**

```
[Join] Player 1 joined (devices: XInputControllerWindows:/XInputControllerWindows)
[FighterCombat] P1 ready (LightPunch enabled=True, devices=1)
[Combat] P1 started light_punch (3/2/7 (total 12)) facing=+X
[Combat] P1 started heavy_punch (8/3/16 (total 27)) facing=+X
[Combat] P1 started heavy_kick (10/4/18 (total 32)) facing=+X
[Combat] P1 started light_kick (5/3/10 (total 18)) facing=+X
```

All four normal attacks fired correctly off the real gamepad after the
fix. At this point in the day, health bars, blocking, cancels,
projectiles, crouch state, and animation were still explicitly out of
scope per `FighterCombat.cs`'s own doc comment. **That changed later the
same day — see "Side-scroller fight system" below for health/characters/
stages; blocking, cancels, projectiles, crouch state, and animation are
still not built.**

One side note from earlier in the same session, worth not re-chasing: a
Desktop rebuild's own `PatientZero.exe` launcher stub can look
byte-identical / same timestamp across builds while `PatientZero_Data`
genuinely updates — this is expected Mono-backend + Unity incremental
build behavior (the exe is a thin bootstrapper; all real code lives in
`PatientZero_Data`), not a broken build. Already covered under
"Confirmation builds" below; re-confirmed the hard way tonight before
being certain it wasn't a bug.

---

## Side-scroller fight system — built, live-verified through join/movement, uncommitted

Still the same day: Terrence had Cursor build an MK-style 2D side-scroller
fight system on top of Gate 1 combat — deliberately chosen over 3D as the
faster, more fun path to something playable. **As of this write-up, none
of this is committed** — `git status` shows it all as modified/untracked
in the working tree. Don't assume it's safe in git; check `git status`
before trusting this section is still accurate.

**New pieces, all cross-checked against real asset GUIDs (not assumed):**

- **`FighterHealth`** — plain HP (`Configure`/`ApplyDamage`/`IsKnockedOut`),
  wired into `FighterCombat` (damage applied on a landed hit) and
  `PlayerMover`/`FighterCombat` (KO blocks movement and further attacks).
- **`CharacterDefinition`** — per-character move library **and** a
  button→moveId override map, so different characters throw different
  named moves off the same six physical buttons. `FighterCombat.ApplyCharacter()`
  applies one at runtime.
- **`FightDefinition`** / **`FightDirector`** — a `FightDefinition` asset
  bundles a stage, P1/P2 `CharacterDefinition`s, starting HP, and a
  side-view-movement flag. `FightDirector` (new, on `PlayerManager` next
  to `PlayerInputManager`) assigns a character per joining player,
  configures `FighterHealth`, switches `PlayerMover` to side-view-only +
  clamps to stage bounds, tints each fighter, positions camera/ground for
  the stage, and draws a basic on-screen HP HUD via `OnGUI`.
- **`Fight_BasicSideScroller`** asset — `Stage_StoneCourtyard`,
  `Character_Warrior` (P1) vs `Character_Shadow` (P2), 100 HP,
  side-view movement on. All three referenced GUIDs confirmed resolving
  to real assets, not dangling.
- **Keyboard bindings from the `e53ad21` fix turned out to double as a
  solo 2-player test path** — WASD/J/I/K/L/U/O lets one person test the
  full 2-fighter loop (gamepad as P1, keyboard as P2) without a second
  physical controller. Useful beyond just Editor testing.

**Verified live, real hardware, after a fresh rebuild (0 errors, 0
warnings; confirmed via the real Unity build log, not just exit code):**

```
[Fight] Stage 'stone_courtyard' applied (basic_side_scroller ...: stage=stone_courtyard, P1=warrior, P2=shadow, hp=100)
[Join] Player 1 joined (devices: XInputControllerWindows:/XInputControllerWindows)
[FighterCombat] P1 ready (LightPunch enabled=True, devices=1)
[Fight] P1 → warrior
[Join] Player 2 joined (devices: Keyboard:/Keyboard)
[FighterCombat] P2 ready (LightPunch enabled=True, devices=1)
[Fight] P2 → shadow
[Combat] P1 started high_kick ...
[Combat] P2 started high_punch ...
```

Confirmed real per-character move names differ from the earlier shared
demo list (`high_kick`/`low_kick` for `warrior` vs `high_kick`/`high_punch`
for `shadow`) — proof `ApplyCharacter()` genuinely overrides the move set,
not just cosmetic.

**Not yet confirmed live: damage actually landing.** No
`[Combat] ... hit ...` or `[Health] ...` log line appeared in this
session — the two fighters were never actually maneuvered into range of
each other before the session ended. The code path exists
(`FighterCombat.TryHitOpponents()` → `FighterHealth.ApplyDamage()`) and
looks correct on inspection, but "looks correct" and "verified live" are
not the same thing per this project's own house rule — whoever picks this
up next should land a real hit and confirm both the `[Combat] ... hit ...`
log and the on-screen HP HUD actually move before trusting it further.

---

## Immediate next steps, in order

1. **Get a second Xbox controller** (or borrow one). Prefer **wired** for
   Xbox Series + Unity (tonight's lesson). Run the exact procedure in
   `docs/07-cursor-qa-report.md` §1.
2. **Update the scorecard** — item 1 status + top verdict line. If the
   full Xbox procedure passes: `Gate 0 complete`.
3. **Fold Bluetooth-vs-wired into `CONTROLLER_SETUP.md`** as a real
   troubleshooting entry (Series over BT may show connected in Windows
   but never become a Unity Gamepad; use wired). Not written into that
   doc yet — flagged here so it isn't lost.
4. Lower priority, already tracked in README "Known gaps": Xbox in-box
   cable clarification.
5. Optional cleanup: delete or gitignore local QA artifact logs listed
   above if they clutter the working tree (they were never committed).

---

## Process / environment state

- Unity Editor: **6000.3.21f1** at
  `C:\Program Files\Unity\Hub\Editor\6000.3.21f1\Editor\Unity.exe`
  (pinned — do not reinstall or bump for Gate 0).
- Repo root:
  `C:\Users\tocam\Documents\GameDev\patient-zero`
- Branch: `master`, in sync with `origin/master` (fast-forwarded through
  the PR #1 merge commit `19344af`, which brought in the match-flow
  spine). Run `git status` / `git log -5` on pickup — don't assume this
  stays accurate forever.
- During hardware testing, Standalone `PatientZero.exe` and/or Unity
  Editor (Input Debugger) may still be open — check Task Manager /
  `tasklist` before assuming either way; close Editor before
  `build-manager.ps1` (Library lock — see `BUILD_AND_DEPLOY.md`).
- Xbox model 1914: use **wired** for reliable Unity input; Bluetooth
  pairing can remain in Windows unused.

---

## Everything else — confirmed solid, no action needed for Gate 0

- **Repo**: public `github.com/terrence-cmd/patient-zero`.
- **AWS**: 6 targets (`kenshi`, `JohhnyCage`, `SubZero`, `Scorpian`,
  `SonyaBlade`, `Kitana`) — [`PROVISIONED_TARGETS.md`](PROVISIONED_TARGETS.md).
  Non-owner credentials live in
  `C:\Users\tocam\Documents\PatientZero_Credentials.docx` (**never**
  commit that file).
- **Unity install / failure modes**: [`UNITY_INSTALL.md`](UNITY_INSTALL.md).
- **Docs / rules already in tree**: `INTEGRITY.md`, `CONTROLLER_SETUP.md`,
  `BUILD_AND_DEPLOY.md`, `CURSOR_SETUP.md`, `GAME_FILE_STORAGE.md`,
  `.cursor/rules/patient-zero-qa.mdc`, `cursor-qa-brief-gate0.md`,
  `docs/00`–`06`, `docs/07-cursor-qa-report.md`, and now
  `SESSION_TRANSCRIPT_2026-08-10.md` (truncated transcript of tonight's
  combat/input debugging session).
- **Runtime surface for Gate 0** remains tiny on purpose:
  `TwoPlayerJoinController`, `PlayerMover`, `Main` scene, `BuildScript` /
  `ProjectSetup`, `build-manager.ps1` / `provision-aws-target.ps1`.
- **Gate 1 has now started** — see "Gate 1 combat system" above. The
  first slice (data-driven moves/stages, executable frame data, real
  hitstun, gamepad + keyboard input) is built and verified live. Health
  bars, blocking, cancels, projectiles, crouch state, and animation are
  still explicitly not started.

---

## Quick resume checklist for the next agent / human

1. Read this file + [`docs/07-cursor-qa-report.md`](docs/07-cursor-qa-report.md).
2. `git pull` on `master`. If picking up the match-flow spine work, note
   it merged via PR #1 but is **still un-verified in the Unity Editor** —
   live-verify before trusting it.
3. If continuing hardware: two Xbox pads, **wired**, Desktop build or
   Editor Play on `Main`, follow scorecard §1; then edit scorecard
   verdict.
4. If continuing docs: write the Bluetooth/wired finding into
   `CONTROLLER_SETUP.md`.
5. If changing code: close Unity Editor before batch builds; re-run the
   two Play Mode tests after any join/input change; do not trust exit
   codes alone.
