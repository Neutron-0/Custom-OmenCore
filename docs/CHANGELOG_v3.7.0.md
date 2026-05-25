# OmenCore v3.7.0 - Runtime Recovery, Fan Authority, and UI Responsiveness

**Release Date:** 2026-05-25
**Release Status:** Pre-release Candidate
**Type:** Major stabilization and architecture hardening release
**Base Version:** v3.6.3

---

## Summary

v3.7.0 is the next fundamentals-first release after the v3.6.x stabilization line. The main goals are to make fan/profile behavior deterministic on active field hardware, reduce focused-window runtime overhead, and continue the architecture cleanup already tracked in the 3.7.0 UI performance audit.

This changelog is intentionally split into field bug fixes and OmenCore-authored improvements so tester reports stay easy to audit.

---

## Field Bug Fixes

### OMEN 16-xd0xxx / ProductId 8BCD Fan Curve and Profile Recovery

- Restored aggressive curve endpoints for built-in fan presets:
  - Performance quick profile uses the restored aggressive Performance/Extreme cooling path and reaches 100% at 75C.
  - Auto/Balanced reaches 100% at 75C.
  - Extreme reaches 100% at 75C.
  - Gaming reaches 100% at 80C.
  - Quiet caps at 85% at 80C; emergency full cooling is handled by the Quiet safety override instead of the normal Quiet curve.
- Added curve temperature smoothing below high-temperature safety territory so one-sample CPU temperature jumps do not jerk fan targets.
- Preserved raw-temperature safety behavior at high temperatures, so smoothing cannot delay critical fan ramp-up.
- Re-enabled the WMI V1 auto-mode manual-floor clear path for ProductId `8BCD` while keeping non-strict EC fan-mode readback.
- Synchronized General quick profiles with curve-backed cooling presets instead of label-only mode changes.
- Fixed Quiet General profile sync so it reports Quiet fan/profile state instead of drifting through Auto.
- Added `Ctrl+Shift+E` as a General profile-cycle hotkey.
- Improved keyboard RGB diagnostics on `8BCD`/xd0xxx-class systems by switching the diagnostics test pattern to full 4-zone ColorTable writes instead of per-zone-only writes that can be ignored by some 2024 BIOS revisions.
- Keyboard lighting telemetry now records V2 backend apply attempts/results, and diagnostics now refreshes telemetry display after test/clear runs so counters no longer remain stuck at `Attempts: 0` during active testing.

### Fan Preset Behavior at High Temperatures

- Max, Extreme, Gaming, Auto/Balanced, Quiet, and Performance curve definitions now use consistent shared fallback data through `FanModeNameResolver`.
- Fan curve smoothing resets cleanly when curves are enabled or disabled.
- Custom fan curves still use their requested endpoints, with safety clamps remaining raw-temperature based.

### Profile and Hotkey Custom-State Accuracy

- `Ctrl+Shift+E` now includes Custom only when a real saved or active custom fan curve exists.
- When the profile hotkey reaches Custom, OmenCore applies the resolved custom curve instead of only changing UI labels.
- If no custom curve exists, the cycle remains `Balanced -> Performance -> Quiet -> Balanced`, preventing fake Custom announcements in OSD, tray history, and General.

### OMEN 16-am0xxx / ProductId 8D2F Identity and Power-Mode Recovery

- Promoted exact ProductId `8D2F` from medium-confidence/unverified to high-confidence verified identity for current field reports.
- Renamed the displayed capability and keyboard profile from the temporary shared AMD/Intel label to `OMEN 16-am0xxx (8D2F)`.
- Kept direct EC fan writes, independent fan curves, and undervolt disabled for this board because the ProductId has appeared across AMD and Intel Core Ultra variants.
- Enabled the opt-in WMI thermal/performance-policy fallback for `8D2F` and the ProductId-missing `16-am0xxx` Intel fallback so Performance mode can still use the OEM policy path when direct EC/MSR power-limit writes are unavailable.
- Left direct CPU PL1/PL2 controls runtime-gated: if firmware reports `0W/0W` or locks package limits, OmenCore should report that state as firmware-controlled/unavailable instead of exposing broken sliders.
- Promoted the exact `8D2F` keyboard entry to high-confidence so Model Identity no longer emits the "not user-verified yet" warning for keyboard support.

