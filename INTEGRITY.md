# Integrity

## The actual goal of this environment

Everything in this repo — the AI-assisted tools, the "hand-coding stays
supported" toggle from `docs/01-environment-setup.md`, all of it — exists
to build one thing: your actual, real skill. Not a working game with your
name on it. Not a repo that looks impressive. Your own growing ability to
build things. A finished game that you didn't really build is worth
nothing next to that.

Concretely, that's not one skill — it's a specific list, and this
environment is built to teach all of them:
- **Reading and understanding code you didn't write** — including
  AI-generated code — well enough to explain it and change it correctly,
  not just trust that it runs. This is the one everything else depends
  on.
- **Input and device handling** — how a physical controller becomes a
  specific, distinct in-game player. One real example already in this
  repo: `TwoPlayerJoinController.cs`.
- **The build/deploy pipeline** — understanding how source code actually
  becomes a real, running program instead of treating that step as a
  black box. See `BUILD_AND_DEPLOY.md`.
- **Debugging discipline** — verifying a claimed result instead of
  trusting it, forming a real hypothesis about *why* something broke.
  Every failure mode in `UNITY_INSTALL.md` is a real example of this.
- **Scoping discipline** — knowing what *not* to build right now, and
  treating that restraint as a real skill, not just a rule someone
  handed you. Every doc in this repo states its own out-of-scope list on
  purpose.

This environment is deliberately both **assisted and unassisted** —
Cursor and AI help are genuinely available, and so is doing it entirely
by hand. Both are legitimate. But they are not the same thing, and
pretending they are is where this whole idea breaks.

## AI is not a magic box — it's a skill amplifier

That's the whole model to hold in your head. A magic box takes nothing
in and produces something finished — you'd have no real relationship to
the output at all. An amplifier is different: it takes what's *already
there* and makes it bigger, faster, more capable. Take the Editor-lock
issue documented in `BUILD_AND_DEPLOY.md` — if you already understand
*why* a running Unity Editor can conflict with a batch-mode build, AI can
help you script around it faster, spot a related edge case, suggest a
cleaner check — real understanding, amplified. If you don't understand
why that matters at all and just paste in whatever makes an error
message go away, there's nothing there for it to amplify. You get
working code and zero actual skill, because amplifying zero is still
zero.

This is exactly why `assisted-understood` and `assisted-learning` are
different things below, not the same box checked two ways.
`assisted-understood` is the amplifier doing its actual job — your
understanding, faster. `assisted-learning` is an honest admission that,
for this specific piece, right now, there wasn't anything for it to
amplify yet. That's fine. It's only a problem if it stays that way.

## The Magic Box Trap: why it's so easy to fall into

Synesthesia is when the brain blends two senses that are supposed to
stay separate — someone hears a sound and perceives a color along with
it, with no seam between the two. It's not a flaw exactly, just a wire
crossed somewhere it isn't in most people.

Something similar happens with AI if you're not paying attention to it
on purpose. A book has a hard boundary — the words on the page are
obviously not yours, and your summary of them is obviously a separate
thing. A back-and-forth conversation with AI doesn't have that seam. It
feels continuous with your own thinking, because it's shaped like
thinking — question, answer, next question — not shaped like "here is a
separate source." Use it enough without checking yourself, and an idea
the AI actually generated can start feeling like something you thought
of, with no seam telling you otherwise. That blending *is* The Magic Box
Trap, mechanically — it's not that you're being careless, it's that the
interface itself doesn't hand you a natural boundary the way a book
does. You have to draw the line on purpose, because the tool won't draw
it for you.

This isn't just an integrity problem — it actively makes the AI *less
useful* to you, which is the part that's easy to miss. Go back to the
amplifier model: an amplifier only does its job if you can tell signal
from what it's amplifying. If you've lost track of what's actually your
own understanding, you can't ask a precise follow-up question, you can't
direct it toward what you actually need next, and you can't catch it
when it's wrong — catching a mistake requires having independent
understanding to check it against. A blurred boundary doesn't just cost
you the credit. It degrades the entire collaboration, because you've
stopped steering it and started just riding along with it. Keeping the
line visible — which is the entire job of the `Authorship:` habit below
— is what keeps you in the driver's seat instead of a passenger who
happens to be holding the wheel.

## Why the difference has to stay visible

Skill grows from friction, not from a working result. If AI quietly does
the hard part and you never have to develop the underlying capability
yourself, you end up with a repo full of things that work and a skill
level that hasn't actually moved. That's not a hypothetical risk — it's
the single easiest way to end up all speed and no substance: a repo that
looks productive, but you still can't do the fundamental things
underneath it.

There's a second, quieter cost that matters just as much: if you can't
tell — or won't admit, even to yourself — how much of something was
really yours, you lose the ability to accurately judge your own growth.
You can't get better at something you don't know you're actually weak
at. Honest attribution isn't about anyone grading you. It's the only way
you get an accurate signal of where you really stand, so you know what
to actually go practice next. That signal compounds, too — an honest
`Authorship:` history over time is a real, visible growth curve: watch
the ratio of `assisted-learning` to `assisted-understood` shift on the
same kind of task, and that's not a guess about whether you're
improving, it's evidence.

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

Here's what it actually looks like in practice:
```
Add double-jump to PlayerMover

Authorship: assisted-learning
Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
```
That's the whole thing. Nobody has to explain it, justify it, or bring
it up out loud. It's a line in a commit message, not a conversation.

It's fair if this doesn't feel fully safe the first few times anyway,
even with all of the above said. Writing "I don't understand this yet"
down, somewhere someone else could read it, taps into something real —
nobody wants to look like they don't know what they're doing, and that
feeling doesn't just switch off because a document told it to. But look
at what `assisted-learning` actually costs you: nothing. It isn't
graded. It doesn't require a conversation you have to start. It's the
lowest-stakes way there is to leave an honest trail — including, if it
comes to it, a trail that shows me exactly where you could use a hand,
without you ever having to be the one to bring it up. That's not a side
effect. That's what it's for.

---

**Guided and inspired by Terrence. Written by Claude Sonnet 5.**
