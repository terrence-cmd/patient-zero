# Overview — The Complexity Roadmap

## Why this exists

Early on, it became clear this project could easily accumulate scope —
engine choice, deployment, multiplayer, online play, infrastructure — all
worth discussing, but not all worth building right now. So the project is
staged into gates. Each gate only unlocks once the previous one has
actually shipped something real and playable, not just been discussed.

## The gates

| Gate | What it covers | Status |
|---|---|---|
| **Gate 0** | Environment: Unity project, Git, Input System, "console mode" build settings, two build profiles (WebGL + Desktop) | **Current — in progress** |
| **Gate 1** | Core prototype: one character, frame data, hitstun, combos, local 2-player. Desktop-only test target. | Not started |
| **Gate 2** | Content scale: full roster, stages, polish. Still entirely local — no backend. | Not started |
| **Gate 3** | Online netcode: rollback netcode, first real AWS backend (matchmaking/relay, not GPU streaming) | Not started |
| **Gate 4** | HA infrastructure: auto-scaling, multi-AZ, ranked ladders — only once Gate 3's online play justifies it | Not started, explicitly out of scope for now |

## The standing rule across all gates

**Don't design Gate N+1 until Gate N has shipped** — not planned, not
scoped, shipped. Deciding a future gate's architecture now is complexity
borrowed from a future that might look different once earlier gates teach
us what the game actually wants to be.

## Why a fighting game specifically

Fighting games are an unusually good stress test for this whole
environment, because they're uniquely punishing about frame timing —
more than almost any other genre. That sensitivity is what actually
determines where tools like WebGL or cloud streaming stop being viable
(see `02-deployment-strategy.md`), rather than it being a guess.
