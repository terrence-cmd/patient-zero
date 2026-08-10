# Gate 0 QA Report (Cursor)

**Gate 0 blocked on: physical Xbox two-gamepad hardware test (Needs Human Execution)**

Independent QA pass per [cursor-qa-brief-gate0.md](../cursor-qa-brief-gate0.md).
Reviewer did not re-run items the brief marks as already independently verified
(build profiles through `build-manager.ps1`, live WebGL HTTP 200, console-mode
.exe launch, Git LFS pointer check, high-level join-cap review).

---

## Scorecard

| # | Checklist item | Status | Evidence |
|---|---|---|---|
| 1 | Physical two-gamepad hardware test (Xbox / XInput official) | **Needs Human Execution** | No Xbox controllers available to this reviewer. Manual procedure below. Do not treat any non-Xbox trial as the official verdict. |
| 2 | Independent code review (`Assets/Scripts/`, `Assets/Editor/`, `scripts/*.ps1`) | **Pass** | Review completed; one real bug fixed (`PlayerMover` material tint). Flags listed below. Post-fix Play Mode re-run: both tests **Passed**, Unity exit code **0** (log `test-run-playmode-playermover.log`, results `TestResults-PlayMode.xml`). |
| 3 | Mid-session controller disconnect | **Pass** | Observed failure → fixed → re-verified. See item 3 detail. |
| 4 | Public-repo safety + scope boundaries | **Pass** | History + working-tree credential-pattern scan clean; Gate 0 scope boundaries hold. See item 4 detail. |
| — | Confirmation builds (Desktop + WebGL via `build-manager.ps1 -PersonName kenshi`) | **Pass** | Both exit code **0**; artifacts verified on disk (not exit-only). Details below. |

---

## 1. Physical two-gamepad hardware test

**Status: Needs Human Execution** (no Xbox pads available to the reviewer).

### Official Xbox / XInput procedure (Pass/Fail standard)

Use two Xbox controllers (wired USB or wireless — both are XInput on Windows).

1. Close any extra Input Debugger noise; open the project in Unity 6000.3.21f1, load `Assets/Scenes/Main.unity`, press Play **or** launch `Builds/Desktop/PatientZero.exe` (Desktop is preferred for “real” feel; Editor Play is acceptable for join logic).
2. Confirm Console (Editor) or that no players are present yet — empty scene with ground only.
3. **P1 join:** On pad A, press any face button (e.g. **A** / South).  
   **Expect:** One capsule spawns (left, red). Console: `[Join] Player 1 joined...`
4. **P2 join:** On pad B, press any face button.  
   **Expect:** Second capsule (right, blue). Console: `[Join] Player 2 joined...` then `2 players in — joining disabled.`
5. **Move isolation:** Move pad A left stick only.  
   **Expect:** Only the red capsule moves. Repeat with pad B → only blue moves.
6. **3rd pad rejected:** Connect pad C (or a third XInput device) and press a face button.  
   **Expect:** Still exactly two players; no third capsule.
7. **Leave / re-open slot:** Turn off or unplug pad A (battery dead / USB unplug).  
   **Expect:** Red capsule destroyed; Console shows device lost + Player left; joining re-enabled. Press face button on pad C (or re-powered pad A).  
   **Expect:** A new player joins into the free slot; back to two players max.

**Pass** only if all Xbox steps above match. Record date, Desktop vs Editor, and wired/wireless.

### Best-effort note (not official)

Not run — no alternate controllers exercised in this pass.

---

## 2. Independent code review

**Status: Pass** (with fix applied and re-verified).

### Fix applied

- **`PlayerMover.cs`:** `renderer.material.color = ...` instantiated a material per join (leaks under join/leave/disconnect). Replaced with `MaterialPropertyBlock` + `_Color`.
- **Re-verification (required, same standard as item 3):** Unity Play Mode batch run after the change — `TwoGamepads_BothJoin_EachControlsDistinctPlayer_CapIsTwo` **Passed** (0.326s), `MidSessionDisconnect_CleansUpPlayer_AndReopensJoinSlot` **Passed** (0.178s), Unity exit code **0**, no `error CS` / compilation-failed hits in `test-run-playmode-playermover.log`.

### Prior fix in scope of this QA window (item 3, also reviewed here)

- **`TwoPlayerJoinController.cs`:** Mid-session device loss left orphaned players and a stuck 2-player cap. `PlayerInput.onDeviceLost` C# hooks do not run when the prefab uses `SendMessages` (default). Fix listens to `InputUser.onChange` for `DeviceLost`, matches `player.user`, `Destroy`s that player, unsubscribes in `OnDisable`.

### Flags (judgment / out of scope — not changed)

1. Join cap is enforced in `HandlePlayerJoined` after the join; a same-frame double-join race is theoretically possible; sequential 3rd-pad rejection is what tests/hardware cover.
2. `ProjectSetup.ApplyAll` overwrites `Main.unity` / player prefab — “idempotent” means safe to re-scaffold, not safe after hand edits.
3. `BuildScript` success path relies on `-quit` rather than explicit `Exit(0)` — already field-verified; left alone.
4. `build-manager.ps1` does not check Editor lock (human step in `BUILD_AND_DEPLOY.md`); Desktop verifies output folder, not specifically `PatientZero.exe`.
5. `PlayerMover` spawn/color follow `playerIndex`, not “original seat identity,” after disconnect/rejoin — acceptable for Gate 0 device proof.

---

## 3. Mid-session controller disconnect

**Status: Pass**

