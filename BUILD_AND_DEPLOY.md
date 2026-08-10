# Editing → Building → Playing

This is the process that turns changes you made while *working* on the
game into something you (or anyone else) can actually *play*. Quick
reminder of the distinction, since it matters here: working happens in
the Unity Editor, playing happens from a build — a `.exe` or a browser
URL. There is no emulator anywhere in this — a build is a real compiled
program, not something interpreted or simulated at runtime.

## Before any of this works (one-time setup, not part of the repeatable path)

- Unity Editor 6000.3.21f1 (the pinned version) installed, with WebGL and
  Windows Build Support (IL2CPP) modules — see the main project setup.
- This repo cloned locally.
- **WebGL only:** your AWS target already provisioned once via
  `scripts/provision-aws-target.ps1 -PersonName "<you>"`. Desktop doesn't
  need this at all — it never leaves your machine.

Everything below assumes these are already done. If they're not, do them
once, first — they're not part of the loop you repeat every time you want
to test a change.

## The critical path

This is the exact, required order. Skipping or reordering a step is the
most common source of confusing failures.

### 1. Make your changes in the Unity Editor

Normal editing — scripts, scenes, prefabs, whatever. This is the "working"
state and it's not saved anywhere durable yet in the sense of being
playable outside the Editor.

### 2. Close the Unity Editor

**This step is easy to skip and it's the #1 thing that breaks the rest of
this process.** The Editor holds a lock on the project's `Library/`
folder while it's open. The build step (next) needs that same folder.
Running a build while the Editor is still open risks a lock conflict or
a confusing failure that has nothing to do with your actual code.

Close it fully before continuing.

### 3. Run the build

```
.\scripts\build-manager.ps1 -PersonName "<you>" -Target Desktop
```
or
```
.\scripts\build-manager.ps1 -PersonName "<you>" -Target WebGL
```

Under the hood, this launches Unity in batch mode (no window, no editor
UI) and calls the exact build entry point (`BuildScript.BuildDesktop` or
`BuildScript.BuildWebGL`) that compiles your scripts, packages the
scene(s), and produces a real standalone build — a native `.exe` for
Desktop, a WebAssembly bundle for WebGL. This takes real time, and will
take longer as the project grows; it is not instant.

**This is also the only point where the console-mode settings
(fullscreen, locked 1920×1080, controller-only navigation) actually get
tested.** The Unity Editor's own Play button never shows you this — it
always runs in a normal resizable Editor window, regardless of what
`ProjectSetup.cs` configured. If you want to know whether the "real"
experience feels right, a build is the only way to check.

### 4. What happens next splits depending on target

**Desktop:** you're done. The build finishes at `Builds\Desktop\`, you
can double-click the `.exe` directly, right now, on your own machine. No
further steps, no internet required.

**WebGL:** the script keeps going automatically —
1. Looks up your already-provisioned AWS target for that `PersonName`
   (this is why the prerequisite above matters — if it's not provisioned
   yet, this fails loudly and tells you to run the provisioning script
   first, rather than silently doing nothing)
2. Syncs the build output to your S3 bucket
3. Invalidates the CloudFront cache so the new version actually shows up
   instead of a stale cached one
4. Prints your Play URL

Once that finishes, the new version is live at your CloudFront URL —
that's the moment it becomes genuinely playable by anyone with the link,
not just you.

## A shortcut that's *not* part of this path — and why it's not enough on its own

Pressing **Play inside the Unity Editor** runs the game live, in seconds,
without a full build. This is where almost all of your actual iteration
and testing should happen day to day — it's fast, and a full build for
every tiny change would be painfully slow.

But it is not a substitute for the real critical path above, because it
cannot show you:
- The real console-mode fullscreen experience (Editor Play Mode is
  always a normal window)
- Whether the actual `BuildScript` entry points still work
- Any WebGL-specific behavior at all, since Editor Play Mode never runs
  in a browser

The practical pattern: use Editor Play Mode constantly while working, and
only run a real build (this document) at real checkpoints — before
showing someone else, before considering something "done," or
specifically when you need to test something Play Mode can't.

## Troubleshooting this specific process

**Build fails immediately with a strange error, and I definitely made a
real code change:** check whether the Unity Editor is still open. This is
the most common cause by far — see Step 2.

**Desktop build works, WebGL build fails at the AWS step:** your target
probably isn't provisioned yet. Run
`scripts\provision-aws-target.ps1 -PersonName "<you>"` once, then retry
the WebGL build.

**I built and deployed, but my browser still shows the old version:**
CloudFront invalidation is usually fast but isn't always instantaneous.
Try a hard refresh (Ctrl+Shift+R) before assuming the deploy didn't work.

---

**Guided and inspired by Terrence. Written by Claude Sonnet 5.**
