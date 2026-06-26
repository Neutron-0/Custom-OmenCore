# OmenCore v3.8.2 - Critical Hang Fix

**Release Date:** TBD
**Release Status:** Code-complete, test-verified, and artifacts built in this environment; field confirmation from the original reporter on physical `8BCD` hardware still pending before tagging
**Type:** Patch release (release-blocker fix)
**Base Version:** v3.8.1

---

## Purpose

v3.8.2 exists solely to fix a critical regression reported within hours of the v3.8.1 release: OmenCore hangs and is force-closed by Windows ("Application Hang", Event ID 1002) within 10-20 seconds of launch. v3.8.1 is withdrawn as a recommended download pending this fix; see [3.8.1-BUG-REPORTS.md](3.8.1-BUG-REPORTS.md) for the full incident writeup (`BUG-3820-001`).

## Fixed

### Critical: Application Hang Within Seconds Of Launch (BUG-3820-001)

**Reported by:** OsamaBiden (Discord, OMEN 16-xd0010ax / ProductId `8BCD`), 2026-06-24, immediately after the v3.8.1 release. Two consecutive launches hung and were force-closed by Windows; Event Viewer confirmed `Application Hang`, `HangType=Cross-process`, `OmenCore.exe 3.8.1.0`.

**Root cause:** `HardwareWorkerClient.SendRequestAsync()` (the named-pipe client that talks to the out-of-process `OmenCore.HardwareWorker.exe`) reused a single `NamedPipeClientStream` across every request with no serialization and no recovery path after a timed-out read:

- If a worker response took longer than the client's `RequestTimeoutMs` (2000ms) — plausible under GC pauses, AMD ADL2/NVAPI driver calls, or just system load — the client's read was cancelled, but the now-late response message was left sitting, unconsumed, in the pipe's receive buffer.
- The *next* request's read would then consume that stale message instead of its own reply, permanently shifting every subsequent request/response pair by one. There was no request/response correlation and no reconnect-on-failure, so the connection never resynchronized itself.
- This was already visible in the field logs as the repeated `🥶 CPU/GPU temperature appears frozen` warnings (`WmiBiosMonitor.UpdateReadings`) immediately before the hang. The affected model (`8BCD`) has `_workerBackedCpuTempOverrideEnabled` set, so it calls into this exact pipe path on every monitoring cycle (every 2-5s) — far more aggressively than models that don't rely on the worker-backed CPU temperature override — which explains why this model hit the bug hard enough to hang within seconds while it went unnoticed elsewhere.
- The escalating retries from this desync (`WmiBiosMonitor`'s `Task.Run(...).Wait(timeout)` wrapper around the now-permanently-failing worker calls, fired every monitoring tick) is consistent with the eventual thread-pool/responsiveness exhaustion that Windows reported as a cross-process hang.

**Fix (`HardwareWorkerClient.cs`):**
- Added a `SemaphoreSlim(1, 1)` request gate around the entire write+read round-trip in `SendRequestAsync`, so concurrent callers queue instead of racing reads/writes on the shared pipe handle.
- On any write/read failure or timeout, the pipe handle is now disposed and nulled instead of reused. The existing `ShouldRecoverConnection`/`TryConnectToExistingWorkerAsync`/`TryRestartWorkerAsync` machinery (already used for worker-process-death recovery) now also handles this case, establishing a fresh, correctly-synchronized connection on the next call instead of perpetuating a desynced one.
- `WriteAsync`/`FlushAsync` are now covered by the same per-request cancellation token as the read (previously unguarded).
- Also fixed two newly-introduced bare `catch {}` blocks flagged by the repo's release-gate hygiene test (`ReleaseGateCodeHygieneTests`) to log the swallowed exception instead of silently discarding it.

**Why this was not a fan/thermal control change:** This is an IPC reliability fix in the telemetry transport layer only. No fan-control activation timing, debounce, EC-write gating, or thermal-protection threshold was touched — consistent with this project's standing rule that those require physical-hardware evidence before any change (see `feedback-omencore-safety` norms). The hang reproduces independent of any specific thermal event.

**Verification performed in this environment (not OMEN hardware):**
- Added `HardwareWorkerClientPipeTests` (2 new tests) using a real `NamedPipeServerStream`/`NamedPipeClientStream` pair with reflection-injected pipe state: one proves a non-responding server now disposes the client pipe instead of leaving it reusable; one proves 5 concurrent requests against a deliberately slow echo server each get back their *own* response, never another caller's.
- Full Release build: 0 errors.
- Full Release test suite: 895/895 passed (up from 894 — includes the 2 new tests and the hygiene-gate fix).
- Smoke-launched `OmenCore.exe` (Release build) on this dev machine (non-OMEN hardware) for 18+ seconds: process stayed responsive (`Get-Process.Responding = True`), clean shutdown, no errors/exceptions in the session log.

**Not yet done / explicitly still open:**
- The original reporter has not yet confirmed v3.8.2 resolves the hang on their `8BCD` hardware. Do not mark this "Fixed" in the public sense until they confirm — see Release Conditions below.
- This fix addresses the protocol-desync/hang mechanism; it does not change the deeper question of *why* the worker sometimes responds slowly on this model (driver contention, etc.). If slow responses continue, they should now degrade gracefully (timeout + clean reconnect) instead of cascading into a hang.

## Minor Improvements (Code-Quality / Reliability Polish)

These are small, hardware-independent cleanups verified by build + the full test suite. They do **not** touch any fan/thermal/EC control path and carry no behavior risk; they ride along with the hotfix because they are zero-risk and thematically aligned with its telemetry-reliability/diagnosability focus.

- **Removed per-poll wasted work in the hardware-worker telemetry path.** `HardwareWorkerClient.GetSampleAsync()` previously logged a debug line on every ~2-second worker poll that ran an `O(n)` `json.Contains("GpuTemperature")` substring scan over the full telemetry payload purely to format a boolean — work that executed on every poll even though the message is dropped at the sink in production. Removed; the adjacent "Deserialized sample" debug line already records the parsed result. Eliminates a recurring per-poll string-build + substring scan on the monitoring hot path (relevant to the standing background-resource concern).
- **Made the diagnostic-logging subsystem's own failures visible.** Converted three bare `catch {}` blocks in `DiagnosticLoggingService` (capture-task shutdown wait, and the relevant-process enumeration loop) to typed catches — `Debug.WriteLine` for single-point failures, a typed silent skip for the per-process inspection loop where logging would be noise across normal system processes. This is the same "logs just stop with no trace" lesson that made `BUG-3820-001` hard to diagnose, applied to the diagnostic subsystem itself. Baseline updated in `ReleaseGateCodeHygieneTests`.
- **Cleared all build warnings and a stale hygiene-baseline entry** (carried from earlier this session): four `CS1998` async-without-`await` warnings in `BloatwareManagerViewModel` (the preset methods did purely synchronous work) were resolved by making the methods synchronous; the resolved `HardwareWorkerClient` bare-catch baseline entry from the hang fix was removed. The main app now builds with **0 warnings**.

## Carried Forward From v3.8.1 (Unchanged, Still Hardware-Gated)

These items were already pending hardware validation in v3.8.1 and are out of scope for this hang-fix patch. They are listed here only so the release gate isn't mistaken for "everything closed." See [3.8.1-BUG-REPORTS.md](3.8.1-BUG-REPORTS.md) for full detail:

- GitHub #141 (OMEN 16-ap0xxx key routing / shipped-artifact provenance) — needs physical `8D26` key-event capture.
- GitHub #142 (HyperX OMEN MAX 16 `8E9A` identity) — needs full diagnostic evidence before any exact-identity entry is added.
- GitHub #143 (Victus 15 `8DCD` fan/thermal regression) — needs a bounded, abortable physical load test.
- BUG-3810-005 (Discord fan-spike-at-idle reports) — diagnostics-only change shipped in 3.8.1; needs an affected user's diagnostic export before any activation-timing change is considered.
- PERF-3810-001 resource/responsiveness scenario matrix — needs physical OMEN/Victus hardware to measure against budget.
- AMD GPU OC startup-restore — still manual-only by design; not revisited in this patch.

## Release Conditions

- This patch does not get marked "Fixed" for BUG-3820-001 from this environment's testing alone — it requires the original reporter (or another `8BCD`/worker-override-enabled user) to confirm v3.8.2 launches and runs without hanging.
- All carried-forward items above remain pending exactly as documented in v3.8.1; nothing here should be read as resolving them.
- Version files and release artifacts move to 3.8.2 only for this hang fix; no unrelated version-gated claims are made.

## Current Validation Status

- `dotnet build OmenCoreApp.Tests.csproj -c Release`: passed, 0 errors.
- `dotnet test OmenCoreApp.Tests.csproj -c Release`: passed, 895/895.
- `dotnet build OmenCoreApp.csproj -c Release`: passed, 0 errors.
- Smoke launch of the built `OmenCore.exe` on this (non-OMEN) dev machine: ran 18+ seconds responsive, clean exit, no exceptions logged.
- Version metadata bumped to `3.8.2` across `VERSION.txt`, `OmenCoreApp`, `OmenCore.Avalonia`, `OmenCore.Linux`, `OmenCore.HardwareWorker` project files, the installer script (`OmenCoreInstaller.iss`), the wizard-image generator default, and the Avalonia version fallback string.
- Windows artifacts built successfully with `build-installer.ps1`: `OmenCoreSetup-3.8.2.exe` and `OmenCore-3.8.2-win-x64.zip`.
- Linux artifact built successfully with `build-linux-package.ps1 -SkipBinaryVersionCheck`: `OmenCore-3.8.2-linux-x64.zip`, `.sha256`, and `version.json`. Binary execution smoke was skipped because this run was on Windows, not Linux/WSL (`binaryExecutionSkipped: true` in the verification manifest) — per the release gate, that smoke test and all physical hardware acceptance criteria above remain pending before tagging `v3.8.2`.
- Artifact SHA256 (also recorded in `artifacts/SHA256SUMS-3.8.2.txt`):
  - `F6FEAB2DDB13E1E70470C7665A414F41E219A96E56E35F0C43C9AB3F595EA86E  OmenCoreSetup-3.8.2.exe`
  - `A61D81D36CFF0839A9E74DCC5C31337318BA258A5F43C5F4C2E9AC5BF6D2E895  OmenCore-3.8.2-win-x64.zip`
  - `B37C02B0FDA17743A95094685D8EDAD182EAB50FF98BA0711093B166FDCB2EBC  OmenCore-3.8.2-linux-x64.zip`
- No claim is made that this fix has been validated on physical OMEN hardware; this development environment is not HP hardware. The reporter's confirmation is the actual acceptance criterion for BUG-3820-001.
