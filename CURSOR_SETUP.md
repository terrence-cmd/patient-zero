# Getting Cursor Up and Running

Cursor is optional, not required — per [docs/01-environment-setup.md](docs/01-environment-setup.md),
AI-assisted tools sit on top of the same codebase and can be toggled off
for normal hand-coding at any time, same project, same files. This
document is for when you *do* want it, specifically for the QA pass
defined in [cursor-qa-brief-gate0.md](cursor-qa-brief-gate0.md).

**Read [INTEGRITY.md](INTEGRITY.md) before this, not after.** This document hands you
the actual tool that makes The Magic Box Trap possible in the first
place. Installing Cursor without already knowing what that trap is and
how to avoid it is exactly backwards — like being handed a lever before
anyone told you what it's for.

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

Worth knowing, in the interest of the same honesty this repo tries to
practice everywhere else: unlike [UNITY_INSTALL.md](UNITY_INSTALL.md), these steps aren't
distilled from a real install session that hit actual failure modes —
Unity's install genuinely fought back and every failure documented there
really happened. Cursor's install is normally far less eventful (it's a
much simpler install than a full game engine), and these steps reflect
that expectation, not a battle-tested account. If something here turns
out to be wrong, that's worth fixing the document over, not just
working around quietly.

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
3. Nothing else to do here — `.cursor/rules/patient-zero-qa.mdc` loads
   automatically on every request in this repo. It points Cursor at
   [INTEGRITY.md](INTEGRITY.md) and [cursor-qa-brief-gate0.md](cursor-qa-brief-gate0.md) and states the standing
   rules (no scope creep, verify claimed successes, the two different
   commit-attribution systems) without you having to paste anything in
   manually or remember to bring it up.
4. Still worth doing yourself, though: actually read [INTEGRITY.md](INTEGRITY.md) and
   [cursor-qa-brief-gate0.md](cursor-qa-brief-gate0.md) in full, not just letting Cursor read them.
   Start using the `Authorship:` trailer from your very first commit in
   this project, not once you feel ready for it — there's no version of
   "ready" that comes before actually doing it.

## Free vs. Pro vs. beyond — what you actually get

There's a practical reason to care about The Magic Box Trap beyond the
pedagogy in [INTEGRITY.md](INTEGRITY.md): **it also burns through your limited
requests faster.** Someone who understands what they actually need asks
one precise question and gets one useful answer. Someone who's fallen
into treating Cursor as a magic box tends to flail — vague prompts,
re-asking the same thing worded differently, accepting an answer that
doesn't quite work and asking again instead of understanding why it
didn't. On a free tier with a real ceiling, that difference is the
difference between the QA brief fitting inside your monthly limit or
not. Avoiding the trap isn't just the honest way to work — on Free
specifically, it's also the efficient one.

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
guess in advance.

Hitting that wall mid-task can feel like being stuck, or like something
went wrong on your end. It isn't. It's just a free tier doing exactly
what free tiers are for — a real, useful preview, not a promise that it
covers everything. And regardless of tier, hand-coding never goes away
— Cursor without any AI credits left is still a fully capable text
editor, and finishing the rest by hand from there isn't a downgrade or a
failure state. Some of your best `assisted-understood` moments will
probably come from exactly this: the AI got you partway, the free tier
ran out, and you finished it yourself because you actually understood
enough to.

---

**Guided and inspired by Terrence. Written by Claude Sonnet 5.**
