# OmenCore v3.6.3 - Safety and Stability Hotfix Rollup

**Release Date:** 2026-05-19
**Release Status:** In Progress
**Type:** Hotfix release
**Base Version:** v3.6.2

---

## Summary

v3.6.3 is a safety-first hotfix release for post-v3.6.2 field reports. It focuses on desktop fan-control risk, legacy model stability, Transcend hotkey behavior, WPF UI pressure, baseline CPU/RAM overhead, OMEN Max per-key RGB capability detection, and a lighter main-shell visual refresh.

This changelog is updated continuously as fixes land.

---

## Community Reports Addressed

- **GitHub #131:** OMEN 45L desktop fan control could leave desktop fan state inconsistent after Performance mode and reboot.
- **GitHub #132:** HP OMEN 15z-en100 / ProductId `88D2` used low-confidence legacy fallback and showed fan hunting across profiles.
- **Discord:** OMEN Transcend 14-fb1xxx startup fan state, ineffective curve UI, Fn+P profile cycling, and Fn+F12 launch failures.
- **Discord:** OMEN 15 dh1xxxx severe main-window lag while tray actions remained responsive.
- **Discord:** OMEN Max per-key RGB-capable keyboard shown as unsupported.
- **Discord:** Victus 16-s0xxx / ProductId `8BD4` startup fans could remain stopped until thermal emergency, and custom fan presets were rejected by unsupported EC readback.
- **Discord:** OMEN 16-am0xxx / ProductId `8D2F` reported as AMD on an Intel Core Ultra + RTX 5070 model, with confusing direct CPU PL1/PL2 status.
- **Discord:** OMEN 16-xd0xxx / ProductId `8BCD` v3.6.3 reports covered hot-window CPU temperature rise, abrupt temperature authority jumps, fan presets not reaching max at high temperatures, Max/Extreme/Gaming/Auto/Quiet profile lag or stuck RPM, General profile fan/profile sync drift, and a request for a profile-cycle hotkey.
- **Reddit:** OMEN Max 16 / RTX 5080 felt sluggish on a 4K external monitor and could not match OGH Unleashed tuning behavior.

---

## Fixed

### 1. Desktop Fan Writes Disabled by Default for OMEN 25L/30L/35L/40L/45L
- **Issue:** Desktop OMEN systems could be routed through laptop-oriented WMI fan write paths, creating risk of persistent or inconsistent fan ownership after mode changes.
- **Root Cause:** Desktop model profiles advertised writable WMI fan control and fan curves before the desktop fan topology had enough validation.
- **Fix Deployed:**
  - Desktop profiles now disable fan writes and fan curves while preserving RPM telemetry and supported performance controls.
  - Capability detection forces desktop systems into monitoring-only fan control.
  - Fan controller creation skips writable backends when the desktop safety gate is active.
  - FanService blocks manual speed, preset, curve, auto/quiet/max, restore-auto, reset, and shutdown fan-write paths when desktop safety gating is active.
  - Performance-mode linked fan policy and legacy WMI thermal-policy fallback now respect model fan-control blocks.
  - Fan calibration and closed-loop verification now stop cleanly when fan writes are disabled by the active safety profile.
  - Main startup warning now clearly states that desktop fan writes are disabled in v3.6.3 safety mode.
- **Files:** `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs`, `src/OmenCoreApp/Hardware/DeviceCapabilities.cs`, `src/OmenCoreApp/Hardware/CapabilityDetectionService.cs`, `src/OmenCoreApp/Hardware/FanControllerFactory.cs`, `src/OmenCoreApp/Services/FanService.cs`, `src/OmenCoreApp/Services/PerformanceModeService.cs`, `src/OmenCoreApp/Services/FanCalibration/FanCalibrationService.cs`, `src/OmenCoreApp/ViewModels/MainViewModel.cs`
- **Status:** Fixed in software; physical OMEN 45L validation still required before re-enabling any desktop fan write path.