### OMEN MAX 16-ah0xxx / ProductId 8D41 Power Policy and RGB Identity

- Enabled the opt-in WMI thermal/performance-policy fallback for `8D41` so General Quick Profiles can still send the OEM performance policy when direct EC/MSR power-limit values report `0W/0W`.
- This addresses the field pattern where the RTX 5090 Laptop GPU stayed below roughly 100W after Quick Profile changes until the user manually toggled Max fan in the OMEN/Custom tab.
- Kept legacy EC writes disabled for `8D41`; this board still uses the MAX-series safe path because legacy EC registers can corrupt EC state.
- Added an exact ProductId `8D41` keyboard profile so Model Identity no longer reports `Keyboard model: Unknown` for OMEN MAX 16t-ah000 reports.
- Marked the `8D41` keyboard as per-key-capable MAX hardware with WMI ColorTable fallback while HID per-key behavior remains field-unverified; the RGB page should now distinguish "per-key backend not available yet" from "this laptop is unsupported."

### OMEN 17-ck1xxx CPU Temperature Authority (GitHub #134)

- Investigated the GitHub #134 diagnostics bundle for `OMEN by HP Laptop 17-ck1xxx` / ProductId `8A18`, where CPU temperature could drop to an incorrect low/coarse WMI/ACPI reading while the worker-backed LibreHardwareMonitor reading was still realistic.
- Confirmed the failure path was not model detection: the `17-ck1xxx` worker-backed CPU temperature override activated correctly, but a temporary worker fallback timeout allowed WMI/ACPI authority to take over during the cooldown window.
- Worker-backed CPU temperature override models now keep a recent good worker reading for a bounded grace window when a worker read times out or is in cooldown, preventing bad WMI/ACPI fallback authority from replacing the trusted source.
- Added regression coverage for the last-good worker CPU temperature freshness/plausibility gate.

### OMEN 15-dc1xxx / ProductId 8574 Identity and Capability Correction

- Added an exact ProductId `8574` capability entry for OMEN 15-dc1xxx systems so Model Identity no longer resolves through low-confidence broad-family fallback.
- Set this board to an EC-first cooling profile based on field telemetry: WMI fan command path disabled, direct EC fan control retained.
- Marked direct TCC offset and direct PL1/PL2 controls unavailable for this board profile to avoid exposing non-functional controls where firmware/MSR readback reports unsupported/locked behavior.
- Added an exact keyboard model entry for `8574` as conservative backlight-only until RGB zone protocol is verified on real hardware.
- Reduced false-positive "all features available" expectations by aligning UI capability surfaces to what this board actually supports today.

### Linux OMEN 15-en0xxx / Board 8787 Support Triage

- Added board-specific Linux diagnose guidance for board `8787` / RTX 2060 reports.
- Diagnose now identifies `8787` as the legacy OMEN 15-en0xxx Ryzen + RTX 2060 generation and recommends validating the `ec_sys` path first.
- Added support-report instructions for `8787` covering DMI/baseboard data, hp-wmi tree, hwmon tree, kernel version, and NVIDIA driver details.
- Kept support conservative: RPM readback remains field-unverified, and OmenCore asks testers to report whether fans physically changed speed instead of trusting missing Linux RPM telemetry.

### Linux Victus 15-fb1xxx / Board 8C30 Support Triage

- Added board-specific Linux diagnose guidance for board `8C30` / Victus 15-fb1xxx reports where the GUI says performance mode control is unavailable.
- Diagnose now identifies the disabled System Control card as a profile-control exposure problem unless the report shows writable hp-wmi/platform_profile/EC paths.
- Added support-report instructions for `8C30` covering DMI/baseboard data, BIOS, hp-wmi tree, hwmon fan/PWM/temp paths, and relevant hp-wmi/platform_profile kernel logs.
- Kept support conservative: no changelog claim is made that performance profile writes work on `8C30` until a real diagnose bundle proves the board/kernel exposes a writable control path.

### Victus 15 / RTX 5060 - GPU Boost Extended Mode Discoverability

