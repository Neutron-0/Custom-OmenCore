# OmenCore v3.7.1 - Final Release Checklist

**Prepared:** 2026-06-07
**Release:** v3.7.1
**Configuration:** Release
**Status:** Full release - automated checks passed, remaining hardware-specific items documented

---

## 1. Automated Gates

| Gate | Required Result | Status |
|---|---|---|
| `dotnet restore` | Restore succeeds | Passed during release package builds |
| `dotnet build -c Release --no-restore` | 0 errors, 0 warnings | Passed: 0 errors, 0 warnings |
| `dotnet test -c Release --no-build` | All tests pass | Passed: 781/781 |
| `git diff --check` | No whitespace errors | Passed |
| `dotnet format --verify-no-changes` | Advisory only for 3.7.1 | Deferred: broad pre-existing formatting debt |

---

## 2. Version And Artifact Consistency

- [x] `VERSION.txt` is `3.7.1`.
- [x] Windows app project version is `3.7.1`.
- [x] Hardware worker project version is `3.7.1`.
- [x] Linux CLI project version is `3.7.1`.
- [x] Avalonia GUI project version is `3.7.1`.
- [x] Installer fallback version is `3.7.1`.
- [x] README artifact names use `3.7.1`.
- [x] `OmenCoreSetup-3.7.1.exe` built locally.
- [x] `OmenCore-3.7.1-win-x64.zip` built locally.
- [x] `OmenCore-3.7.1-linux-x64.zip` built locally.
- [x] Local SHA256 files generated for all release artifacts.
- [x] `SHA256SUMS-3.7.1.txt` generated locally.
- [ ] Final GitHub Release notes include SHA256 hashes for every published artifact.

> `src/OmenCore.Desktop` is an archived prototype and is intentionally not version-bumped during release cycles.

---

## 3. Install And Upgrade Matrix

- [ ] Clean Windows install from `OmenCoreSetup-3.7.1.exe`.
- [ ] Upgrade Windows install from previous stable.
- [ ] Portable Windows zip launches and writes logs/config in the expected location.
- [ ] Linux `omencore-cli status` runs from the `OmenCore-3.7.1-linux-x64.zip` bundle.
- [ ] Linux profile command applies or reports unsupported capability clearly.
- [ ] Auto-updater rejects release notes without SHA256 and accepts release notes with matching SHA256.

---

## 4. Driver And Security Matrix

- [ ] PawnIO missing: app starts, warns clearly, and hides/gates PawnIO-only actions.
- [ ] PawnIO installed: EC/MSR-backed features detect availability.
- [ ] Secure Boot enabled with PawnIO: no misleading unsupported-driver warning.
- [ ] Run as non-admin: privileged actions fail with actionable messaging.
- [ ] Run as admin: privileged actions use the expected backend.

---

## 5. Hardware Capability Matrix

- [ ] Unsupported model: app stays in safe/limited mode and diagnostics explain why.
- [ ] Known supported OMEN model: fan, performance, RGB, and diagnostics paths appear as expected.
- [ ] NVIDIA GPU present: NVAPI/NVIDIA telemetry initializes or degrades explicitly.
- [ ] No NVIDIA GPU: app does not show NVIDIA-only actions as available.
- [ ] Hybrid AMD+NVIDIA laptop: hardware worker does not crash in AMD ADL frame metrics and NVIDIA telemetry remains active.

---

## 6. Reported Model Validation

- [ ] `8D2F` OMEN 16-am0xxx: Auto handoff clears stale fan floors after load.
- [ ] `8D2F`: diagnostics show undervolt disabled by model capability before PawnIO/MSR probing.
- [ ] `8D2F`: performance profiles record WMI fallback / EC skip state in `launch-readiness.txt`.
- [ ] `8E41` OMEN Transcend 14: profile-only fan controls apply OEM modes without custom curve writes.
- [ ] `8E41`: Custom curve card/editor remains disabled or unavailable.
- [ ] `8BD4` Victus 16: fans ramp down after long gaming sessions.
- [ ] `8BD4`: AMD ADL quarantine prevents hardware-worker crash while keeping NVIDIA telemetry.
- [ ] `8574` OMEN 15-dc1077tx: collect fresh 3.7.1 diagnostics after Restore OEM Auto, Fan Cleaner, Max, Auto, and profile changes.
- [ ] `8574`: confirm whether fan cleaner low-output behavior changes RPM/level readback.
- [ ] OMEN 16 Max: per-key-capable hardware is detected without claiming the dedicated HID editor/backend is implemented.
- [ ] OMEN 16 Max: zone/light-bar fallback remains available where supported.

---

## 7. Functional Smoke Test

- [ ] Quick Access Quiet, Balanced, and Performance one-click buttons apply combined profiles.
- [ ] Quick Access Auto, Curve, and Max fan-only controls still work where supported.
- [ ] Fan Control Restore OEM Auto clears OmenCore fan ownership and records command history.
- [ ] Fan Auto mode returns control to firmware without leaving stale fixed fan levels.
- [ ] Diagnostics export includes `launch-readiness.txt`.
- [ ] `launch-readiness.txt` includes performance trace, fan recovery state, CPU authority, RGB backend status, and AMD ADL quarantine expectations.
- [ ] App closes cleanly from tray and main window.

---

## 8. Deferred From 3.7.1

- Dedicated OMEN Max `HidPerKey` backend/editor.
- OGH Eco equivalent combining low performance and 60 Hz display behavior.
- Direct PL1/PL2 control discovery beyond current firmware/MSR-gated paths.
- WPF UI framework migration or broad shell redesign.
- Whole-repo `dotnet format` cleanup.

---

## 9. Release Verdict

Automated validation supports a full v3.7.1 release. The unchecked manual hardware items above remain tracked as known limitations and follow-up validation items in the release notes.