### 2. HP OMEN 15z-en100 / ProductId 88D2 Gets an Exact Conservative Profile
- **Issue:** ProductId `88D2` resolved through broad legacy fallback, which exposed unstable defaults for a system reporting fan hunting.
- **Root Cause:** The model database lacked an exact 15z-en100 entry, and unverified legacy profiles used the same curve write cadence as validated models.
- **Fix Deployed:**
  - Added exact ProductId `88D2` profile for `OMEN by HP Laptop 15z-en100`.
  - Uses conservative legacy WMI V1 assumptions.
  - Direct EC writes, independent curves, GPU power boost, and undervolt are disabled pending field verification.
  - Max fan level is constrained to the legacy 55 scale.
  - Added conservative curve write policy for `88D2` / unverified legacy WMI profiles:
    - Tiny curve target deltas below 5% are suppressed.
    - Non-forced curve writes observe a 10-second dwell window.
    - Curve force-refresh cadence is relaxed to 60 seconds.
- **Files:** `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs`, `src/OmenCoreApp/Services/FanService.cs`
- **Status:** Partially fixed; field telemetry still needed to validate the anti-hunting thresholds.

### 3. Transcend Fn+F12 Dedicated OMEN Launch Signature No Longer Blocked as Plain F12
- **Issue:** Fn+F12 launch behavior could be filtered as a generic function key before OMEN-key handling completed.
- **Root Cause:** The never-intercept function-key guard treated F12 broadly, even when paired with the dedicated OMEN launch scan code.
- **Fix Deployed:**
  - Fn+F12 with the dedicated OMEN launch scan is exempted from the generic function-key never-intercept rule.
  - Plain F12 remains protected and is not intercepted.
  - Brightness/lock-key conflict filters remain intact.
- **File:** `src/OmenCoreApp/Services/OmenKeyService.cs`
- **Status:** Fixed for the known dedicated scan signature; Fn+P firmware event capture still needs field validation.

### 4. Dashboard UI Projection Pressure Reduced Under Telemetry Load
- **Issue:** Some systems showed severe main-window lag while tray controls stayed responsive, suggesting WPF dispatcher pressure in the full UI path.
- **Root Cause:** Dashboard telemetry projection could still be too chatty under constrained CPU or backlog conditions.
- **Fix Deployed:**
  - Increased dashboard UI projection minimum interval from 750 ms to 1000 ms.
  - Pending dashboard projections now record skipped/replaced samples, improving runtime UI pressure diagnostics.
- **File:** `src/OmenCoreApp/ViewModels/DashboardViewModel.cs`
- **Status:** Partially fixed; manual lag reproduction with runtime UI counters remains a release gate.

### 5. OMEN MAX 16 ak0003nr Marked Per-Key RGB Capable
- **Issue:** OMEN Max per-key-capable hardware could be shown as unsupported.
- **Root Cause:** The main model capability database had the `ak0003nr` OMEN Max profile marked as four-zone RGB without also marking per-key RGB capability.
- **Fix Deployed:**
  - `OMEN MAX 16 ak0003nr` now sets `HasPerKeyRgb = true`.
  - Keyboard model-name resolution is covered by regression tests and resolves OMEN Max ak0003nr to the HID per-key profile.
- **Files:** `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs`, `src/OmenCoreApp/Services/KeyboardLighting/KeyboardModelDatabase.cs`
- **Status:** Capability detection fixed; full per-key editing still depends on backend support.

