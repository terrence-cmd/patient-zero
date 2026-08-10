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

## Your job — three checklist items, in priority order

### 1. Physical two-gamepad hardware test

Fable 5's own join-test used the Input System's virtual/synthetic device
test fixtures, not real hardware. Nobody has confirmed this with actual
gamepads yet.

**You almost certainly cannot press physical buttons yourself.** Don't
claim this passed. Instead:
- Write a precise, step-by-step manual test procedure (which two USB/
  Bluetooth gamepad types to use, exact button to press, what should
  happen on-screen for each of: player 1 joins, player 2 joins, a 3rd
  pad is rejected, a player leaving re-opens a join slot).
- If the human running you (Terrence) has gamepads connected and wants
  to run it live with you watching Unity's Console output, do that and
  report the real result.
- Otherwise, mark this item **Needs Human Execution** in your report
  (see Deliverables) rather than asserting a pass.

### 2. Independent code review

Go through `Assets/Scripts/`, `Assets/Editor/`, and `scripts/*.ps1` with
fresh eyes — not looking for what Fable 5 said it did, looking for what's
actually there. Specifically worth checking:
- Edge cases in `TwoPlayerJoinController.cs` and `PlayerMover.cs`
  (device disconnect mid-session, rapid join/leave, etc.)
- Anything in `BuildScript.cs` or `ProjectSetup.cs` that's fragile or
  silently assumes something not guaranteed to be true
- General code quality: naming, dead code, anything that would confuse
  the next person (a kid, possibly) reading this

If you find a **real bug**, fix it directly as a diff — don't just
describe it in prose. If you find something that's a judgment call or
genuinely out of scope, flag it in the report instead of touching it.

### 3. Public-repo safety pass

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
- Anything not explicitly listed in the three checklist items above,
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
