# Environment Setup

## Engine: Unity

Chosen because it's already the engine of the AI-Assisted Unity Game
Development course being built as a separate but related project — this
environment is meant to be the atomic, hands-on component of that course.
Same tooling the eventual students will use.

## "Console mode" — simulated, not real hardware

The goal was to write, test, and play games "as if on a console device."
Two real options existed:

- **Real hardware** (Steam Deck, Raspberry Pi, an actual console dev kit)
- **Simulated on PC** — fullscreen, gamepad input, TV-style controller-only
  navigation

**Simulated on PC was chosen.** It gets the actual experience that
mattered (fullscreen, controller-driven, no mouse/keyboard needed) without
any hardware cost or acquisition step. Concretely, this means:

- Fullscreen Window or Exclusive Fullscreen player settings, resolution
  locked, no resolution dialog on launch
- Unity's Input System package for gamepad support (Xbox controllers work
  natively via XInput, no extra drivers)
- Menus navigable entirely by controller (UI Toolkit / uGUI + Input
  System's UI navigation actions) — the actual thing that sells the
  "console" feel, more than fullscreen alone

## Editor and version control

- **Code editor:** VS Code + C# Dev Kit, or JetBrains Rider — whichever's
  installed, Unity Hub points to it
- **Version control:** Git + GitHub, with **Git LFS** for binary assets
  (textures, models, audio) — plain Git chokes on large binaries over
  time; LFS is a one-time setup that avoids that entirely

## Hand-coding stays supported

AI-assisted tools (Cursor, Claude Code) sit on top of the same codebase —
they don't replace hand-coding, they add to it. Toggling their features
off (or just not invoking them) gives a normal manual-coding experience in
the same project, same files. No separate environment needed for this.
