# Task Brief: Patient Zero — Gate 0 QA Pass (Cursor)

## Context

Gate 0 (Unity environment, both build profiles, 2-player local input) was
built by Fable 5 from `fable5-task-brief-gate0.md`, then further built on
and tested against the live AWS pipeline outside of Cursor. Per
`docs/05-build-pipeline.md`, this QA pass is **Piece 3** of the build
structure — deliberately a separate, independent reviewer, specifically
because a builder's own self-verification misses things a fresh reviewer
catches. You are that fresh reviewer. You are not building anything new.

## Already independently verified — do not re-verify, trust these and move on

- Both build profiles (`BuildScript.BuildWebGL` / `BuildDesktop`) run
  through the real `scripts/build-manager.ps1` pipeline, not just compile
  in-editor — confirmed via actual log output showing `return code 0`.
- The WebGL build is live and confirmed serving real content (HTTP 200,
  correct HTML/canvas) at a deployed CloudFront URL.
- Console-mode Player Settings confirmed live: built .exe launches into a
  borderless 1920×1080 window, no resolution dialog.
- Git LFS confirmed genuinely tracking binary assets — checked that a
  staged PNG's git blob is an actual LFS pointer, not the raw file.
- 2-player join logic reviewed at a high level: it's one script,
  `Assets/Scripts/Input/TwoPlayerJoinController.cs`, capping
  `PlayerInputManager` at `MaxPlayers = 2`.

Re-running any of the above from scratch is wasted scope. Spend your time
on what's below instead.

## Your job — four checklist items, in priority order

### 1. Physical two-gamepad hardware test

Fable 5's own join-test used the Input System's virtual/synthetic device
test fixtures, not real hardware. Nobody has confirmed this with actual
gamepads yet.

**Xbox controllers (XInput) are the required, officially-tested standard —
but they are not the only thing that has to work.** The input bindings in
`Assets/Scripts/Input/PlayerControls.inputactions` are intentionally left
generic (`<Gamepad>`, not `<XInputController>`), specifically so kids can
use whatever gamepad their family already owns — PlayStation, Switch Pro,
generic USB/Bluetooth — without anyone needing to buy new hardware. Do
not tighten that binding. Xbox is the *official pass/fail standard*
because it's driver-free on Windows and gives a consistent baseline
across different kids' machines, not because other controllers are
unsupported.

Known real differences across brands, worth being aware of even though
they're not blockers: analog stick deadzone/drift varies by controller
quality; some third-party pads report the d-pad as an 8-way HAT switch
rather than Xbox's discrete buttons, which can feel different; and a
sufficiently non-standard/generic pad may fail Unity's device
auto-detection entirely, in which case Move and Join simply won't fire —
no crash, no error, just silence. If a kid's controller doesn't seem to
do anything, that's the first thing to check, and swapping to Xbox
hardware is the known-good fallback.

**You almost certainly cannot press physical buttons yourself.** Don't
claim this passed. Instead:
- Write a precise, step-by-step manual test procedure for two Xbox/
  XInput controllers (wired or wireless — both use the same XInput
  driver on Windows): exact button to press, what should happen
  on-screen for each of: player 1 joins, player 2 joins, a 3rd pad is
  rejected, a player leaving re-opens a join slot. **This Xbox procedure
  is what the required Pass/Fail verdict is based on** — do not
  substitute a different brand for the official result.
