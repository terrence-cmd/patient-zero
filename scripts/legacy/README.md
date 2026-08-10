# Legacy Scripts

`1-scaffold-project.ps1` and `2-init-git.ps1` were the first concrete Gate
0 deliverables — written when the plan was still "run this yourself as a
manual checklist," before the decision to hand Gate 0 to Fable 5 as an
autonomous build.

They do the same job as acceptance criteria #1 (folder scaffold) and #6
(Git + LFS) in `../../fable5-task-brief-gate0.md`. **They are not part of
the active build plan** — Fable 5's brief covers that ground now.

## Why they're still here

The Fable 5 brief includes a budget guardrail: if it gets stuck on any one
acceptance criterion after 3-4 attempts, it stops and reports a blocker
instead of grinding indefinitely. If that blocker happens to land on the
scaffold or Git step specifically, these two scripts are a ready-to-run
manual fallback — something to reach for immediately instead of writing a
fix from scratch under pressure.

If Gate 0 completes cleanly via Fable 5 without ever needing these, that's
fine — they just stay unused. Nothing about keeping them costs anything
going forward.