### 6. Runtime CPU/RAM Overhead Reduced While OmenCore Is Running
- **Issue:** Dashboard and optional ambient RGB paths could continue doing more background work than needed while the UI was dormant or while high-rate timers were active.
- **Root Cause:** Dashboard metric history retained more samples than the current UI needs, chart queries returned every raw point, per-sample debug strings were built in the monitor loop, and ambient screen sampling allowed overlapping timer callbacks.
- **Fix Deployed:**
  - Dashboard metric history is capped to a two-hour active-cadence window instead of the larger legacy buffer.
  - Dashboard metric age pruning now runs at a bounded cadence instead of on every telemetry sample.
  - Historical chart queries downsample large result sets to keep chart redraw allocations bounded.
  - Per-sample dashboard debug log messages now avoid formatting work unless debug logging is enabled.
  - Dashboard uptime/last-sample dispatcher timer stops while telemetry projection is dormant and restarts when visible projection resumes.
  - Ambient screen sampling now prevents overlapping capture and RGB apply work when timer callbacks or device writes run long.
- **Files:** `src/OmenCoreApp/Services/HardwareMonitoringService.cs`, `src/OmenCoreApp/ViewModels/DashboardViewModel.cs`, `src/OmenCoreApp/Services/ScreenSamplingService.cs`
- **Status:** Fixed in software; needs normal runtime observation on systems that previously reported main-window lag.

### 7. Conservative WMI Fan Handoff for Victus and Unverified WMI-Only Profiles
- **Issue:** Victus 16-s0xxx logs showed WMI fan commands returning success while EC fan-mode readback stayed at an unrelated value, causing presets to be rejected. The same startup logs also showed manual `SetFanLevel(0,0)` handoff writes before the user reported fans only waking during thermal emergency.
- **Root Cause:** The WMI fan controller treated EC register `0x95` as a strict verification source even on profiles where direct EC fan control is disabled. V1 auto handoff also still used a manual-zero floor clear that is risky on unverified Victus/WMI-only firmware.
- **Fix Deployed:**
  - WMI-only or unverified model profiles now use non-strict EC fan-mode readback verification.
  - Conservative WMI profiles skip V1 `SetFanLevel(0,0)` auto floor clear to avoid manual-zero fan stops.
  - Transition hints such as `SetFanLevel(20,20)` are retained where already used to help V1 firmware leave Performance/Max state without forcing a zero level.
- **Files:** `src/OmenCoreApp/Hardware/WmiFanController.cs`, `src/OmenCoreApp/Hardware/FanControllerFactory.cs`
- **Status:** Fixed in software for conservative profiles; Victus 16-s0xxx still needs field confirmation after boot and after long gaming sessions.

### 8. OMEN 16-am0xxx ProductId 8D2F Label Made Neutral
- **Issue:** ProductId `8D2F` was shown as `OMEN 16 (2024) am0xxx AMD` on a reported Intel Core Ultra 7-255H + RTX 5070 OMEN 16-am0xxx.
- **Root Cause:** HP appears to reuse the same ProductId across am0xxx variants, and the existing exact ProductId label was too specific.
- **Fix Deployed:**
  - ProductId `8D2F` now resolves to a neutral shared AMD/Intel am0xxx profile.
  - Direct EC fan control and independent fan curves remain disabled for this unverified shared profile.
  - CPU PL1/PL2 UI now treats `0W/0W` MSR readback as unavailable/firmware-controlled instead of presenting it as a writable BIOS-locked limit.
  - Undervolt verification warnings now use backend-neutral MSR wording instead of stale WinRing0-specific text.
- **Files:** `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs`, `src/OmenCoreApp/Services/KeyboardLighting/KeyboardModelDatabase.cs`, `src/OmenCoreApp/ViewModels/SystemControlViewModel.cs`, `src/OmenCoreApp/Hardware/CpuUndervoltProvider.cs`
- **Status:** Identity label fixed; OGH-equivalent Unleashed behavior remains a separate tuning capability gap.

### 9. OSD Initialization Hardened
- **Issue:** A user reported OSD only becoming active after visiting settings instead of reliably after boot.
- **Root Cause:** OSD initialization was tied to hotkey/window initialization, so a startup-minimized window or unrelated hotkey registration failure could leave the overlay uncreated until settings touched it.
- **Fix Deployed:**
  - OSD initialization is now idempotent.
  - Showing the OSD lazily initializes the overlay if it was enabled but not yet created.
  - Main window startup now initializes OSD separately before registering the broader hotkey set.
  - The hotkey path still calls the same idempotent initializer, keeping normal and startup-minimized launches aligned.
