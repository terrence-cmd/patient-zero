# Session Handoff

Authoritative record of where this project stands, for picking this back
up in a future session with zero memory of how it got here. If this
conflicts with anything else in the repo, this file wins — it's more
current.

## Where things actually stand

**Gate 0 is functionally done, blocked on exactly one thing:** a second
physical Xbox controller to finish the official two-player hardware
test. Everything else — environment, build pipeline, AWS deploy, Cursor
QA pass, code fixes — is real, verified, and committed. See
[`docs/07-cursor-qa-report.md`](docs/07-cursor-qa-report.md) for the
full scorecard; its verdict line currently reads `Gate 0 blocked on:
physical Xbox two-gamepad hardware test (Needs Human Execution)`.

## Tonight's hardware test session — the part not yet folded into the scorecard

A real Xbox controller showed up partway through this session: **model
1914 (Xbox Series X|S wireless controller)**. What actually happened,
in order:

1. **Bluetooth pairing succeeded at the Windows level** (`Xbox Wireless
   Controller`, Bluetooth class, Status OK) but **the game never saw
   any input from it.**
2. Diagnosed via Unity's Input Debugger: the controller showed up as
   `045E:0B13` (Microsoft's real vendor ID, Bluetooth product ID) but
   landed in Unity's "22 unsupported items" bucket — Input System
   couldn't match it to a `Gamepad` layout, only saw a raw/generic HID
   descriptor.
3. **Confirmed via web search this is a known, real, cross-platform
   gap** — not a bug in this project. Xbox Series controllers over
   Bluetooth specifically (VID:PID `045E:0B13`) have documented
   recognition/mapping issues on multiple platforms (Linux driver
   projects have open issues about this exact ID). Sources are in the
   conversation transcript, not yet copied into a repo doc.
4. **Fix: switch to wired (USB-C).** Over USB the same controller
   reports as `045E:0B12` and Windows correctly shows it as
   `XboxComposite` / `XINPUT compatible HID device` — a completely
   different, well-supported path.
5. **Confirmed working with real hardware evidence**, not just visual
   impression — pulled directly from the Standalone build's
   `Player.log`:
   ```
   [Join] Player 1 joined (devices: XInputControllerWindows:/XInputControllerWindows)
   ```
   Player 1 join is genuinely verified over wired connection with the
   real controller.

**What's still not done:** only one physical controller was available
tonight. The full official procedure in the scorecard needs a second
Xbox controller for P2 join, move-isolation between two simultaneous
pads, 3rd-pad rejection, and the disconnect/rejoin check with real
hardware — all of that logic is already verified via the automated
Play Mode tests (`TwoPlayerJoinTests.cs`), just not yet with a second
physical device.

## Immediate next steps, in order

1. **Get a second Xbox controller** (or borrow one), connect it wired
   (learned tonight: don't trust Bluetooth for this specific
   controller/Unity combination), and run through the exact procedure
   already written in `docs/07-cursor-qa-report.md` item 1.
2. **Update the scorecard's item 1 status and the verdict line** once
   that's done — if it passes, the verdict becomes `Gate 0 complete`.
3. **Fold tonight's Bluetooth-vs-wired finding into `CONTROLLER_SETUP.md`**
   as a new, real troubleshooting entry — this is genuinely useful,
   verified knowledge (Xbox Series controllers over Bluetooth may not
   register in Unity at all despite Windows showing them connected; use
   wired) that isn't written down anywhere in the repo yet. Not done
   yet, flagged here specifically so it doesn't get lost.
4. Known, already-tracked, lower-priority gap: the Xbox in-box-cable
   clarification noted in `README.md`'s "Known gaps" section is still
   unaddressed too.

## Process/environment state at end of session

- Two `PatientZero.exe` Standalone instances were running during
  testing; the stale one was killed, one real instance may still be
  running depending on what happens right after this handoff is
  written — check `tasklist` before assuming either way.
- Unity Editor was opened (GUI mode) specifically to use the Input
  Debugger for hardware diagnosis — may still be open.
- The Xbox controller (model 1914) is currently connected **wired**
  (USB-C to USB-C), not via its earlier Bluetooth pairing. The
  Bluetooth pairing itself is still registered in Windows and doesn't
  need to be removed — it's just not the connection being used.

## Everything else — confirmed solid, no action needed

- **Repo**: public at `github.com/terrence-cmd/patient-zero`, all
  cross-doc links verified traversable (not just styled text), latest
  commits pushed.
- **AWS**: 6 targets provisioned (`kenshi`, `JohhnyCage`, `SubZero`,
  `Scorpian`, `SonyaBlade`, `Kitana`) — see
  [`PROVISIONED_TARGETS.md`](PROVISIONED_TARGETS.md). Credentials for
  the 5 non-owner targets were generated once and handed off via
  `C:\Users\tocam\Documents\PatientZero_Credentials.docx` (outside the
  repo on purpose — never commit that file).
- **Unity**: 6000.3.21f1 (LTS, pinned) installed and working, WebGL +
  Windows-IL2CPP modules confirmed. See `UNITY_INSTALL.md` for the
  install procedure and every real failure mode hit getting there.
- **Docs written this session**: `INTEGRITY.md`, `UNITY_INSTALL.md`,
  `CONTROLLER_SETUP.md`, `BUILD_AND_DEPLOY.md`, `CURSOR_SETUP.md`,
  `GAME_FILE_STORAGE.md`, `PROVISIONED_TARGETS.md`,
  `.cursor/rules/patient-zero-qa.mdc`, `cursor-qa-brief-gate0.md` — all
  committed, all linked from `README.md`.
- **Cursor QA pass**: ran for real, found and fixed two genuine bugs
  (mid-session disconnect leaving orphaned players / stuck join cap;
  `PlayerMover` leaking a material instance per join) — both
  independently re-verified, not just claimed. Full detail in
  `docs/07-cursor-qa-report.md`.