- If a non-Xbox controller happens to be available and you want to note
  how it behaves, that's fine and useful — but record it as a *separate,
  clearly-labeled best-effort note* in the scorecard (e.g. "Also tried:
  PS4 controller — worked / didn't / felt different, because X"), never
  as a substitute for the required Xbox result.
- No Xbox controllers were available as of this brief being written —
  mark the required item **Needs Human Execution** in your report (see
  Deliverables) rather than asserting a pass. If that changes and
  Xbox controllers become available, run the procedure live with
  whoever's running you, watching Unity's Console output, and report
  the real result instead.

### 2. Independent code review

Go through `Assets/Scripts/`, `Assets/Editor/`, and `scripts/*.ps1` with
fresh eyes — not looking for what Fable 5 said it did, looking for what's
actually there. Specifically worth checking:
- Edge cases in `TwoPlayerJoinController.cs` and `PlayerMover.cs` (rapid
  join/leave, unusual join ordering, etc. — mid-session disconnect
  specifically is its own item, #3 below)
- Anything in `BuildScript.cs` or `ProjectSetup.cs` that's fragile or
  silently assumes something not guaranteed to be true
- General code quality: naming, dead code, anything that would confuse
  the next person (a kid, possibly) reading this

If you find a **real bug**, fix it directly as a diff — don't just
describe it in prose. If you find something that's a judgment call or
genuinely out of scope, flag it in the report instead of touching it.

### 3. Mid-session controller disconnect behavior

Nobody has verified what actually happens if a joined player's controller
disconnects mid-session — dead battery, walked out of Bluetooth range,
cable unplugged. Fable 5's automated tests covered join-in-progress and
the 3-player cap, but never a disconnect *after* a player already joined.
This is a real gap, not just a documentation gap — it's untested
behavior, not known behavior.

**Unlike the physical hardware test above, this one doesn't need real
hardware at all.** Unity's Input System supports simulating a device
removal mid-test via `InputSystem.RemoveDevice()` — the same
virtual-device approach Fable 5 already used in
`Assets/Tests/PlayMode/TwoPlayerJoinTests.cs`. Write a Play Mode test
that: joins two virtual gamepads, removes one mid-test, and observes
what actually happens:
- Does `TwoPlayerJoinController.HandlePlayerLeft` actually fire?
- Does the disconnected player's character get cleaned up (destroyed),
  or does it sit frozen/orphaned in the scene?
- Does joining correctly re-open afterward for a new (3rd) device, per
  the existing cap logic?

Report the real, observed behavior in the scorecard — not an assumption
of "should work." If it's ugly (a frozen half-visible character, no
cleanup), that's a legitimate finding: fix it directly if it's a small,
contained fix; if it's bigger than that, flag it clearly rather than
scope-creep into a real fix.

### 4. Public-repo safety pass

This repo is now public on GitHub (`github.com/terrence-cmd/patient-zero`),
and multiple kids will eventually be pointed at it with their own Cursor
access. One secret-scan already ran before the initial push and came back
clean, but given who's coming next, a second independent pass matters:
- Re-scan the full repo history (not just current files) for anything
  that looks like a credential, access key, account ID, or personal
  info — `git log -p` or equivalent, not just a working-tree grep.
- Confirm the out-of-scope boundaries from `fable5-task-brief-gate0.md`
  actually hold in the shipped code — no AWS config, no networking/
  netcode, no combat system beyond the placeholder capsule, nothing
  beyond what the acceptance criteria list.

## Deliverables (all required — this is what "done" looks like)

1. **A QA scorecard**, written to `docs/07-cursor-qa-report.md`: one row
   per checklist item above, status (Pass / Fail / Needs Human
   Execution), and the concrete evidence or reasoning behind each status
   — not just a bare verdict.
2. **Any real bugs found, fixed directly** as commits in the repo (not
   just described in the report). Reference the fix in the scorecard.
3. **One confirmation build after any fixes**, both targets, via:
   ```
   .\scripts\build-manager.ps1 -PersonName "kenshi" -Target Desktop
   .\scripts\build-manager.ps1 -PersonName "kenshi" -Target WebGL
   ```
   Paste or reference the actual success output in the scorecard. If you
   made no code changes, a confirmation build is still required — prove
   nothing regressed.
4. **A single verdict line at the very top of the scorecard**: either
   `Gate 0 complete` or `Gate 0 blocked on: <specific thing>`. One line,
   unambiguous, first thing anyone reading the file sees.

## Out of scope — do not build any of this

- Any new gameplay feature, Gate 1 combat/frame-data/netcode work, or
  content beyond what already exists
- Any AWS/infrastructure change of any kind — if something AWS-related
  looks wrong, flag it in the report, do not touch it
- Anything not explicitly listed in the four checklist items above,
  even if it seems like an obvious small fix

If you find yourself about to build or change something not covered by
this brief, stop and flag it in the report instead.

## Environment facts

- Unity Editor: `C:\Program Files\Unity\Hub\Editor\6000.3.21f1\Editor\Unity.exe`
  (pinned LTS, already installed — do not reinstall or change version)
- Repo root / project path: this directory
  (`C:\Users\tocam\Documents\GameDev\patient-zero`)
- AWS targets already provisioned are tracked in `PROVISIONED_TARGETS.md`
  — read-only reference, not something to modify here
- Platform is Windows; PowerShell for anything outside C#/Unity