- **Files:** `src/OmenCoreApp/Services/OsdService.cs`, `src/OmenCoreApp/ViewModels/MainViewModel.cs`, `src/OmenCoreApp/Views/MainWindow.xaml.cs`
- **Status:** Fixed in software; startup-minimized field validation still recommended.

### 10. Main Shell Visual Polish and Log Rendering Optimization
- **Issue:** The main tab strip was text-heavy and visually flatter than the rest of the shell, while the footer log view could keep pushing dispatcher work during log bursts.
- **Root Cause:** The shell relied on plain text tab headers, decoded the large app logo bitmap for tiny placements, rebuilt/split the whole UI log buffer for every emitted line, and eagerly built the heavy monitoring dashboard on window load.
- **Fix Deployed:**
  - Main tabs now use consistent vector icons, tighter active/hover states, and a contained header rail.
  - Shared text rendering now opts into ClearType for crisper WPF typography.
  - Small shell/about logo placements now use `logo-small.png` with decode pixel widths instead of decoding the full-size logo.
  - Footer activity/system logs are collapsed by default with a working toggle.
  - UI log text is cached from a bounded queue instead of repeatedly converting and splitting the full buffer.
  - System log auto-scroll is skipped while logs are collapsed and throttled during log bursts.
  - Monitoring dashboard construction is now lazy and only occurs when the Monitoring tab is opened.
  - Startup no longer forces immediate `ApplyTemplate()` / `UpdateLayout()` on the dashboard or tab control, reducing first-window work on high-DPI and 4K displays.
- **Files:** `src/OmenCoreApp/Styles/ModernStyles.xaml`, `src/OmenCoreApp/Views/MainWindow.xaml`, `src/OmenCoreApp/Views/MainWindow.xaml.cs`, `src/OmenCoreApp/ViewModels/MainViewModel.cs`, `src/OmenCoreApp/Views/AboutWindow.xaml`
- **Status:** Fixed in software; manual visual/performance pass still recommended on 1080p, 1440p, and 4K external monitor layouts.

### 11. Release Version Metadata Updated to 3.6.3
- **Issue:** Project metadata and public install/readme references still pointed at older release versions after the hotfix patches landed.
- **Fix Deployed:**
  - `VERSION.txt`, Windows app, hardware worker, Linux CLI, Avalonia GUI, installer defaults, README, INSTALL, and root changelog now identify the active release as `3.6.3`.
  - README artifact hashes are reset to `Pending` until final release packages are built and hashed.
- **Files:** `VERSION.txt`, `README.md`, `INSTALL.md`, `CHANGELOG.md`, `src/OmenCoreApp/OmenCoreApp.csproj`, `src/OmenCore.HardwareWorker/OmenCore.HardwareWorker.csproj`, `src/OmenCore.Linux/OmenCore.Linux.csproj`, `src/OmenCore.Avalonia/OmenCore.Avalonia.csproj`, `src/OmenCore.Desktop/OmenCore.Desktop.csproj`, `installer/OmenCoreInstaller.iss`
- **Status:** Fixed.

