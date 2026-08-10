# Integrity

## The actual goal of this environment

Everything in this repo — the AI-assisted tools, the "hand-coding stays
supported" toggle from `docs/01-environment-setup.md`, all of it — exists
to build one thing: your actual, real skill. Not a working game with your
name on it. Not a repo that looks impressive. Your own growing ability to
build things. A finished game that you didn't really build is worth
nothing next to that.

This environment is deliberately both **assisted and unassisted** —
Cursor and AI help are genuinely available, and so is doing it entirely
by hand. Both are legitimate. But they are not the same thing, and
pretending they are is where this whole idea breaks.

## AI is not a magic box — it's a skill amplifier

That's the whole model to hold in your head. A magic box takes nothing
in and produces something finished — you'd have no real relationship to
the output at all. An amplifier is different: it takes what's *already
there* and makes it bigger, faster, more capable. If you already
understand what a join-in-progress system needs to do, AI can help you
build it faster, catch mistakes, show you approaches you hadn't thought
of — real understanding, amplified. If you don't understand it yet at
all and just accept whatever gets generated, there's nothing there for
it to amplify. You get working code and zero actual skill, because
amplifying zero is still zero.

This is exactly why `assisted-understood` and `assisted-learning` are
different things below, not the same box checked two ways.
`assisted-understood` is the amplifier doing its actual job — your
understanding, faster. `assisted-learning` is an honest admission that,
for this specific piece, right now, there wasn't anything for it to
amplify yet. That's fine. It's only a problem if it stays that way.

## Why the difference has to stay visible

Skill grows from friction, not from a working result. If AI quietly does
the hard part and you never have to develop the underlying capability
yourself, you end up with a repo full of things that work and a skill
level that hasn't actually moved. That's not a hypothetical risk — it's
the single easiest way to end up all speed and no substance, three
months in, still not actually able to do the fundamental things.

There's a second, quieter cost that matters just as much: if you can't
tell — or won't admit, even to yourself — how much of something was
really yours, you lose the ability to accurately judge your own growth.
You can't get better at something you don't know you're actually weak
at. Honest attribution isn't about anyone grading you. It's the only way
you get an accurate signal of where you really stand, so you know what
to actually go practice next.

This is the same idea from `UNITY_INSTALL.md`, applied one level deeper:
*I can't teach you to code. I can only teach you how to teach yourself,
and those are two very different things.* Attribution is how you keep
that distinction real instead of it quietly dissolving over time.

## How this actually works — attribution, not enforcement

When you commit code, you can add a line like this:

```
Authorship: self
```
or
```
Authorship: assisted-understood
```
or
```
Authorship: assisted-learning
```

Same idea as the `Co-Authored-By:` lines you'll already see throughout
this repo's own commit history — this repo has been doing exactly this
kind of attribution since before you got here.

**What each one actually means:**
- **`self`** — you wrote it. No AI involved.
- **`assisted-understood`** — AI wrote or helped write it, and you
  understand it well enough to explain how it works, out loud, without
  the AI's help.
- **`assisted-learning`** — AI wrote it, and you're not there yet on
  understanding it. This is a completely normal, expected, safe thing to
  write. It is not a confession.

**This is not enforced.** No hook blocks a commit without it. That's on
purpose — the same reason Game Files are a manual process instead of an
automated one in `GAME_FILE_STORAGE.md`. The discipline of doing this
honestly, on your own, without anything forcing you to, is a real part
of what's being built here. A rule you can't break isn't teaching you
anything. A habit you keep on your own is.

## The one thing that actually matters

`assisted-learning` has to stay a safe thing to write, every time, or
none of this works. The moment it starts feeling like an admission of
failure instead of an honest, normal snapshot of where you are right
now, people stop writing it — and then the whole system becomes
decoration instead of a real signal. If you're ever unsure whether
something counts as `assisted-understood` or `assisted-learning`, that
uncertainty itself is useful information. Write `assisted-learning` and
move on. That's the correct call every time you're not sure.

---

**Guided and inspired by Terrence. Written by Claude Sonnet 5.**
