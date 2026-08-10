# Installing Unity — the reliable path

Getting Unity installed and working turned out to be the single most
failure-prone step in setting up this environment — more so than
anything AWS-related. This document is the clean procedure that actually
works, distilled from a real install session that hit several genuine
bugs along the way. The failure modes below aren't hypothetical — every
one of them actually happened.

## The pinned version

This project targets **Unity 6000.3.21f1** (LTS). Don't install "latest"
— see `docs/01-environment-setup.md` for why a pinned version matters.
If this project ever moves to a newer LTS, update this document's
version number and changeset (see "Finding the changeset" below) at the
same time.

## The reliable install procedure

### 1. Create/sign in to a Unity ID first — before anything else

Unity requires a free Unity ID account to activate the Editor (Personal
license), and trying to install before this is ready is a real way to
get things to hang or silently fail partway through. Go to Unity's
account sign-up/sign-in and make sure this is done *before* starting the
Editor install, not partway through.

### 2. Install Unity Hub

Either works for getting Hub itself installed:
```
winget install --id Unity.UnityHub --silent --accept-package-agreements --accept-source-agreements
```
or download the installer directly from unity.com and run it.

**Pick one and stick with it.** Don't end up with both a winget-installed
version and a manually-downloaded one on the same machine — see
"Two Hub installs fighting each other" below for why that's a real
problem, not just untidy.

### 3. Find the changeset for your target version

Every Unity release has a changeset hash tied to it (visible in the
release notes on unity.com, or in the version string of an already-
installed Editor, e.g. `6000.3.21f1 (c02631ffc030)` — that last part is
it). For 6000.3.21f1, the changeset is `c02631ffc030`.

### 4. Download the Editor and module installers directly

Skip Unity Hub's own download manager — see "Hub's headless CLI
silently does nothing" below for why. Instead, pull the installers
straight from Unity's CDN, using the changeset from step 3:

```
https://download.unity3d.com/download_unity/<changeset>/Windows64EditorInstaller/UnitySetup64-<version>.exe
https://download.unity3d.com/download_unity/<changeset>/TargetSupportInstaller/UnitySetup-WebGL-Support-for-Editor-<version>.exe
https://download.unity3d.com/download_unity/<changeset>/TargetSupportInstaller/UnitySetup-Windows-IL2CPP-Support-for-Editor-<version>.exe
```

For 6000.3.21f1 specifically, that's changeset `c02631ffc030` in each
URL above.

### 5. Install each one silently, in order — Editor first, then modules

```powershell
$targetDir = "C:\Program Files\Unity\Hub\Editor\6000.3.21f1"
Start-Process -FilePath "UnitySetup64-6000.3.21f1.exe" -ArgumentList "/S","/D=$targetDir" -Wait
Start-Process -FilePath "UnitySetup-WebGL-Support-for-Editor-6000.3.21f1.exe" -ArgumentList "/S","/D=$targetDir" -Wait
Start-Process -FilePath "UnitySetup-Windows-IL2CPP-Support-for-Editor-6000.3.21f1.exe" -ArgumentList "/S","/D=$targetDir" -Wait
```

**Use `Start-Process -Wait`, not the `&` call operator.** This matters —
see "Unity's exit code lies" below.

### 6. Verify with a real test, not just "the files exist"

```powershell
$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.21f1\Editor\Unity.exe"
$testProject = "$env:TEMP\unity-install-check"
$proc = Start-Process -FilePath $unity -ArgumentList "-batchmode","-nographics","-quit","-createProject",$testProject,"-logFile","$env:TEMP\unity_check.log" -PassThru -Wait
"Exit code: $($proc.ExitCode)"   # should be 0
Test-Path "$testProject\Assets"  # should be True
```

If both check out, the install is genuinely good — not just "the
installer said it finished."

## Failure modes actually hit — what they look like and why

### Two Hub installs fighting each other

**What it looks like:** Unity Hub's window opens, then immediately
closes, repeatedly, with no error message.

**Why:** winget's Unity Hub package installs as an MSIX app. If a
traditionally-installed Hub also exists (or vice versa), both can
register as handlers for the same launch, and one instance detects a
"duplicate" and self-closes. Only keep one installation method.
**Fix:** `Remove-AppxPackage` the MSIX version (or uninstall the
traditional one) so exactly one remains, then relaunch.

### Hub's headless CLI silently does nothing — for adding modules to an *existing* Editor

**What it looks like:** `Unity Hub.exe -- --headless install --version X
--module Y --cm` returns immediately, exit code 0, and nothing happens —
no new process spawns, no download, no error.

**Why:** this command reliably works for a *fresh* install (Editor +
modules together, first time). It does **not** reliably work for adding
a module to a version that's already installed and registered in Hub —
it appears to just no-op. This isn't a one-off; it reproduced multiple
times, on both the MSIX and traditional Hub installs.
**Fix:** this is exactly why step 4-5 above skip Hub's download manager
entirely and pull installers directly from Unity's CDN instead — that
path is fully reliable.

### Unity's exit code lies (when invoked the wrong way)

**What it looks like:** a batch-mode Unity build reports `Build FAILED
(exit code )` — note the *blank* exit code — even though the build log
itself clearly shows `Result: Success` and `return code 0` at the very
end.

**Why:** invoking `Unity.exe` with the bare `&` PowerShell call operator
does not reliably surface its real exit code in every invocation
context. `Start-Process -Wait -PassThru` and reading `.ExitCode` does,
consistently. This bit both the install process and `build-manager.ps1`
independently — same root cause both times.

### A relative `-projectPath` can fail to resolve

**What it looks like:** `. is not a valid directory name` — an error
that has nothing to do with your actual project.

**Why:** passing `-projectPath .` works fine interactively but isn't
reliable across all invocation contexts (background processes,
different working directories). **Fix:** always resolve to an absolute
path before passing it to Unity — `(Resolve-Path $path).Path`.

### A stale "last project" reference derails an unrelated command

**What it looks like:** running Unity in batch mode without an explicit
`-projectPath` tries to reopen whatever project it last had open —
including a broken/nonexistent one from an earlier failed attempt — and
exits with code 1 for reasons that look unrelated to what you're
actually trying to do.

**Fix:** always pass an explicit `-projectPath` or `-createProject`,
never rely on Unity's default "reopen last project" behavior in
scripted/batch contexts.

## After install: confirm the pinned version, every time

Whatever machine this runs on, confirm `Unity.exe`'s version string
matches `6000.3.21f1` exactly before trusting anything else in this
repo to work against it. A different patch version may behave
differently in ways that are hard to predict.
