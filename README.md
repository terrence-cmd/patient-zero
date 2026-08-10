# Patient Zero

This is the origin instance of what's meant to grow into a bigger console
game-dev project. Everything here was planned and built in one continuous
working session — this README is the map of it. Start here.

## What this actually is

A personal environment for writing, testing, and deploying video games,
built around one first target: a sub-professional 2D fighting game, aimed
eventually at Mortal-Kombat-caliber depth. It's designed to scale from
"one person, one PC" to "a handful of people, each on their own PC,
working in parallel" without re-architecting anything.

## Your space

[`NOTES.md`](NOTES.md) — reserved for you. Nothing else writes to this
file. Open threads, half-formed ideas, questions for next time — whatever
doesn't belong in the settled record below.

## Where to start reading

| Doc | What's in it |
|---|---|
| [`docs/00-overview.md`](docs/00-overview.md) | The complexity roadmap (Gate 0 through Gate 4) and where this project currently stands |
| [`docs/01-environment-setup.md`](docs/01-environment-setup.md) | Engine choice, "console mode" simulation, why real hardware wasn't the answer |
| [`docs/02-deployment-strategy.md`](docs/02-deployment-strategy.md) | The AWS cost/performance analysis — WebGL vs. desktop vs. cloud streaming, and why |
| [`docs/03-multiplayer-scope.md`](docs/03-multiplayer-scope.md) | Why 2 local players, and the complexity-gating principle behind that call |
| [`docs/04-parallel-dev-and-fable5.md`](docs/04-parallel-dev-and-fable5.md) | Multi-person parallel dev, the Fable 5 build split, and the scope-discipline rule |
| [`docs/05-build-pipeline.md`](docs/05-build-pipeline.md) | The 4 build pieces, how they chain, and what's deliberately NOT automated yet |
| [`docs/06-decision-log.md`](docs/06-decision-log.md) | The full chronological record — every real decision, in order |

## Where the actual deliverables are

| File | Purpose |
|---|---|
| [`SESSION_HANDOFF.md`](SESSION_HANDOFF.md) | Authoritative "where things actually stand right now" — read this first if picking the project back up, it wins over anything else if they conflict |
| [`INTEGRITY.md`](INTEGRITY.md) | Read this one first. Why assisted vs. unassisted has to stay honest and visible, and how to self-attribute commits — not enforced, on purpose |
| [`UNITY_INSTALL.md`](UNITY_INSTALL.md) | The reliable Unity install procedure — this was the single most failure-prone step of the whole setup, and this doc is every real failure mode hit, with fixes |
| [`fable5-task-brief-gate0.md`](fable5-task-brief-gate0.md) | The task brief Fable 5 builds from |
| [`cursor-qa-brief-gate0.md`](cursor-qa-brief-gate0.md) | The task brief Cursor's independent QA pass works from (Piece 3 of the build — see `docs/05-build-pipeline.md`) |
| [`scripts/provision-aws-target.ps1`](scripts/provision-aws-target.ps1) | Provisions one person's AWS hosting target (run once per person) |
| [`scripts/build-manager.ps1`](scripts/build-manager.ps1) | Builds + deploys a WebGL/Desktop build (run every time there's something new to ship) |
| [`scripts/legacy/`](scripts/legacy/) | Pre-Fable-5-decision scaffold scripts, kept as a manual fallback — see the README inside |
| [`PROVISIONED_TARGETS.md`](PROVISIONED_TARGETS.md) | Who has a live AWS hosting target already, so it's clear what exists before provisioning another |
| [`CONTROLLER_SETUP.md`](CONTROLLER_SETUP.md) | Kid-facing walkthrough: wired vs. wireless controller setup, what to do on a desktop tower with no built-in Bluetooth, and troubleshooting |
| [`BUILD_AND_DEPLOY.md`](BUILD_AND_DEPLOY.md) | The critical path from editing a change to it being playable — build-manager.ps1's exact steps, why closing the Editor first matters, Desktop vs. WebGL |
| [`CURSOR_SETUP.md`](CURSOR_SETUP.md) | Installing Cursor, opening this repo in it, and what Free vs. Pro actually gets you |
| [`.cursor/rules/patient-zero-qa.mdc`](.cursor/rules/patient-zero-qa.mdc) | Loads automatically in Cursor on every request in this repo — points it at INTEGRITY.md/the QA brief and states the standing rules, so nobody has to paste a kickoff prompt by hand |
| [`GAME_FILE_STORAGE.md`](GAME_FILE_STORAGE.md) | What a "Game File" is (a resumable saved snapshot, distinct from working and playing), naming/storage convention, and how it relates to git |

## Known gaps — tracked, not yet fixed

Small, real refinements identified but deliberately deferred rather than
done in the moment. Fix these down the road, not urgent:

- **`CONTROLLER_SETUP.md`'s cable-testing section is more cautious than
  it needs to be for one common case.** A genuine Microsoft Xbox Series
  controller's own in-box USB-C cable is reliably a real data cable —
  wired play is an advertised feature, so it has to be. The doc's
  Device-Manager test is still correct as a general fallback, but
  doesn't yet carve out "genuine Xbox Series controller + its own
  original cable = skip the test" as an explicit exception. Doesn't
  apply the same way to older Micro-USB Xbox One controllers (didn't
  always ship with a cable at all) or third-party "Xbox-compatible"
  controllers (no guarantee on their included cable).
- **A real, verified finding from live hardware testing isn't in
  `CONTROLLER_SETUP.md` yet**: Xbox Series controllers connected via
  Bluetooth may not register in Unity's Input System at all (shows as
  unsupported generic HID) even though Windows shows them fully
  connected — wired is the confirmed fix. Full detail and sourcing in
  [`SESSION_HANDOFF.md`](SESSION_HANDOFF.md).

## The standing rule

No scope creep. Every doc and script above states its own boundaries
explicitly — what's in, and just as important, what's deliberately left
out. If something isn't written down as in-scope somewhere in this repo,
it doesn't get built without a new, explicit decision first.
