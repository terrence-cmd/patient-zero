# Game Files: Long-Term Storage

Three different things, easy to mix up, each covered by a different doc:

- **Working** on a game — editing live in the Unity Editor (this doc
  doesn't cover this — see `BUILD_AND_DEPLOY.md`)
- **Playing** a game — running a build, a `.exe` or a browser URL (also
  `BUILD_AND_DEPLOY.md`)
- **A Game File** — a saved, archived snapshot of a project you can put
  away and come back to later and keep editing. This is what this
  document is about, and right now, nothing else in this project does
  this for you automatically.

**A Game File is not a build.** A build is for playing, and you can't
resume editing from one. A Game File is the opposite — it's for coming
back to your actual project later, not for playing right now.

## Why this is manual on purpose

Git already exists in this repo and does something like this — but
automatically, in small increments, in a way that isn't very tangible if
you're new to it. This document is the hands-on version: a Game File is
something you make yourself, name yourself, and file away yourself.
Deciding *when* something's worth saving and *where* it lives is the
actual skill here, not something to skip past. It's supposed to feel a
little like real work — that's the point, not a flaw.

## What goes in a Game File (and what doesn't)

Short answer: **everything in the project folder except what
`.gitignore` already excludes.** That file exists for exactly this
reason — it's the list of what's regenerated automatically and never
needs to be saved. Specifically, leave these out:

- `Library/`, `Temp/`, `Obj/`, `Logs/` — Unity rebuilds all of these
  automatically the next time the project opens. They're also often
  huge, and including them just bloats your archive for no reason.
- `Builds/` — that's play output, not something you'd resume editing
  from. Not part of a Game File.
- IDE clutter (`.vs/`, `.idea/`, `.vscode/`) — regenerates on its own.

What you actually want: `Assets/`, `ProjectSettings/`, `Packages/`, and
any top-level docs specific to your work. That's the real, editable
state of your game.

## Naming your Game File

Use this pattern: `<GameName>_<YYYY-MM-DD>_<short description>.zip`

Example: `PatientZero_2026-08-10_first-working-combo.zip`

Two things matter here on purpose:
- **Date first, in YYYY-MM-DD order.** File browsers sort alphabetically
  — if the date's written this way, alphabetical order and chronological
  order are the same thing, so your Game Files naturally sort oldest to
  newest without you doing anything extra. (Month/day-first dates don't
  sort correctly this way — this is why YYYY-MM-DD specifically.)
- **A real, short description**, not "final" or "final2" or "backup" —
  something that tells you what's actually different about this one when
  you're looking at a list of ten of them six months from now.

## Where to keep them

**Not inside the live project folder.** Keep a separate folder just for
this — e.g. `Documents\GameDev\Game Files\` sitting next to (not inside)
`Documents\GameDev\patient-zero\`. Keeping it physically separate means
you never confuse "my saved snapshot" with "the live thing I'm actively
editing," and it can never accidentally get swept into a git commit by
mistake either.

## When to make one

There's no fixed schedule — this is a judgment call you make, and making
it well is the actual point. Reasonable moments to save one:
- Before trying something you're not sure will work
- When you've reached something you'd be upset to lose
- Before taking a real break from the project
- Just... periodically, if nothing else prompts it

If you're not sure whether something counts as "worth saving," that
uncertainty is normal — err toward saving it. A Game File is cheap to
make and costs you nothing to have extras of.

## How to make one

1. Close the Unity Editor first (same reason as in `BUILD_AND_DEPLOY.md`
   — avoids any file-lock weirdness while archiving).
2. Open your project folder in File Explorer.
3. Select `Assets`, `ProjectSettings`, and `Packages` (hold Ctrl and
   click each one to select all three at once).
4. Right-click → **Send to → Compressed (zipped) folder**.
5. Rename the resulting `.zip` using the naming pattern above.
6. Move it into your separate Game Files folder.

## How to resume from one

1. Unzip it into a new, empty folder — anywhere, doesn't need to be
   inside the live project location.
2. Open **Unity Hub → Add → Add project from disk**, and select that
   unzipped folder.
3. Open it. The first open will take longer than normal — Unity is
   rebuilding `Library/` from scratch, since that wasn't in the archive.
   This is normal and expected, not an error.
4. From here it behaves like any other project — keep editing, or make
   this the active one going forward.

## Where this leads

Once making these by hand feels tedious — that's exactly the moment git
starts making sense. Git does the same fundamental thing (a saved,
returnable snapshot of your project) but automatically, far more
granularly, and in a way that's shareable with other people. This
document isn't trying to replace that — it's the on-ramp to
understanding *why* you'd want it before jumping straight into learning
git itself.