### 12. OMEN 16-xd0xxx Fan Curve and Profile Recovery for 3.7.0 Readiness
- **Issue:** v3.6.3 field logs from ProductId `8BCD` showed WMI V1 fan control with repeated CPU/GPU frozen-temperature recovery, abrupt fan curve target jumps, General profiles falling back to firmware-only fan modes, and built-in fan curves that no longer reached full cooling at the temperatures users expected.
- **Root Cause:** The v3.6 fan-curve rebalance pushed Auto/Gaming/Extreme full-speed endpoints too high for this cohort. General profile buttons used Auto/Quiet firmware fan modes instead of the matching curve payloads, so high-temperature max behavior depended on firmware policy. Curve control also consumed raw recovered temperatures directly, so authority changes such as `54C -> 70C` could jerk fan targets.
- **Fix Deployed:**
  - Restored aggressive high-temp endpoints:
    - Performance profile max at `70C`.
    - Auto/Balanced and Extreme max at `75C`.
    - Gaming max at `80C`.
    - Quiet max at `85C`.
  - General Performance/Balanced/Quiet profiles now apply curve-backed cooling presets with immediate curve evaluation instead of relying only on firmware fan modes.
  - Quiet profile UI sync now selects the Quiet fan preset instead of Auto.
  - Fan curve control now slew-limits warm temperature jumps before `75C`, while bypassing smoothing at hot/safety temperatures so cooling can still react immediately.
  - ProductId `8BCD` keeps non-strict EC fan-mode readback but re-enables the V1 auto-mode floor clear, matching the field evidence that the old conservative handoff could leave the fan floor stuck after Max/manual transitions.
  - Added `Ctrl+Shift+E` to cycle General profiles: Balanced -> Performance -> Quiet -> Custom.
- **Files:** `src/OmenCoreApp/Services/FanService.cs`, `src/OmenCoreApp/ViewModels/FanControlViewModel.cs`, `src/OmenCoreApp/ViewModels/GeneralViewModel.cs`, `src/OmenCoreApp/ViewModels/MainViewModel.cs`, `src/OmenCoreApp/Services/HotkeyService.cs`, `src/OmenCoreApp/Models/FanModeNameResolver.cs`, `src/OmenCoreApp/Hardware/FanControllerFactory.cs`
- **Status:** Fixed in software; needs v3.7.0 field validation on OMEN 16-xd0xxx / ProductId `8BCD`.

---

## Tests Added

- Desktop capability fan-write safety gating.
- FanService desktop write suppression across direct write/reset paths.
- PerformanceModeService desktop/model fan-policy blocking.
- FanCalibrationService desktop/model fan-write blocking.
- ProductId `88D2` conservative profile resolution.
- Conservative legacy fan policy suppresses small curve target changes without affecting non-conservative profiles.
- Desktop model profile fan-write disablement.
- Fn+F12 dedicated OMEN launch scan acceptance and plain-F12 rejection.
- OMEN Max ak0003nr model and keyboard per-key resolution.
- Dashboard metric history cap and large chart-query downsampling.
- Dashboard dormancy disables the uptime dispatcher timer.
- Startup-minimized OSD initialization is now independent from the broader hotkey registration path.
- Monitoring dashboard creation is now lazy instead of being force-rendered during main-window load.
- Conservative WMI fan handoff skips manual-zero floor clear on unverified/WMI-only profiles.
- Non-strict fan-mode readback for WMI-only profiles.
- ProductId `8D2F` shared AMD/Intel model identity regression.
- OMEN 16-xd0xxx aggressive fan-curve endpoints, curve-temperature smoothing, and General profile fan sync.
- Ctrl+Shift+E General profile-cycle hotkey registration path.

---

## Validation

- Targeted regression suite passed:
  - `ModelCapabilityDatabaseTests`
  - `DeviceCapabilitiesTests`
  - `FanServiceDesktopSafetyTests`
  - `FanCalibrationServiceSafetyTests`
  - `FanSmoothingTests`
  - `OmenKeyServiceTests`
  - `PerformanceModeServiceTdpOverrideTests`
  - `KeyboardModelDatabaseTests`
  - `WmiV2VerificationTests`
  - `HotkeyAndMonitoringTests`
  - `DashboardViewModelTests`
