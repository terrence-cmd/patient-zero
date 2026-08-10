# Decision Log

Chronological record of every real decision made building this
environment. Ordered as they happened, not by topic — see the other docs
for the topic-organized version.

1. **Goal set:** an environment to write, test, and deploy video games —
   personal learning, tied to the Unity course as its atomic component.
2. **Engine:** Unity, matching the course.
3. **"Console" clarified:** simulated on PC (fullscreen, gamepad, TV-style
   UI), not real hardware — after directly checking which was meant.
4. **Environment scaffolded (first pass):** Unity Hub/Editor, code editor,
   Git, Input System, console-mode Player Settings, TV-style controller
   nav, one-click Build & Run — laid out as a manual checklist.
5. **Multiplayer scope set:** 1-2 local players, shared screen — chosen
   over split-screen specifically because split-screen was a real
   complexity jump and the lesser option was preferred by explicit rule.
6. **Deploy target clarified:** "deploy" meant local-only at first: then
   expanded to "run from AWS if feasible, desktop as fallback" once asked
   directly what deploy should cover.
7. **AWS approach chosen:** WebGL on S3 + CloudFront (near-$0/month) over
   GPU cloud streaming ($150-$2,000+/month) — explicit "free first,
   expensive later" sequencing.
8. **Cost/performance trade-off mapped:** WebGL vs. desktop vs. cloud
   streaming compared directly on cost, performance ceiling, and setup
   complexity.
9. **Fighter-game dev-cycle walkthrough:** mapped where WebGL, desktop,
   and AWS each become obsolete across a fighting game's actual dev
   lifecycle — this is where it became clear GPU streaming is wrong for
   fighters specifically (latency), not just expensive.
10. **Two parallel dev environments decided:** cheap/free browser (WebGL)
    for early ideas, desktop for continued dev — with a workflow mapping
    Claude in Chrome + Cursor to the browser side, Claude Code + Cursor to
    desktop.
11. **HA infrastructure surfaced, then explicitly deferred:** clarified as
    "high-availability stack," confirmed as a future dependency (Gate 4),
    not current-scope work.
12. **Gate structure formalized:** Gate 0 through Gate 4, with the rule
    "don't design Gate N+1 until Gate N has shipped."
13. **Hand-coding requirement added:** confirmed already supported by the
    existing tooling (VS Code/Rider underneath Cursor/Claude Code) — no
    new environment needed, just a toggle.
14. **First scaffold scripts written:** `1-scaffold-project.ps1` and
    `2-init-git.ps1` — the manual-checklist version of Gate 0, before the
    Fable 5 approach existed.
15. **Fable 5 split proposed:** work divided between what Fable 5 handles
    best (long-horizon, exploratory, self-verifying) and what it doesn't
    (tight, precise, fast iteration) — based on researching Fable 5's
    actual documented strengths/weaknesses.
16. **5-parallel-instance AWS cost question:** confirmed hosting 5 WebGL
    sites costs about the same as 1, since CloudFront/S3 free tiers are
    account-wide pools, not per-site.
17. **IAM misconception corrected:** confirmed IAM users are for managing
    AWS access, not for players — simultaneous players need zero AWS
    account setup.
18. **Parallel-dev-environment question, round 2:** clarified "dev in the
    cloud and browser" meant simultaneous work, not necessarily
    browser-based coding — and that a true browser-based Unity Editor
    would reintroduce the same expensive streaming cost already rejected.
    Confirmed everyone has their own PC, settled on local-per-person dev.
19. **3-piece build structure locked:** Fable 5 build / AWS config script
    / Cursor review pass, validated as reasonable given Fable 5's
    strengths and weaknesses.
20. **Scope-discipline rule declared explicitly:** no scope creep, under
    any circumstances — applied retroactively and going forward to every
    deliverable.
21. **Fable 5 list itemized:** the 7 specific things in Fable 5's build,
    each with an explicit rationale tied to its strengths; explicitly
    excluding AWS config and the review pass.
22. **Piece 3 assigned to Cursor:** confirmed as the right fit given
    Fable 5's own weak spot on review precision.
23. **AWS script written:** `provision-aws-target.ps1` — lean, idempotent,
    scoped to exactly 5 things (bucket, OAC, distribution, bucket policy,
    deploy-only IAM policy), explicitly excluding WAF/custom domains/
    Route 53/flat-rate plans/HA territory.
24. **Fable 5 brief written:** [fable5-task-brief-gate0.md](../fable5-task-brief-gate0.md) — a
    destination-plus-boundaries brief (not a checklist), with the 7
    acceptance criteria and an explicit out-of-scope list.
25. **Fable 5 brief optimized:** added the budget guardrail (stop after
    3-4 attempts on any single blocked item) — the one gap identified
    against Fable 5's documented tendency to run until cut off.
26. **Cross-check performed:** Fable 5 brief vs. AWS script compared
    directly for consistency. Found: scope boundaries matched, WebGL
    output format matched CloudFront's config, but two real gaps existed
    — no deploy/sync step, and no shared naming convention.
27. **Build manager written:** `build-manager.ps1` — closes the deploy gap
    by chaining Unity batch build → S3 sync → CloudFront invalidation,
    using the same naming convention as the AWS script (looked up
    dynamically, not duplicated).
28. **Build-entry-point contract added:** the Fable 5 brief was updated to
    specify the exact `BuildScript.BuildWebGL()`/`BuildDesktop()` method
    names the build manager depends on — closing the naming-convention gap
    from item 26.
29. **Auto-chaining question raised and deferred:** asked whether
    provisioning should auto-trigger from the build manager. Decided
    against it — Gate 0 needs to actually run once for real before that
    connection gets automated, to avoid baking in untested assumptions.
30. **Legacy-script overlap identified:** `1-scaffold-project.ps1` and
    `2-init-git.ps1` were found to duplicate what the Fable 5 brief now
    covers itself. Decided to keep them as a manual fallback rather than
    delete them, given the budget guardrail creates a real scenario where
    they'd be useful.
31. **This catalog created:** everything above written up into this repo,
    as the "patient zero" instance this project grows from.