- Reworded the **Extended** GPU Power Boost description from "For RTX 5080/newer GPUs that support +25W or more" to "Try if Maximum doesn't reach your GPU's rated TGP (RTX 50-series and up)".
- The old wording implied Extended was 5080-exclusive; RTX 5060 and other RTX 50-series users on Victus 15 2025 hardware were not trying it even though the PPAB+ payload is exposed by firmware on those machines.
- No capability-gate or payload change - Extended still maps to `GpuPowerLevel.Extended3` (`customTgp=1, ppab=2`); only the label is corrected.

### Victus 16-s0xxx AMD / RTX 3050 Laptop GPU Power-Limit Report

- Investigated the 2026-05-21 field log for a Victus 16-s0xxx AMD system with an NVIDIA GeForce RTX 3050 6GB Laptop GPU.
- Confirmed GPU Power Boost is intentionally unavailable on this Victus path because HP WMI TGP/PPAB control is not exposed for the model.
- Fixed the NVIDIA Tuning page exposing/applying NVAPI power-limit writes through the wrong capability path:
  - NvAPIWrapper power-policy writes are now attempted when the wrapper reports writable power-policy entries.
  - Legacy NVAPI power writes remain supported as a fallback.
  - Failed or unavailable power writes no longer update OmenCore's internal confirmed power-limit value.
  - If the driver/firmware rejects power writes, the UI marks Power Limit as locked and stops presenting the slider as an active control.
- Preserved working core/memory clock OC behavior when only power-limit control is locked, avoiding misleading `Partial (power failed)` results for supported clock changes.
- Extended the NVIDIA memory/VRAM offset guardrail ceiling from `+500 MHz` laptop default / `+1500 MHz` desktop default to `+2000 MHz`, matching the requested Afterburner-style headroom while still treating high offsets as experimental.
- Improved Victus fan-control failure UX: when fan writes/backend are unavailable (including Max fan hotkey/tray apply failures), OmenCore now surfaces Victus-specific guidance to run Diagnostics and submit evidence in the support-guide format instead of only showing a generic backend warning.

---

## Internal Fixes and Hardening

### 8BCD Capability Truth Model

- ProductId `8BCD` is now pinned to the conservative WMI V1 fan-control profile:
  - direct EC fan writes disabled pending register-layout validation
  - independent fan curves disabled pending validation
  - WMI fan curves and 55-level max fan scale retained
- Added regression coverage so future database edits do not accidentally re-enable direct EC or independent curve assumptions for this board.

### OMEN 16-n0xxx / ProductId 8A43 Fan and Telemetry Reliability Hardening

- Updated the exact ProductId `8A43` capability profile to use `MaxFanLevel = 60` so diagnostics and verification mapping reflect observed field behavior instead of a stale 55-level ceiling assumption.
- Hardened guided fan verification evidence policy: medium/high target checks no longer pass on level-only readback when RPM response remains materially below expected, reducing false-positive "applied" outcomes in diagnostics.
- Added short-window last-good CPU/GPU temperature reuse in thermal sampling so transient read timeouts do not collapse fan-control inputs to `0C` and trigger misleading low-demand fan decisions.
- Added CPU temperature authority return hysteresis in monitor arbitration: primary WMI authority is restored only after consecutive healthy primary readings, reducing fallback/primary flapping in unstable telemetry windows.

### Runtime UI and Monitoring Direction

- Kept the 3.7.0 UI performance audit as the active architecture reference for dispatcher pressure, projection throttling, hidden-surface dormancy, tray/popup render dedupe, and diagnostic runtime counters.
- Deferred STEP-09 monitoring dispatch lock removal until it can be validated with a real 60-second OMEN hardware chart observation, matching the existing release verification guidance.
- Reduced dashboard binding churn in low-overhead/graph-hidden mode by suppressing RAM sparkline notifications when historical graph projection is dormant.
- Reconnected dashboard fan telemetry to the live `FanService` instance so the dashboard fan strip, fan curve summary, and visual state use real fan data instead of remaining degraded.
- Dashboard fan readouts now subscribe to live fan telemetry collection and RPM property changes, then coalesce UI refreshes so fan labels can update without waiting for the next monitoring sample.
- Replaced dashboard power/fan summary chips that parsed formatted strings through XAML converters with direct view-model display properties.
- Connected the dashboard Refresh button to a lightweight dashboard refresh command and clarified its tooltip.
- Replaced several dashboard/sidebar emoji and encoded metric glyphs with existing vector icons and added direct temperature display properties for CPU, GPU, and SSD chips.
- Dashboard temperature chips now distinguish inactive GPU state from unavailable telemetry while keeping stale readings visibly marked.
- Dashboard telemetry projection now has a headless/test fallback when no WPF dispatcher exists, preventing queued samples from leaving the UI coalescing gate stuck closed in diagnostic or no-application contexts.
- Fan telemetry updates now preserve stable `FanTelemetry` objects when the fan count is unchanged, reducing `ObservableCollection` reset/add/remove churn on the dashboard and fan-control surfaces.
- `FanTelemetry.Name` now raises `PropertyChanged`, matching the dashboard's live fan-label subscription path.
- Runtime performance diagnostics now export fan telemetry sync counters, collection resize counts, item update counts, property-only sync counts, and resize/property-only ratios so field bundles can verify that fan readback is not rebuilding UI collections every poll.
- Bounded runtime performance snapshots now include fan telemetry resize/property-only ratios and window deltas for fan telemetry sync, resize, item-update, and property-only counts.
- General profile selection borders now use a cached brush instead of allocating a `BrushConverter` result from every getter call.
- Corrected the Fan page Extreme preset label from `@88C` to `@75C` so the visible card matches the restored 3.7.0 fan curve endpoint.

