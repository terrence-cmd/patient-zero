# Build Pipeline

## The four pieces, and what connects them

```
fable5-task-brief-gate0.md
        |
        v
   Fable 5 runs, produces the Unity project
   (including BuildScript.BuildWebGL() / BuildDesktop() —
   the exact entry-point contract the build manager calls)
        |
        v
   Cursor QA pass (checklist, not a script — see below)
        |
        v
   provision-aws-target.ps1 -PersonName "X"      (run once per person)
        |
        v
   build-manager.ps1 -PersonName "X" -Target WebGL   (run per build)
        |
        v
   Deployed to that person's CloudFront URL
```

## The interface contract between Fable 5 and the build manager

The build manager triggers builds via
`Unity.exe -executeMethod BuildScript.BuildWebGL` (or `BuildDesktop`).
This only works because the Fable 5 brief explicitly specifies that exact
static method contract in its acceptance criteria — the two pieces were
written independently but deliberately kept in sync on this interface.
This was checked directly, not assumed: see the comparison recorded in
[06-decision-log.md](06-decision-log.md).

## Piece 3 — why it's a process, not a file

The Cursor review pass is a QA step, not a script:

- **Input:** the built project directory, Fable 5's own self-test/
  validation results, and a fixed checklist (does it open, do both build
  profiles run, does fullscreen/gamepad mode work, do 2 controllers join
  correctly, is Git LFS actually tracking the right file types)
- **Output:** a pass/fail scorecard, concrete fixes applied directly as a
  diff (not suggestions), one confirmation build post-fix, and a single
  verdict line — "Gate 0 complete" or "Gate 0 blocked on: X"

## What's deliberately NOT automated yet

`provision-aws-target.ps1` and `build-manager.ps1` are **not** chained
together automatically. The build manager fails loudly and stops if a
person's AWS target doesn't exist yet, rather than trying to create it.

This was a deliberate choice, not an oversight: the provisioning → deploy
order is a design on paper until Gate 0 actually runs end-to-end for the
first time. Automating the connection before that happens risks baking in
wrong assumptions instead of catching them by hand on the first real run.
Once Gate 0 has actually shipped, chaining them (or not) becomes a much
lower-risk decision — this is effectively "Gate 0.5," deliberately parked
until Gate 0 ships.

## Legacy scripts

`scripts/legacy/1-scaffold-project.ps1` and `2-init-git.ps1` predate the
decision to hand Gate 0 to Fable 5. They do the same job as Fable 5's
acceptance criteria #1 and #6 (folder scaffold, Git+LFS setup). Rather
than delete them, they're kept as a manual fallback — if Fable 5 reports a
blocker specifically on the scaffold or Git step (per the budget
guardrail), these are ready to run by hand instead of writing something
from scratch under pressure. See [scripts/legacy/README.md](../scripts/legacy/README.md).