| Step | Result |
|---|---|
| First Play Mode run (pre-fix) | **Failed** — `onPlayerLeft` never fired; no `[Join] ... device lost` log. Root cause: prefab `PlayerInput` `m_NotificationBehavior: 0` (SendMessages); C# `onDeviceLost` only runs under `InvokeCSharpEvents`. |
| Fix | `InputUser.onChange` → filter `DeviceLost` → `player.user == user` → `Destroy`; unsubscribe in `OnDisable`. |
| Assertion polish | Do not assume `PlayerInput.all` order after mid-list disconnect. |
| Final Play Mode run | **Passed** — log shows join → device lost → left → new pad joins → cap at 2. |
| Regression | Original `TwoGamepads_BothJoin_...` **Passed** on the same runs. |

Test: `Assets/Tests/PlayMode/TwoPlayerJoinTests.cs` → `MidSessionDisconnect_CleansUpPlayer_AndReopensJoinSlot`.

---

## 4. Public-repo safety pass

**Status: Pass**

### Credential / secret scan

- Working tree: no hits for `AKIA*` / `ASIA*`, `aws_secret_access_key=`, PEM/OpenSSH private key headers, `ghp_` / `github_pat_` / `sk-` token shapes (`git grep`).
- Full history: no hits via `git log -S` pickaxe on those patterns and `git grep` across `git rev-list --all`.

### Deliberate non-secret infra (flagged, not removed)

`PROVISIONED_TARGETS.md` lists AWS account id, bucket names, CloudFront distribution ids, and play URLs. These are **not** access keys; the doc states keys are handed out-of-band and never recorded. No change made (AWS/docs edits of that sort are out of scope for this brief unless a real secret appeared).

### Scope boundaries vs `fable5-task-brief-gate0.md`

| Boundary | Observed |
|---|---|
| No combat beyond placeholder | `Assets/Scripts/Combat/` empty |
| No netcode / online | No networking identifiers under `Assets/`; `Netcode/` folder placeholder only |
| No AWS config in Unity project | No credentials/PEM under Assets; `Backend/` only `.gitkeep` |
| Runtime scripts | Only `PlayerMover.cs` + `TwoPlayerJoinController.cs` |
| Build contract | `BuildScript.BuildWebGL` / `BuildDesktop` unchanged in name |

---

## Confirmation builds

**Command contract (from brief):**

```
.\scripts\build-manager.ps1 -PersonName "kenshi" -Target Desktop
.\scripts\build-manager.ps1 -PersonName "kenshi" -Target WebGL
```

**Status: Pass** — exit codes checked **and** artifacts verified independently.

| Target | `build-manager` exit | Artifact check | Notes |
|---|---|---|---|
| Desktop | **0** | `Builds\Desktop\PatientZero.exe` present (667,136 bytes stub); **player content refreshed** — e.g. `PatientZero_Data\boot.config` / `level0` / `globalgamemanagers*` LastWriteTime **12:06 PM** (same window as this run). Stub `PatientZero.exe` / `UnityPlayer.dll` kept older timestamps (identical native binaries often not rewritten). | Log: `qa-confirm-desktop.log`; Unity log: `build-log-Desktop.txt` — `[BuildScript] StandaloneWindows64 build SUCCEEDED -> Builds/Desktop/PatientZero.exe (90615892 bytes, 36s, 0 errors, 0 warnings)` then `Exiting batchmode successfully now!` return code 0 |
| WebGL | **0** | `Builds\WebGL\index.html` present; **payload refreshed** — `Build\WebGL.data` / `.wasm` / `.framework.js` / `.loader.js` LastWriteTime **12:19 PM**. `index.html` timestamp unchanged (template). | Built, synced to S3, CloudFront invalidated. Play URL: `https://d22jn5ymxt1ztg.cloudfront.net` (`qa-confirm-webgl.log`). Unity: `[BuildScript] WebGL build SUCCEEDED -> Builds/WebGL (23401666 bytes, 186s, 0 errors, 0 warnings)` |

Desktop log excerpt:
```
Build succeeded: C:\Users\tocam\Documents\GameDev\patient-zero\Builds\Desktop
Desktop build is local-only. Output at: C:\Users\tocam\Documents\GameDev\patient-zero\Builds\Desktop
```

WebGL log excerpt:
```
Build succeeded: C:\Users\tocam\Documents\GameDev\patient-zero\Builds\WebGL
Bucket: patient-zero-webgl-kenshi-493168378006
Distribution: E1ED294MQFZ8Y7
== Deployed: kenshi ==
Play URL: https://d22jn5ymxt1ztg.cloudfront.net
```

**Verify-don’t-trust note:** trusting only `PatientZero.exe` / `index.html` timestamps would have falsely looked “stale.” Checking `PatientZero_Data` content files and `Builds/WebGL/Build/*` payload timestamps (plus BuildScript SUCCEEDED lines) is what confirms this run actually rebuilt.

---

## Files touched this QA pass

| File | Change |
|---|---|
| `Assets/Scripts/Input/TwoPlayerJoinController.cs` | Device-lost cleanup via `InputUser.onChange` |
| `Assets/Tests/PlayMode/TwoPlayerJoinTests.cs` | Mid-session disconnect Play Mode test |
| `Assets/Scripts/Characters/PlayerMover.cs` | `MaterialPropertyBlock` tint (no material instantiate) |
| `docs/07-cursor-qa-report.md` | This scorecard |

---

## Out of scope (not done, per brief)

- Gate 1 gameplay / frame data / move library
- Any AWS resource changes
- Anything outside the four checklist items + required confirmation builds / bugfixes