### Telemetry Truthfulness and Backend Health Scaffolding

- Added additive telemetry reliability scaffolding for CPU metrics by introducing structured `TelemetryValue<T>` projection for CPU temperature and CPU power through `MonitoringTelemetryAdapter`.
- `MonitoringSample` now carries `CpuTemperatureTelemetry` and `CpuPowerTelemetry` envelopes while preserving existing scalar/state fields for backward compatibility.
- OSD stale-data behavior no longer infers synthetic CPU/GPU load from temperature-only data, avoiding misleading utilization output during partial telemetry failure.
- Dashboard total power now uses measured sensor aggregation only; synthetic estimated fallback math was removed from dashboard projection.
- Added structured backend provider health model (`BackendStatus`) with capability tags for `Telemetry`, `FanControl`, `PerformanceProfiles`, `Undervolt`, and `ECAccess`.
- Capability detection now publishes provider health snapshots for WMI, PawnIO, NVAPI, and retired WinRing0 including availability, health, reason, recommended action, and timestamp.
- Added structured backend logging for initialization failures, availability transitions, and required capability loss detection without changing backend execution behavior.
- `DeviceCapabilities` now exposes critical-vs-optional backend degradation projection and summary text for diagnostics-first readiness reporting.
- Standalone dependency audit now includes explicit degradation classification (`Critical`, `Optional`, `None`) and summary language aligned to required-vs-optional loss semantics.
- Secure Boot guidance relevance was tightened so PawnIO health drives warning projection and legacy-backend concern is suppressed when PawnIO is healthy.

### Intel Core Ultra CPU Temperature Sensor Selection

- Extended the preferred-sensor name list in `GetBestCpuTemperature` to include `"IA Cores"`, which is how LibreHardwareMonitor names the P-core cluster temperature sensor on Intel Core Ultra (Meteor Lake / Arrow Lake) platforms.
- Replaced the Intel fallback path from `tempSensors.Max()` (hottest single core) to a package-hint-weighted fallback: any sensor whose name contains `"Package"`, `"Core Max"`, or `"IA Cores"` is preferred over raw per-core maximums before falling back to `Max()` as a last resort.
- The old `Max()` fallback was the cause of transient 90–105 °C single-poll display spikes on Intel Core Ultra laptops at workload launch, where one core turbos briefly before the package settles.

### Dashboard Temperature Display Smoothing

- CPU and GPU temperature values displayed in the hardware monitoring dashboard are now computed as a 3-sample trailing average of the last three polling samples (`_cpuTempHistory.TakeLast(3).Average()` / `_gpuTempHistory.TakeLast(3).Average()`).
- Smoothing applies only to the displayed number — the existing history queues were already populated each poll cycle. Alert animations (`AnimateMetricIfCritical`), status colors (`GetTemperatureBrush`), and status labels (`GetTemperatureStatus`) continue to use the raw current value so real thermal warnings are not delayed.
- Fan curve control uses its own independent smoothing in `FanCurveEngine` and is unaffected.
- Addresses the field pattern of displayed CPU temps spiking to 85–90 °C at game launch and recovering within 5 seconds — the spike is real but the display lag misled users into thinking OmenCore was reading incorrectly.