- Latest targeted command passed:
  - `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~WmiV2VerificationTests|FullyQualifiedName~ModelCapabilityDatabaseTests|FullyQualifiedName~KeyboardModelDatabaseTests|FullyQualifiedName~HotkeyAndMonitoringTests|FullyQualifiedName~DashboardViewModelTests|FullyQualifiedName~FanServiceDesktopSafetyTests|FullyQualifiedName~FanSmoothingTests|FullyQualifiedName~PerformanceModeServiceTdpOverrideTests|FullyQualifiedName~FanCalibrationServiceSafetyTests"`
  - Result: 160 passed, 0 failed.
- App build passed:
  - `dotnet build src\OmenCoreApp\OmenCoreApp.csproj -c Debug --no-restore`
- Version-bumped project builds passed:
  - `dotnet build src\OmenCore.HardwareWorker\OmenCore.HardwareWorker.csproj -c Debug --no-restore`
  - `dotnet build src\OmenCore.Linux\OmenCore.Linux.csproj -c Debug --no-restore`
  - `dotnet build src\OmenCore.Avalonia\OmenCore.Avalonia.csproj -c Debug --no-restore`
- UI/monitoring targeted regression passed:
  - `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~DashboardViewModelTests|FullyQualifiedName~HotkeyAndMonitoringTests|FullyQualifiedName~ModelCapabilityDatabaseTests|FullyQualifiedName~WmiV2VerificationTests"`
  - Result: 107 passed, 0 failed.
- Startup/UI hardening regression passed:
  - `dotnet build src\OmenCoreApp\OmenCoreApp.csproj -c Debug --no-restore`
  - `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~DashboardViewModelTests|FullyQualifiedName~HotkeyAndMonitoringTests|FullyQualifiedName~OsdServiceLifecycleTests"`
  - Result: 44 passed, 0 failed.
- Version metadata sweep completed:
  - Active project/readme/install/installer metadata now reports `3.6.3`; older release references remain only as historical changelog/history links.
- 3.7.0 readiness fan/profile regression passed:
  - `dotnet test src\OmenCoreApp.Tests\OmenCoreApp.Tests.csproj --no-restore --filter "FanSmoothingTests|FanControlViewModelTests|GeneralViewModelTests|PowerAutomationServiceTests|HotkeyAndMonitoringTests"`
  - Result: 71 passed, 0 failed.

---

## Known Issues / Open Risks

### OMEN 45L Desktop Fan Control
- **Status:** Fan writes disabled by default.
- **Risk:** Desktop fan write paths must not be re-enabled until validated on physical 45L-class hardware.

### HP OMEN 15z-en100 Fan Hunting
- **Status:** Exact model profile added.
- **Risk:** Additional fan-command cadence and hysteresis tuning may be needed after field logs confirm write timing and thermal response.

### OMEN Transcend 14-fb1xxx Fn+P and Startup Fan State
- **Status:** Fn+F12 dedicated launch signature fixed; Fn+P and first-launch fan state still require field evidence.
- **Risk:** Firmware event signatures may vary by BIOS and keyboard controller.

### Severe Main-Window Lag on OMEN 15 dh1xxxx
- **Status:** Dashboard projection pressure and background dashboard overhead reduced.
- **Risk:** Full fix may require broader 3.7.0 UI projection coordinator work if dispatcher counters still show backlog.

---

## Release Artifacts (SHA256)

- `OmenCoreSetup-3.6.3.exe`  
  `3EC5D7E59C0018F0845ED8B8A44C35AEE55C919F1B9E6692F61FF7B498A06346`
- `OmenCore-3.6.3-win-x64.zip`  
  `31DE619954DC31B5ECA0394CDB43DF1D0BA93C88347A18E39AE11E7B50BFAF45`
- `OmenCore-3.6.3-linux-x64.zip`  
  `1A02ACF34AD073AD044DD2FB5EA6233AC430978CBE90F947A78D3E30B2E0A735`

---

## Notes

- This is a rolling changelog. Additional v3.6.3 fixes will be appended as new patches land.
- Desktop fan control is intentionally conservative in this release. Telemetry and safe non-fan controls remain available where supported.
