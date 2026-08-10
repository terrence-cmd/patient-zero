# Task Brief: Fighter Project — Gate 0 Environment Build

## Context

This is the environment setup for a 2D fighting game project, developed
across two build targets from one Unity project: a WebGL build (early
prototyping/sharing) and a Windows Standalone build (the real dev target,
since fighting games need frame-perfect timing that WebGL can't reliably
guarantee). Multiple people, each on their own PC, will eventually clone
this same template and work in parallel — so the output of this task needs
to be a clean, reusable starting point, not a one-off.

You have a lot of latitude here. This brief describes the destination and
the acceptance criteria, not a step-by-step script. Explore the environment,
figure out what's actually needed at each stage, write your own tests where
that makes sense, and verify your own work by actually running builds and
confirming they work — don't just assert that something should work.

## Stop condition

Stop when a validated 2-player local build exists and passes on BOTH the
WebGL and Desktop build targets. Do not continue past that point. If you
finish early, stop — do not use spare time to start on anything in the
"Out of scope" list below.

## Budget guardrail

If any single acceptance criterion isn't resolved after a reasonable
number of attempts (roughly 3-4 distinct approaches), stop iterating on
it. Report it as a blocker — what you tried, what failed, and your best
diagnosis — rather than continuing to grind on it. A clearly reported
blocker is a better outcome than an open-ended debugging loop. This
applies per-item, not to the task as a whole: keep working on the other
acceptance criteria even if one is blocked.

## What "done" looks like (acceptance criteria)

1. **Unity project + folder scaffold** — Project created via CLI (batch
   mode). Folder structure in place: Assets/Scenes, Assets/Scripts
   (Characters, Combat, Input, UI, Netcode), Assets/Prefabs, Assets/Art,
   Assets/Audio, Assets/Editor, Builds/WebGL, Builds/Desktop, Backend/
   (placeholder only — do not populate).

2. **Input System package** — Added via Packages/manifest.json. Resolve
   any import errors Unity throws. Confirm it imported cleanly (no console
   errors on project open).

3. **"Console mode" Player Settings, applied and verified** — Fullscreen
   Window or Exclusive Fullscreen, resolution locked (1920x1080 default),
   no resolution dialog on launch. Set this via a C# Editor script run
   with `-executeMethod` in batch mode — don't hand-edit the settings
   asset directly. Then actually produce a build and confirm the settings
   took effect (the built .exe should launch straight into fullscreen).

4. **Two build profiles, each validated with a real build** — WebGL and
   Windows Standalone. For each: configure the profile, run the build,
   confirm it completes without errors, and confirm the output actually
   launches/runs. A build that compiles but doesn't run doesn't count as
   done. Expose each as a callable batch-mode entry point: a static class
   `BuildScript` in `Assets/Editor/` with public static methods
   `BuildWebGL()` and `BuildDesktop()`, each runnable via
   `-executeMethod BuildScript.BuildWebGL` (or `BuildDesktop`). This is the
   contract the external build manager script calls — the method names
   must match exactly.

5. **2-player local input** — `PlayerInputManager` set up for 1-2 players,
   shared screen (no split-screen), join-in-progress on gamepad button
   press. Test criterion: two gamepads connected, both can join and each
   controls a distinct character. This is local-only — no networking.

6. **Git + Git LFS, and the per-person template** — `git init`, `.gitignore`
   for Unity (Library/, Temp/, etc. excluded), `.gitattributes` with LFS
   tracking for binary asset types (png, psd, wav, mp3, fbx, blend, anim,
   controller, etc.). Confirm LFS is actually tracking those types, not
   just configured. Structure this so it works as a clean template another
   person could clone and get an identical, working starting point from.

7. **Workflow documentation** — A short PROJECT_NOTES.md covering: the two
   build targets and what each is for, the browser-to-desktop development
   flow, and a note that AI-assisted tools (Cursor, Claude Code) can be
   toggled off for hand-coding — same project, same files, just without
   the assistance active.

## Out of scope — do not build any of this

- AWS/S3/CloudFront configuration of any kind (handled by a separate,
  already-written script — not your job)
- Any code review or self-critique pass beyond your own build verification
  (a separate review pass happens after you're done)
- Rollback netcode, online multiplayer, or any networking beyond local
  input (that's a future gate, contingent on this one shipping first)
- Split-screen, more than 2 players, or any multiplayer mode beyond local
  shared-screen 2-player
- Full character roster, stages, VFX/SFX, or any content beyond what's
  needed to test the systems above (a placeholder character/box is fine)
- Cloud streaming, HA infrastructure, matchmaking, or backend services of
  any kind — the Backend/ folder stays an empty placeholder
- Anything not explicitly listed in the acceptance criteria above, even if
  it seems like an obvious or small addition

If you find yourself about to build something not on the acceptance
criteria list, stop and flag it instead — don't build it.

## Environment notes

- Target OS: Windows. Any scripts you write outside of C#/Unity should be
  PowerShell.
- Unity version: pin to a specific LTS release number rather than "latest"
  — write down which version you used, so this is reproducible later.
- This is Gate 0 of a staged plan. Everything past Gate 1 (frame-perfect
  combat systems) is intentionally not your concern.
