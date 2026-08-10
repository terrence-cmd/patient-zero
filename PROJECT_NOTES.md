# Project Notes — Patient Zero (Gate 0)

Unity version: **6000.3.21f1** (LTS, pinned — see `ProjectSettings/ProjectVersion.txt`).

## The two build targets

| Target | Output | What it's for |
|---|---|---|
| **WebGL** | `Builds/WebGL/` (open `index.html` via a local server, or deploy via `scripts/build-manager.ps1`) | Early prototyping and easy sharing — anyone with the link can play in a browser. Not frame-accurate enough for real fighting-game timing. |
| **Desktop** (Windows Standalone) | `Builds/Desktop/PatientZero.exe` | The real dev target. Launches straight into fullscreen 1920x1080 "console mode" — no resolution dialog, no window resize, controller-driven. Frame-perfect timing work (Gate 1+) happens here. |

Both are built from the same project and scene via batch mode:

```
Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWebGL
Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildDesktop
```

(`scripts/build-manager.ps1` wraps exactly these calls.)

## Browser-to-desktop development flow

1. Iterate on a feature; push a **WebGL** build for quick sharing/feedback —
   cheapest way for others to see and react to a change.
2. Anything timing-sensitive (input latency, frame data, hitstun — i.e. the
   actual fighting game) gets validated on the **Desktop** build, which is
   the target that ships. WebGL is a window into the project, not the product.
3. Local 2-player: connect two gamepads, launch the desktop build, press any
   button on each pad to join. P1 spawns red (left), P2 blue (right). Join is
   capped at 2 — no split-screen, shared camera.

## AI-assisted tools are optional

Cursor / Claude Code sit on top of this same project — same files, same
repo. Toggling them off (or simply not invoking them) gives a normal
hand-coding experience in any editor (VS Code, Rider). Nothing in this
project depends on AI assistance being active.

## Template usage (per person)

Clone the repo, open in Unity 6000.3.21f1, and you have an identical
working start: `git clone <repo>` (Git LFS required — `git lfs install`
once per machine). `Library/` regenerates on first open; first open takes
a few minutes.
