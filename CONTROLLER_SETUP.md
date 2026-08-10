# Controller Setup

How to get a controller working so you can play. **Find your situation
in the list below and jump straight to it — you don't need to read this
whole document, just your one section.**

## Find your situation

- **I have an Xbox controller** → [Step 0](#step-0-what-controller-do-you-have)
- **I have a different controller** (PlayStation, Switch, generic/no-name)
  → [Step 0](#step-0-what-controller-do-you-have), then
  [Using a non-Xbox controller](#using-a-non-xbox-controller)
- **I don't know if my PC has Bluetooth** →
  [Step 1](#step-1-does-your-pc-have-bluetooth)
- **I only have a tablet** (iPad, Android) →
  [Using a tablet](#using-a-tablet-ipad-or-android)
- **I have a Windows tablet / Surface / 2-in-1** →
  [Using a Windows tablet or 2-in-1](#using-a-windows-tablet-or-2-in-1-surface-etc)
- **Can I just use a keyboard?** →
  [Can I use a keyboard instead](#can-i-use-a-keyboard-instead-of-a-controller)
- **Something's not working** → [Troubleshooting](#troubleshooting)

## What you need before you start

To play, you need:
1. A controller (Xbox is what's tested — see Step 0).
2. **If you're going wired: a USB cable that carries data, not just
   power.** This is the single most common thing that goes wrong, and
   it's worth understanding *why* before you start.

**Data cable vs. charge-only cable — what's actually different:** A USB
cable is just wires inside a rubber sleeve. A real data cable has wires
for power *and* for data (the signal that carries "this button was
pressed"). A charge-only cable — common with cheap phone chargers and
some bundled charging cables — only has the power wires. It looks
**completely identical** from the outside. There is no way to tell by
looking at it, or even by looking at the connector.

**What this looks like when it happens:** you plug the controller in,
the light turns on (it's getting power, so it looks like it's working),
and then... nothing. No response to any button, in Windows or in the
game. This is not a broken controller and not a broken game — it's a
cable that can only charge, not talk.

**How to test if your cable is a real data cable:** plug the controller
in, then open **Device Manager** (search for it in the Start menu). Look
through the list for a new entry — usually under **"Xbox Peripherals"**
or **"Human Interface Devices"** — that appeared when you plugged in.
- **New device shows up in the list:** it's a real data cable. If the
  controller still doesn't respond, the problem is something else — see
  [Troubleshooting](#troubleshooting) below.
- **Nothing new shows up, even though the light is on:** it's a
  charge-only cable. Try a different one — the cable that originally
  came in the box with the controller is always a safe bet, since
  manufacturers don't ship charge-only cables with a controller that
  needs to send input.

## Step 0: what controller do you have

**Xbox controller** — this is the one everything below is written for and
tested against. If you have one, skip to Step 1.

**Something else** (PlayStation, Switch Pro, a generic/no-name gamepad) —
it'll probably still work, since the game doesn't specifically block
other brands. But it isn't officially tested, and it might feel a little
different (see [Using a non-Xbox controller](#using-a-non-xbox-controller)
below).
Nothing bad happens if it's not perfect — worst case it just doesn't feel
as tight, or doesn't get recognized at all. If that happens, borrowing an
Xbox controller is the known-good fallback.

## Step 1: does your PC have Bluetooth

Open **Settings → Bluetooth & devices**.

- If you see a Bluetooth toggle/switch there, you have it.
- If there's no Bluetooth option at all, you don't — and that's fine,
  see "Wireless — if your PC does NOT have Bluetooth" below. Most
  laptops have it built in; a lot of desktop towers don't.

## Step 2: pick wired or wireless

### Wired (plug in a USB cable) — recommended default, works on every PC

This is the simplest option and it's not a compromise — it's arguably
*more* reliable than wireless, not less. **Make sure you're using a real
data cable, not a charge-only one — see "What you need" above if you're
not sure.**

1. Plug the controller's USB cable into any free USB port on the PC
   (front ports on a desktop tower are usually easiest to reach).
2. Wait a few seconds — Windows installs what it needs automatically, no
   download or install screen.
3. The Xbox button in the middle of the controller lights up **solid**
   (not blinking) once it's connected. You're done.

### Wireless — if your PC already has Bluetooth (from Step 1)

1. Turn on Bluetooth: **Settings → Bluetooth & devices → On**.
2. On the controller, find the **small pairing button** — it's a tiny
   button near the shoulder buttons, *not* the big Xbox button in the
   middle. Hold it down for a few seconds.
3. The big Xbox button will start **flashing rapidly** — that means the
   controller is in pairing mode.
4. On the PC: **Settings → Bluetooth & devices → Add device →
   Bluetooth**, then click on **"Xbox Wireless Controller"** when it
   shows up in the list.
5. Once paired, the Xbox button goes solid. From now on, just pressing
   the Xbox button reconnects automatically — you never have to pair
   again on this PC.

### Wireless — if your PC does NOT have Bluetooth (a desktop tower, usually)

You have two options; either works.

**Option A — just plug in wired instead (easiest, costs nothing).** Skip
straight to the "Wired" instructions above. This is genuinely the
simplest fix if wireless isn't a must-have.

**Option B — add a small Bluetooth adapter (~$10-15).** Any basic
Bluetooth 4.0-or-newer USB adapter works.
1. Plug the adapter into a free USB port.
2. Windows usually installs it automatically within a minute or two. If
   it doesn't, use the driver link/CD that came with the adapter.
3. Check **Settings → Bluetooth & devices** again — the Bluetooth toggle
   will now be there, where it wasn't before. That confirms it worked.
4. Follow the "Wireless — if your PC already has Bluetooth" steps above.

## Step 3: connecting isn't the same as joining the game

Getting the controller connected to Windows (Steps 1-2) is a one-time
setup. It doesn't automatically put you in the game — once the game is
running, **press any button on the controller** to actually join as a
player. That's a separate step every time you play, not something you
only do once.

## Troubleshooting

**Controller connects but nothing happens in-game:** make sure the game
is actually running and press a button — connecting to Windows and
joining the game are two different things (see Step 3).

**Controller does nothing at all, even in Windows:** double check you're
holding the *small* pairing button, not the big Xbox button, when
pairing wirelessly. If wired, try a different USB port.

**Wired controller randomly disconnects during play:** this is a known
Windows power-saving issue, not a broken controller or a broken game.
Fix: open **Device Manager → Universal Serial Bus controllers → USB Root
Hub** (there may be more than one — check each), right-click →
**Properties → Power Management tab**, and uncheck **"Allow the computer
to turn off this device to save power."** Do this for each USB Root Hub
listed if the first one doesn't fix it.

## Using a non-Xbox controller

A couple of real (but harmless) things that can differ:
- The analog stick might feel drifty or less precise — that's the
  controller's hardware, not a bug in the game.
- The d-pad might feel a little off on diagonals on some brands.
- On rare cheap/generic controllers, Windows might not recognize it as a
  gamepad at all, and nothing will happen when you press buttons.

None of this causes any lasting problem — it only affects how that one
play session feels. Nothing gets saved, remembered, or damaged by a
controller having a rough connection or drifty sticks. If a non-Xbox
controller doesn't work well, borrowing an Xbox controller is always the
fallback that's guaranteed to work as described in this doc.

## Using a tablet (iPad or Android)

**Short answer: not supported right now.** Not because it's blocked on
purpose — nothing was built for it yet. Specifically:
- The Desktop version can't run on an iPad or Android tablet at all,
  under any circumstances — it's a Windows program, full stop.
- The browser version might technically load on a tablet, but there's no
  touch controls built into the game at all — no on-screen joystick, no
  tap-to-move. Even if the page opens, nothing would respond to touch.
- Pairing a real Xbox controller to a tablet and trying the browser
  version *might* work, in theory — tablets can pair Xbox controllers at
  the system level. This has never actually been tested, though, and
  browser controller support on tablets has a spotty track record. Don't
  count on it.

If a tablet is what's available, a Windows PC is the reliable path for
now.

## Using a Windows tablet or 2-in-1 (Surface, etc.)

This is a different situation from an iPad or Android tablet — a Windows
tablet or 2-in-1 is a full Windows PC in a different shape, not a
separate platform. Everything else in this document applies to it
exactly as written.

One thing to know: touching the screen won't do anything either, for the
same reason as above — no touch controls exist. A Windows tablet still
needs an actual controller connected (wired or Bluetooth, same steps as
any other PC) to play at all.

## Can I use a keyboard instead of a controller

**No, not currently** — only gamepad input works right now. This is a
deliberate choice, not an oversight: the way local 2-player is built,
each player is identified by *which physical device* they're using. A
single shared keyboard doesn't cleanly split into "two players" without
real extra design work (deciding how to divide the keys, whether joining
even makes sense the same way, etc.) — it's a real feature to build, not
a quick setting to flip on.

For now, an Xbox controller (or another gamepad, best-effort — see
[Using a non-Xbox controller](#using-a-non-xbox-controller)) is the way
to play.

## Appendix: other things worth knowing

Smaller stuff — none of this needs action ahead of time, just good to
recognize if it comes up so it doesn't look like something's broken.

**Whoever presses a button first becomes Player 1.** Joining is a race,
not a choice — there's no seat-picking. If two people mash a button at
the same instant, which one ends up red (Player 1) vs. blue (Player 2)
is basically unpredictable. Normal behavior, not a bug.

**The LED ring around the Xbox button doesn't mean what you'd think.**
On an actual Xbox console, those four lights show your player number.
On a PC, that light's behavior is inconsistent and usually has nothing
to do with whether you're Player 1 or 2 in this game — don't use it to
figure out who's who.

**Pressing the big Xbox button might open a Windows menu instead of
doing anything in the game.** Windows sometimes intercepts that specific
button for its own overlay (Xbox Game Bar). Use a regular button (A, B,
etc.) to join instead of the Xbox button.

**A wireless controller can fall asleep if it sits idle for a few
minutes.** If you step away and come back to a controller that seems
dead, it probably just powered off to save battery — press the Xbox
button to wake it back up.

**Bluetooth has a real range limit — roughly 30 feet with a clear line
of sight, less through walls.** If the couch or play spot is far from
wherever the PC sits, that's worth knowing before assuming something's
broken. Other Bluetooth devices nearby (phones, other wireless
peripherals) can also cause occasional lag or drops.

**Parental control or "family safety" software can block this.** Some
of these programs restrict installing new device drivers or pairing new
Bluetooth/USB devices by policy. If every step above was followed
exactly and nothing works at all, this is worth checking.

**Some non-Microsoft controllers deliberately disguise themselves as
genuine Xbox controllers.** Certain cheap "Xbox compatible" controllers
copy Xbox's internal ID so Windows treats them exactly like a real one.
They'll work the same as described in this doc, for better or worse —
just know that "Windows says it's an Xbox controller" isn't a 100%
guarantee it's genuinely a Microsoft one.
