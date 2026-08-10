# Parallel Dev, and the Fable 5 Build Split

## Multiple people, working simultaneously

The requirement: several people should be able to use their own dev
environment at the same time, with no turn-taking. The resolution turned
out to be simpler than it first sounded: **parallelism isn't a feature to
build, it's just the natural result of each person having their own
separate copy of everything** — own PC, own repo clone, own editor. No
shared-resource coordination needed.

## Why "browser-based development" was rejected (distinct from browser-hosted games)

This took a real clarification. Earlier, "browser-based" had been decided
for where the *game* runs (see `02-deployment-strategy.md`). This is a
different question: where the *coding* happens.

The Unity **Editor** is a heavy native desktop application — there's no
lightweight "Unity Editor in a browser tab." The only way to get real
Editor access through a browser is to stream a full cloud desktop/VM to
that tab, which quietly reintroduces the same expensive GPU-streaming cost
tier already rejected in `02-deployment-strategy.md`. Once it was
confirmed everyone has their own capable PC, the answer became simple:
**local development, full Unity Editor, zero extra cost, inherently
parallel.**

## The Fable 5 build split

Research into Claude Fable 5's actual strengths and weaknesses shaped how
work gets divided:

- **Fable 5's strength:** long-horizon, autonomous, exploratory,
  self-verifying, multi-file work — it explores an unfamiliar environment,
  writes its own tests, and works for extended periods with minimal
  supervision.
- **Fable 5's documented weak spot:** it tends to keep working until
  something cuts it off (expensive without a hard stop condition), and for
  tight code-review precision it isn't yet a clear upgrade over other
  approaches.

This produced a 3-piece (later 4-piece) build structure:

| Piece | Owner | Why |
|---|---|---|
| 1 — Main environment build | Fable 5 | Long-horizon, exploratory, self-verifying — exactly its strength |
| 2 — AWS configuration | A tighter, faster, precision-focused approach | Well-specified infra work, not exploratory |
| 3 — Review/QA pass | Cursor, as a QA step | Needs to be independent of the builder to catch what self-verification misses |
| 4 — Build/deploy manager | Precision-focused, not Fable 5 | Orchestration/glue code, well-specified |

## The standing scope-discipline rule

Declared explicitly and applied to every piece since: **no scope creep,
under any circumstances.** Each deliverable in this repo states its own
"out of scope" list as deliberately as its "in scope" list — see
`fable5-task-brief-gate0.md` and the header comments in each script.

## The budget guardrail

Because Fable 5 tends to run until cut off, the task brief includes an
explicit guardrail: if any single acceptance criterion isn't resolved
after roughly 3-4 distinct attempts, stop and report it as a blocker
rather than continuing to grind. Applies per-item, not to the whole task.