### Legacy WinRing0 Removal

- Removed the remaining WinRing0 EC backend implementation and its allowlist tests.
- Removed the optional `OMENCORE_ENABLE_WINRING0` EC fallback path from `EcAccessFactory`; direct EC access is now PawnIO-only.
- Removed WinRing0 capability promotion from runtime detection so fan control and undervolt paths no longer select WinRing0 as a fallback backend.
- Removed stale WinRing0 setup/stub documentation and refreshed antivirus guidance to explain that WinRing0 alerts now point to leftover files or other tools, not the current OmenCore EC backend.
- Updated Settings, diagnostics, fan-cleaning, fan-controller, and cross-platform settings text so supported low-level access points users at PawnIO.
- Final sweep removed the orphaned legacy dependency-audit helper from `SystemInfoService`, updated README driver priority/troubleshooting/licensing text to the PawnIO-only direction, and cleaned the single-curve fan application guard for clearer control flow.

### Linux Fan Curve Safety and Accuracy

- Linux fan curves no longer treat missing CPU/GPU temperature telemetry as `0C`; the daemon preserves the last fan target until a plausible sensor reading returns.
- Linux telemetry now filters implausible sysfs/EC temperature readings outside the 1C-125C range before fan decisions use them.
- Linux fan smoothing is bypassed at 90C and above so critical thermal events can jump directly to 100% fan instead of stepping upward over several polling cycles.
- Linux performance and fan-profile writes now use the centralized hp-wmi profile path map, covering `thermal_profile`, `platform_profile`, and `performance_profile` naming variants.
- Linux profile writes now consult available `*_profile_choices` files before writing and resolve common aliases such as `quiet`/`low-power`, `performance`/`balanced-performance`, and `max`/`extreme`.
- Linux fan-profile writes now fall through to ACPI/hwmon fallback when an hp-wmi profile path is present but rejects a requested value, instead of stopping after the first failed backend.
- Linux `perf` output now reports the backend used for performance mode application/readback, making hp-wmi, ACPI platform-profile, and legacy `ec_sys` routing visible in field reports.
- Linux `perf --mode balanced` now treats `default`/`balanced` readback as equivalent, matching daemon behavior and avoiding false warning output on kernels that expose the balanced policy as `default`.
- Linux diagnose human-readable output now defaults to ASCII-safe table rendering to avoid corrupted box-drawing/status glyphs in terminals and copied Discord logs.
- Linux diagnose now emits a dedicated kernel-issue hint when boot logs include hp-wmi `WQ00` missing-method firmware warnings, and recommends attaching full kernel logs plus ACPI tables for upstream triage.
- Added board-specific Linux diagnose guidance for OMEN 16-wf1xxx board `8C77` (Insyde BIOS reports), including explicit evidence capture commands and conservative messaging that board-list membership does not guarantee effective profile-power control.

---

## Profile-Oriented Architecture (v3.7.0)

v3.7.0 makes profiles the primary control concept in OmenCore rather than a derived label on top of independent fan/performance state.

### Four Explicit Profiles

| Profile | Fan Curve Endpoint | Power Mode |
|---|---|---|
| Performance | 100% at 75 °C | Performance |
| Balanced | 100% at 75 °C | Balanced |
| Quiet | 85% at 80 °C (capped — does not ramp further) | Quiet |
| Custom | User-defined fan curve + user-selected power mode | User-set |

- Quiet fan curve updated: ramp stops at 85% fan speed at 80 °C and does not increase further under normal conditions, reducing noise in light/medium workloads.
- Custom profile is now a full first-class profile rather than a label; selecting it navigates directly to the Custom tab so the user can configure their fan curve and power settings.

### Quiet Thermal Safety Override

When the Quiet profile is active, a background `QuietSafetyMonitor` service watches CPU and GPU temperatures and automatically intervenes if thermals reach dangerous levels:

- **Safety-on threshold** (default 90 °C): when `max(cpuTemp, gpuTemp)` crosses this value, OmenCore switches to Max fan cooling while keeping the Quiet power mode.
- **Safety-off threshold** (default 70 °C): once temperature drops back below this value, Max fan cooling is released and Quiet fan mode is restored.
- The transition is hysteretic — the override does not oscillate on or off near the threshold.
- Both thresholds are user-configurable via `AppConfig.QuietSafety` (`SafetyOnTempC`, `SafetyOffTempC`).
- The override can be disabled entirely via `AppConfig.QuietSafety.Enabled = false`.
- A yellow warning banner reading **THERMAL SAFETY ACTIVE** appears in the General tab profile card area while the override is engaged.
- The monitor disarms automatically when the user switches away from Quiet profile; no safety event fires when explicitly changing profiles.

### UI and Tray Changes

- Renamed the **OMEN** tab to **Custom** throughout the application, matching its role as the user-defined profile configuration surface.
- Added **Custom** to the tray icon Quick Profile submenu (alongside Performance, Balanced, Quiet).
- General tab profile card tooltip updated: "OMEN Tab" → "Custom Tab".

### New Configuration Surface

```json
"QuietSafety": {
  "Enabled": true,
  "SafetyOnTempC": 90.0,
  "SafetyOffTempC": 70.0
}
```

---

## Enhancements and Additions

- RGB tab now supports per-card control toggles directly in card headers for:
  - Scene Quick Select
  - Corsair Devices
  - Logitech Devices
  - Razer Devices
  - HP OMEN Keyboard
- Toggling a card off now hides that card's inner controls in-place while keeping the card header visible for quick re-enable.
- Card toggle state is persisted across restarts.
- Corsair/Logitech/Razer/Keyboard card toggles are wired to the existing feature preference state so RGB-tab control and Settings stay consistent.

- Added a dedicated General profile-cycle hotkey path (`Ctrl+Shift+E`) separate from performance-mode cycling.
- Added profile-cycle resolver coverage for Custom-present and Custom-absent cases.
- Added default hotkey registration coverage for `Ctrl+Shift+E`.
- Added regression coverage ensuring stable fan telemetry updates mutate existing fan items in place instead of rebuilding the collection.
- Added runtime counter coverage for fan telemetry collection resize/property-only sync ratios.
- Added diagnostic export coverage for fan telemetry churn counters in runtime performance snapshots.
- Removed obsolete WinRing0 EC backend source/tests and refreshed Defender/antivirus documentation for the PawnIO-only direction.
- Fixed NVIDIA power-limit capability detection so Tuning distinguishes clock OC availability from writable NVAPI power-policy availability.
- Added driver/firmware-locked power-limit messaging to both Tuning and System Control views.
- Extended NVIDIA VRAM offset UI/config guardrails to `+2000 MHz`.
- Extracted hotkey profile-cycle resolution into `ProfileCycleService` with direct coverage for standard and Custom-present cycles, reducing `MainViewModel` ownership.
- Hardened profile-cycle fan-mode lookup to be case-insensitive before resolving the next hotkey target.
- Removed tracked one-off build/test output files, the stray root `query` file, and the obsolete comment-only Corsair DPI tombstone file.
- Added ignore rules for root build/test output files and one-off OSD lifecycle captures.
- Expanded model capability tests for the OMEN 16-xd0xxx `8BCD` report.
- Expanded Linux support documentation for board `8787` / RTX 2060 field triage.
- Expanded Linux backend compatibility for hp-wmi boards that expose `platform_profile` or `performance_profile` instead of the older `thermal_profile` file.
- Added [3.7.0 code audit](3.7.0-CODE-AUDIT.md) covering runtime architecture, optimization targets, and redundant/obsolete code candidates.
- Corrected **Extended** GPU Power Boost description to remove RTX 5080-exclusive wording; RTX 50-series users are now prompted to try Extended if Maximum doesn't reach rated TGP.
- Added `"IA Cores"` to the Intel CPU temperature preferred-sensor list (covers Intel Core Ultra P-core cluster in LibreHardwareMonitor).
- Improved Intel CPU temperature fallback to prefer package-hinting sensors over raw per-core `Max()`, reducing transient display spikes on Meteor Lake / Arrow Lake platforms.
- Added 3-sample trailing-average smoothing for displayed CPU and GPU temperatures in the hardware monitoring dashboard.

---

## Diagnostics and Testing

Focused regression coverage added or updated:

