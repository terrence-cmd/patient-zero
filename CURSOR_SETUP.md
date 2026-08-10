# Getting Cursor Up and Running

Cursor is optional, not required — per `docs/01-environment-setup.md`,
AI-assisted tools sit on top of the same codebase and can be toggled off
for normal hand-coding at any time, same project, same files. This
document is for when you *do* want it, specifically for the QA pass
defined in `cursor-qa-brief-gate0.md`.

## Two separate accounts, don't mix them up

- **A Cursor account** — required to use any of Cursor's AI features at
  all (Chat, Tab completion, Agent). Free or paid, doesn't matter, you
  need one either way.
- **A GitHub account** — only needed if you want to *push* changes back
  to the repo. Since `patient-zero` is public, **cloning and reading it
  requires no GitHub account at all** — anyone can pull it down with
  plain `git clone`.

These are two unrelated sign-ins. Having one doesn't give you the other.

## Install

1. Download the installer from cursor.com (or use `CursorUserSetup-x64-*.exe`
   if it's already been downloaded).
2. Run it. You'll get a Windows permission prompt (UAC) — allow it.
3. Follow the setup wizard, default options are fine.
4. First launch will prompt you to sign in — GitHub, Google, or email all
   work. This is the Cursor account from above, not a GitHub-specific
   requirement.

## Opening this project

1. In Cursor, use **Clone Repository** (or open a terminal and run
   `git clone https://github.com/terrence-cmd/patient-zero.git`).
2. Open the cloned folder in Cursor.
3. Read `cursor-qa-brief-gate0.md` first, in full — that's the actual
   task specification, not this document. This doc is just about getting
   Cursor itself working; the brief defines what to actually do once it
   is.

## Free vs. Pro vs. beyond — what you actually get

Cursor's exact numeric limits shift over time and by account/promotion —
the pricing page itself doesn't commit to one fixed number anymore. Check
the live usage view inside Cursor (or your account dashboard) for your
actual current numbers. The figures below are a reasonable, commonly-cited
sense of scale, not a guarantee.

### Free ("Hobby") — $0, no credit card required

- Roughly **2,000 Tab completions** and **50 "slow" premium AI requests**
  per month, as a rough order of magnitude — not an exact promise.
- New accounts typically get a 1-week Pro trial on top of this.
- **Realistic expectation: this is for evaluation, not for getting
  through a real task in one sitting.** It's genuinely easy to run out
  mid-session on anything nontrivial — like a car running out of gas on
  the highway on-ramp. For light poking around or short, focused edits,
  it's fine. For actually working through the full QA brief in one go,
  expect to hit the wall partway through.
- **Hand-coding still works with zero limits, always**, free tier or no
  account at all — the AI features are what's capped, not the editor
  itself. If the free tier runs out mid-task, falling back to writing
  the rest by hand is a completely normal way to finish, not a failure
  state.

### Pro — $20/month ($16/month billed annually)

- Extended agent requests, access to frontier models, a $20/month usage
  credit pool.
- Using Cursor's **Auto** mode (it picks the model for you) draws from
  this pool much more efficiently than manually picking a specific
  premium model — Auto is effectively unlimited for most normal use;
  manually forcing a specific model burns the $20 pool faster.
- Rough real-world coverage on that $20, based on typical usage: **around
  225 Claude Sonnet requests, ~550 Gemini requests, or ~650 GPT-4.1
  requests per month** — these numbers move depending on which model and
  how complex each request is, so treat them as a ballpark, not a
  contract.
- This is the realistic tier for someone actually working through tasks
  regularly, not just evaluating.

### Beyond Pro (know these exist, probably not relevant here)

- **Pro+** — $60/month, roughly 3x Pro's usage pool.
- **Ultra** — $200/month, roughly 20x usage plus priority access to new
  features.
- **Teams / Enterprise** — per-seat or custom pricing, meant for
  organizations, not individual kids working on their own machine.

Annual billing knocks 20% off any paid tier if committing for a year
makes sense.

## Practical recommendation

Start on the free tier — there's no reason not to, it costs nothing to
try. If the QA brief (or any real task) keeps getting cut off mid-way by
hitting the limit, that's the actual signal Pro is worth it, not a
guess in advance. And regardless of tier, remember hand-coding never
goes away — Cursor without any AI credits left is just a very capable
text editor at that point, still fully usable.

---

**Guided and inspired by Terrence. Written by Claude Sonnet 5.**
