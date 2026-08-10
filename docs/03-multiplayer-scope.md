# Multiplayer Scope

## The rule that decided this

Stated explicitly early on: if 2-player support adds substantially more
complexity than single-player, go with the lesser complexity. This same
"choose the lesser complexity" instinct ended up governing several later
decisions too (AWS deployment path, dev-environment shape) — it's one of
the recurring principles behind this whole environment, not a one-off.

## What was chosen: 1-2 local players, shared screen

Unity's Input System ships with `PlayerInputManager`, whose entire job is
this exact case: it listens for any connected gamepad to press a button,
auto-assigns that controller to a new player, and spawns them in. For two
people sharing one screen (not split-screen), this is roughly 10-15
minutes of setup — a small addition, not a new architecture.

## What was explicitly NOT chosen, and why

**True split-screen** (separate cameras/viewports, viewport-aware UI,
per-player camera-follow) would be a real complexity jump — separate
render logic, separate UI layout per player. Since shared-screen 1-2
players was confirmed as sufficient, split-screen was dropped entirely
rather than half-built.

**4-player / true 2v2** came up once as a casual phrase but was explicitly
confirmed back down to 2 players when checked directly — 2 remains the
locked scope.

## Where this connects to Gate 1

The 2-player local input wiring is part of Gate 1's acceptance criteria
(see [fable5-task-brief-gate0.md](../fable5-task-brief-gate0.md)) — join-in-progress via gamepad, two
distinct characters spawn, no split-screen, no networking. This is local
input only; anything about *online* multiplayer is Gate 3 territory, not
this.