- `FanSmoothingTests`
- `FanControlViewModelTests`
- `GeneralViewModelTests`
- `PowerAutomationServiceTests`
- `HotkeyAndMonitoringTests`
- `MainViewModelTests`
- `HotkeyServiceTests`
- `ModelCapabilityDatabaseTests`
- `KeyboardModelDatabaseTests`
- `ModelIdentityResolutionSummaryTests`
- `PerformanceModeServiceTdpOverrideTests`
- `DashboardViewModelTests`
- `MonitoringTelemetryAdapterTests`
- `MonitoringSampleCopyConstructorTests`
- `DeviceCapabilitiesTests`
- `DependencyAuditTests`

RC field sign-off checklist: [3.7.0-RC-VALIDATION.md](3.7.0-RC-VALIDATION.md)

Current validation matrix:

| Area | Command | Result |
| --- | --- | --- |
| Windows WPF app | `dotnet build src\OmenCoreApp\OmenCoreApp.csproj --no-restore` | Passed, 0 warnings, 0 errors after allowing WPF generated-file writes; rerun passed after final profile-cycle hardening. |
| Linux CLI | `dotnet build src\OmenCore.Linux\OmenCore.Linux.csproj --no-restore -p:IntermediateOutputPath=artifacts\obj\OmenCoreLinux\Debug\ -p:OutputPath=artifacts\bin\OmenCoreLinux\Debug\` | Passed, 0 warnings, 0 errors. |
| Avalonia GUI | `dotnet build src\OmenCore.Avalonia\OmenCore.Avalonia.csproj --no-restore -p:IntermediateOutputPath=artifacts\obj\OmenCoreAvalonia\Debug\ -p:OutputPath=artifacts\bin\OmenCoreAvalonia\Debug\` | Passed, 0 warnings, 0 errors. |
| Dashboard projection | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "DashboardViewModelTests"` | Passed, 4 passed, 0 failed, 0 skipped. |
| Fan smoothing/service | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "FanSmoothingTests"` | Passed, 13 passed, 0 failed, 0 skipped. |
| Runtime diagnostics/counters | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "RuntimeUiPerformanceCountersTests|DiagnosticExportSnapshotTests"` | Passed, 17 passed, 0 failed, 0 skipped. |
| Model/identity/power-mode regression | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "ModelCapabilityDatabaseTests|KeyboardModelDatabaseTests|ModelIdentityResolutionSummaryTests|PerformanceModeServiceTdpOverrideTests"` | Passed after `8D41` updates, 79 passed, 0 failed, 0 skipped after allowing WPF generated-file writes. |
| Fan page profile curves | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "FanControlViewModelTests.BuiltInFanCurves_MaxOutAtFieldVerifiedHighTemps"` | Passed, 1 passed, 0 failed, 0 skipped after Quiet curve/changelog alignment. |
| NVIDIA tuning status | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "TuningStatusFormatterTests|DashboardViewModelTests"` | Passed, 7 passed, 0 failed, 0 skipped. |
| Profile-cycle extraction | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "ProfileCycleServiceTests|MainViewModelTests"` | Passed, 29 passed, 0 failed, 0 skipped after allowing WPF generated-file writes; rerun passed after final case-insensitive fan-mode hardening. |
| Telemetry/backend scaffolding | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "MonitoringTelemetryAdapterTests|MonitoringSampleCopyConstructorTests|DeviceCapabilitiesTests|DependencyAuditTests"` | Passed, focused suites green after telemetry envelope and backend-health scaffolding changes. |
| 8A43 fan/telemetry hardening | Focused `ModelCapabilityDatabaseTests`, `FanVerificationServiceTests`, and `WmiBiosMonitorTests` | Passed, 50 passed, 0 failed after applying stricter verification evidence policy and authority recovery hysteresis. |
| Full Windows test project | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --logger "console;verbosity=normal"` | Passed before the late GitHub #134 patch, 743 passed, 0 failed. Full rerun required before upload. |
| GitHub #134 CPU temperature fallback | `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "WmiBiosMonitorFallbackTests|ModelIdentityResolutionSummaryTests.Build_8D41ExactProductId_ResolvesKeyboardProfile"` | Passed after the worker last-good CPU temperature fix, 13 passed, 0 failed. |
| Windows prerelease package | `powershell -NoProfile -ExecutionPolicy Bypass -File .\build-installer.ps1` | Passed before the late GitHub #134 patch. Produced superseded `OmenCoreSetup-3.7.0.exe` and `OmenCore-3.7.0-win-x64.zip`; rebuild required before upload. |
| Linux prerelease package | `powershell -NoProfile -ExecutionPolicy Bypass -File .\build-linux-package.ps1 -SkipBinaryVersionCheck` | Passed before the late GitHub #134 patch after excluding Avalonia `artifacts\**` from source compilation and adding publish exit-code checks. Produced superseded `OmenCore-3.7.0-linux-x64.zip`; rebuild required before upload. |
| Release hygiene | `rg -n "catch\s*\{\s*\}" ...` over the files reported by the release-gate failure | Passed after patching: no bare `catch {}` remains in the reported files. |
| Release hygiene | `rg -n "3\.6\.3|3\.6\.3\.0|OmenCoreSetup-3\.6\.3|OmenCore-3\.6\.3" ...` over active version/package surfaces | Passed for release-facing metadata; remaining `3.6.3` matches are intentional historical changelog/base-version references. |
| Final Linux rebuild | `dotnet build src\OmenCore.Linux\OmenCore.Linux.csproj --no-restore -p:IntermediateOutputPath=artifacts\obj\OmenCoreLinux\Debug\ -p:OutputPath=artifacts\bin\OmenCoreLinux\Debug\` | Passed, 0 warnings, 0 errors. |
| Release hygiene | `git diff --check` | Line-ending warnings only. |

Superseded prerelease artifacts:

These hashes describe the earlier 3.7.0 candidate build and must be regenerated after the GitHub #134 CPU-temperature authority fix.

| File | Size | SHA256 |
| --- | ---: | --- |
| `OmenCoreSetup-3.7.0.exe` | 101.72 MB | `AD47AEC9E4D0CD894672B822B5F01337030372F5F7EC42A5F6298B955EAA8101` |
| `OmenCore-3.7.0-win-x64.zip` | 104.88 MB | `682D914A6234900B530A9E1B27B016883D1E0FAAD0BC9A3B34466AB3C0CD5090` |
| `OmenCore-3.7.0-linux-x64.zip` | 43.56 MB | `6A66450F97AF3E1F678FA70D0B226613D72500BBC49BF9D6946159660D652697` |

Known validation limits:

- Non-elevated WPF builds/tests can hit access-denied writes under generated `obj` files in this workspace.
- App/package version metadata has been bumped to `3.7.0` across `VERSION.txt`, shipping project files, installer defaults, and active README/INSTALL release references.
- Physical hardware behavior still needs the sign-off items below.

Pending validation before release sign-off:

- Physical OMEN 16-xd0xxx / ProductId `8BCD` fan RPM behavior under sustained load.
- Physical OMEN 16-am0xxx / ProductId `8D2F` confirmation that Performance mode applies through the WMI policy path when direct CPU PL1/PL2 limits are unavailable.
- Physical OMEN MAX 16-ah0xxx / ProductId `8D41` confirmation that Quick Profiles now hold >100W GPU behavior through the WMI thermal-policy fallback without requiring a manual Max fan toggle.
- Physical OMEN MAX 16-ah0xxx / ProductId `8D41` confirmation of HID per-key RGB behavior; WMI ColorTable remains the fallback until verified.
- Physical Victus 16-s0xxx / RTX 3050 confirmation that the NVIDIA Tuning page now shows Power Limit as locked when NVAPI/firmware rejects power-policy writes, while core/memory OC still applies.
- Physical OMEN 17-ck1xxx / ProductId `8A18` confirmation that CPU temperature stays on worker-backed authority after fallback timeout/cooldown events (GitHub #134).
- Linux OMEN 15-en0xxx / board `8787` confirmation of `ec_sys` or hp-wmi control exposure, physical fan response, and RPM readback behavior.
- Linux Victus 15-fb1xxx / board `8C30` diagnose bundle showing whether any hp-wmi/platform_profile/EC control path is exposed on Linux Mint 22.3 / Ubuntu 24.04 kernel 6.14.
- Focused-window idle CPU/GPU usage with the main window open for at least 5 minutes.
- UI frame pacing and dispatcher-depth measurements from an interactive Windows session, with GitHub #133 treated as mitigated by 3.7.0 projection throttling/instrumentation but not closed until a reporter confirms the prerelease.
- Tray/minimized cadence measurement evidence.
