# OmenCore v3.6.2 Changelog (Stabilization)

> Release-ready notes now live in [CHANGELOG_v3.6.2.md](CHANGELOG_v3.6.2.md).
> This file remains the detailed engineering audit trail for the v3.6.2 stabilization work.

## Test Suite Validation (Final Pass)
- [x] Cadence mismatch resolved: Test `GetEffectiveCadenceInterval_UsesActiveCadence2s_WhenOverlayRealtimeModeEnabledInTray` updated to expect 2s active cadence.
  Root cause: Test was stale. v3.6.2 deliberately reduced active monitoring cadence from 1s to 2s for focused-window overhead reduction. This change was already documented in Fixed section and implemented in [src/OmenCoreApp/Services/HardwareMonitoringService.cs:24](../src/OmenCoreApp/Services/HardwareMonitoringService.cs).
  Fix: [src/OmenCoreApp.Tests/Services/HotkeyAndMonitoringTests.cs:326-345](../src/OmenCoreApp.Tests/Services/HotkeyAndMonitoringTests.cs) now expects `TimeSpan.FromSeconds(2)` with comment noting OSD overlay remains responsive at 2s cadence.
- [x] Post-RC1 focused hardening passed: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore` -> **0 errors, 0 warnings**.
- [x] Post-RC1 focused regression tests passed: `dotnet test src/OmenCoreApp.Tests/OmenCoreApp.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~RgbManagerTests|FullyQualifiedName~WmiBiosMonitorTests|FullyQualifiedName~RuntimeCommandDispatcherTests"` -> **21 passed, 0 failed**.
- [x] RC1 field-fix build passed after battery/OSD source-of-truth corrections: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore` -> **0 errors, 0 warnings**.
- [x] RC1 focused UI/runtime regression tests passed: `dotnet test src/OmenCoreApp.Tests/OmenCoreApp.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~DashboardViewModelTests|FullyQualifiedName~MainViewModelTests|FullyQualifiedName~RuntimeIntentDispatchIntegrationTests|FullyQualifiedName~TrayFanModeHeaderTests"` -> **27 passed, 0 failed**.
- [x] RC1 fan/profile/identity hotfix build passed: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore` -> **0 errors, 0 warnings**.
- [x] RC1 fan/profile/identity focused regression tests passed: `dotnet test src/OmenCoreApp.Tests/OmenCoreApp.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~GeneralViewModelTests|FullyQualifiedName~ModelCapabilityDatabaseTests|FullyQualifiedName~KeyboardModelDatabaseTests|FullyQualifiedName~FanDiagnosticsViewModelTests|FullyQualifiedName~FanControlViewModelTests|FullyQualifiedName~MainViewModelTests"` -> **70 passed, 0 failed**.
- [x] Deep-sweep custom-curve/game-profile/UI safeguard build passed: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore -p:UseSharedCompilation=false` -> **0 errors, 0 warnings**.
- [x] Deep-sweep focused regression tests passed: `dotnet test src/OmenCoreApp.Tests/OmenCoreApp.Tests.csproj -c Debug --no-restore -p:UseSharedCompilation=false --filter "FullyQualifiedName~FanControlViewModelTests|FullyQualifiedName~ResourceDictionaryTests|FullyQualifiedName~GameProfile|FullyQualifiedName~MainViewModelTests|FullyQualifiedName~GeneralViewModelTests"` -> **37 passed, 0 failed**.
- [x] 3.2.5-baseline follow-up build passed after Quick Access/hotkey/custom-delete hardening: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore -p:UseSharedCompilation=false` -> **0 errors, 0 warnings**.
- [x] 3.2.5-baseline focused regression tests passed: `dotnet test src/OmenCoreApp.Tests/OmenCoreApp.Tests.csproj -c Debug --no-restore -p:UseSharedCompilation=false --filter "FullyQualifiedName~FanControlViewModelTests|FullyQualifiedName~MainViewModelTests|FullyQualifiedName~GeneralViewModelTests|FullyQualifiedName~GameProfile"` -> **37 passed, 0 failed**.
- [x] RC1 field-feedback build passed after Victus 8BD4, startup-restore, sync, calibration, and GPU-power hardening: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore -p:UseSharedCompilation=false` -> **0 errors, 0 warnings**.
- [x] RC1 field-feedback focused regression tests passed: `dotnet test src/OmenCoreApp.Tests/OmenCoreApp.Tests.csproj -c Debug --no-restore -p:UseSharedCompilation=false --filter "FullyQualifiedName~ModelCapabilityDatabaseTests|FullyQualifiedName~KeyboardModelDatabaseTests|FullyQualifiedName~NvapiServiceTests|FullyQualifiedName~GeneralViewModelTests|FullyQualifiedName~MainViewModelTests|FullyQualifiedName~FanControlViewModelTests"` -> **76 passed, 0 failed**.
- [x] RC1 hotkey/Quick Access follow-up build passed: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore -p:UseSharedCompilation=false` -> **0 errors, 0 warnings**.
- [x] RC1 hotkey/Quick Access focused regression tests passed: `dotnet test src/OmenCoreApp.Tests/OmenCoreApp.Tests.csproj -c Debug --no-restore -p:UseSharedCompilation=false --filter "FullyQualifiedName~ModelCapabilityDatabaseTests|FullyQualifiedName~KeyboardModelDatabaseTests|FullyQualifiedName~NvapiServiceTests|FullyQualifiedName~GeneralViewModelTests|FullyQualifiedName~MainViewModelTests|FullyQualifiedName~FanControlViewModelTests|FullyQualifiedName~HotkeyServiceTests"` -> **81 passed, 0 failed**.
- [x] Linux hp-wmi/hwmon follow-up build passed: `dotnet build src/OmenCore.Linux/OmenCore.Linux.csproj -c Debug --no-restore -p:UseSharedCompilation=false` -> **0 errors, 0 warnings**.
- [x] Full solution build passed: `dotnet build OmenCore.sln -c Debug --no-restore`.
- [x] Full Windows test suite passed: `dotnet test src/OmenCoreApp.Tests/OmenCoreApp.Tests.csproj -c Debug --no-build` -> **642 passed, 0 failed**.
- [x] Focused cadence/concurrency regressions passed: targeted run covering cadence + latest-wins coordinator tests -> **2 passed, 0 failed**.
- [x] All modified source files verified for compile errors: No errors found.
- [x] Focused regression tests passed (dashboard dormancy, GeneralViewModel projection, runtime counters, tray behavior, release gates): **36 passed, 0 failed**.
- [x] Windows app build succeeded (OmenCoreApp.csproj, Debug configuration).

## Release Readiness
- **Status**: RC hardening in progress; code-side stabilization complete, awaiting final hardware matrix sign-off.
- **Scope**: Runtime authority correction, UI quieting, thermal safety, focused-window overhead reduction.
- **Field Impact**: Addresses mode drift, OSD anomalies, hidden-surface churn, and tray/popup dispatcher overhead.
- **Remaining**: Manual performance matrix on real hardware (captures frame pacing, load averages, GC pressure with sustained gaming session) and Scenario A operator runbook evidence.
## Architecture Remediation Status
- v3.6.2 is a stabilization release, not a feature release.
- The focus is runtime authority correction, deterministic hotkey cycling, thermal safety, and lower focused-window telemetry overhead.
- The release narrows field-reported mode drift and OSD anomalies by removing phantom runtime states and making the canonical mode labels match the actual fan policy.

## Fixed
- [x] CPU thermal authority attribution no longer flaps during same-poll fallback evaluation.
  Root cause: WMI/ACPI authority could be recorded before the LibreHardwareMonitor fallback decision finished, then immediately switch to fallback within the same polling pass. That made diagnostics noisier and could inflate authority switch counts without a real source transition.
  Fix: [src/OmenCoreApp/Hardware/WmiBiosMonitor.cs](../src/OmenCoreApp/Hardware/WmiBiosMonitor.cs) now defers primary CPU authority assignment until fallback evaluation completes, only records WMI/ACPI when fallback did not apply, and logs the actual adaptive fallback cooldown duration after worker timeout.
- [x] RGB effect capability filtering covers additional real effect payloads before provider fanout.
  Root cause: the graceful-degradation filter recognized basic `effect:*` names, but payload forms such as `breathing:#RRGGBB`, `pulse:#RRGGBB:1000`, `wave`, and `off` could still be sent to providers that did not advertise support.
  Fix: [src/OmenCoreApp/Services/Rgb/RgbManager.cs](../src/OmenCoreApp/Services/Rgb/RgbManager.cs) now resolves those payload forms to `Breathing`, `Wave`, and `Off` capability classes before fanout, keeping static-only providers out of unsupported dynamic/off requests.
- [x] Monitoring battery capacity no longer falls back to a fake 100% value.
  Root cause: the dashboard battery-health card displayed `100%` when WMI capacity data was unavailable, which confused charge-limit users because an 80% charge cap is separate from battery wear/capacity health.
  Fix: [src/OmenCoreApp/Controls/HardwareMonitoringDashboard.xaml.cs](../src/OmenCoreApp/Controls/HardwareMonitoringDashboard.xaml.cs) now shows capacity health as unavailable when design/full-charge capacity cannot be read, and [src/OmenCoreApp/Controls/HardwareMonitoringDashboard.xaml](../src/OmenCoreApp/Controls/HardwareMonitoringDashboard.xaml) labels the card as battery capacity instead of implying current charge-limit state.
- [x] Tray/OSD fan and performance mode projection now uses confirmed runtime state instead of requested UI selection.
  Root cause: `MainViewModel` could publish selected FanControl/SystemControl item names as soon as the UI selection changed, before `FanService` or `PerformanceModeService` confirmed the actual applied mode.
  Fix: [src/OmenCoreApp/ViewModels/MainViewModel.cs](../src/OmenCoreApp/ViewModels/MainViewModel.cs) now keeps tray, dashboard, and OSD projection on service-confirmed fan/performance state; request selections no longer become authoritative until the service reports the applied mode.
- [x] Fan curve presets now apply their real curve immediately when selected.
  Root cause: the fan editor only loaded saved curves for `Manual` presets, so built-in curve presets could show the default Auto graph, and user-applied Auto/Gaming/Quiet curves could wait for the next monitor cadence before writing a new duty level. With `ApplyImmediatelyOnUserAction=false`, this made high-temperature systems look like the curve was ignored.
  Fix: [src/OmenCoreApp/ViewModels/FanControlViewModel.cs](../src/OmenCoreApp/ViewModels/FanControlViewModel.cs) now loads any preset curve into the graph and requests immediate application for curve-backed user selections; [src/OmenCoreApp/Services/FanService.cs](../src/OmenCoreApp/Services/FanService.cs) now honors immediate application for Auto presets that carry explicit curves.
- [x] Edited custom fan presets persist when applied.
  Root cause: applying an edited saved custom curve created a transient `Custom` preset and did not update the selected saved preset unless the user separately saved it again.
  Fix: applying a selected non-built-in manual preset now updates that preset's stored curve and `LastFanPresetName`, so reopening OmenCore preserves the graph.
- [x] Ad-hoc custom fan curves now survive restart even when the user clicks Apply instead of Save.
  Root cause: `AppConfig.CustomFanCurve` existed, but the Fan Control page did not restore or promote the last applied ad-hoc curve into the preset list. That left users with an applied curve during the session and a default-looking graph after restart.
  Fix: [src/OmenCoreApp/ViewModels/FanControlViewModel.cs](../src/OmenCoreApp/ViewModels/FanControlViewModel.cs) now persists the last applied custom curve, restores it as a user-editable `Custom` preset, and opens the editor on the last selected preset without reapplying hardware state.
- [x] General tab Performance quick profile no longer forces Max fans as the first choice.
  Root cause: the quick Performance profile coupled Performance power mode to Max cooling, causing idle fan blasts and making normal performance-profile testing look unstable.
  Fix: [src/OmenCoreApp/ViewModels/GeneralViewModel.cs](../src/OmenCoreApp/ViewModels/GeneralViewModel.cs) now applies the Gaming/Extreme curve when available and only falls back to Max if no curve preset exists.
- [x] Tray Performance quick profile now uses Gaming cooling instead of Max.
  Root cause: the tray quick-profile path still used `ApplyMaxCooling()` even after the General tab path was softened. That preserved a 3.2.5 regression risk where quick actions could feel like they overrode normal fan intent.
  Fix: [src/OmenCoreApp/ViewModels/MainViewModel.cs](../src/OmenCoreApp/ViewModels/MainViewModel.cs) now applies a bounded Gaming curve for the tray Performance quick profile and projects the confirmed performance/fan state afterward.
- [x] Hotkey fan cycling no longer announces Custom when no custom curve exists.
  Root cause: the deterministic hotkey cycle included `Custom`, but when no saved/active custom preset was resolvable the target silently fell back to Auto while the OSD/event log still said Custom.
  Fix: [src/OmenCoreApp/ViewModels/MainViewModel.cs](../src/OmenCoreApp/ViewModels/MainViewModel.cs) now skips the Custom display slot unless a real saved or active custom curve can be resolved, preserving honest OSD/tray state.
- [x] Quick Access custom preset resolution now checks active Fan Control state and config-backed curves.
  Root cause: `MainViewModel` had a legacy fan-preset collection separate from the Fan Control page, so Custom quick access could miss a curve that the Fan Control view had just saved or selected.
  Fix: [src/OmenCoreApp/ViewModels/MainViewModel.cs](../src/OmenCoreApp/ViewModels/MainViewModel.cs) now resolves Custom from active main selection, active Fan Control selection, the current service preset name, `LastFanPresetName`, and config-backed custom presets.
- [x] Deleting a custom fan preset clears stale last-custom config.
  Root cause: deleting a saved custom preset removed it from the visible list, but `LastFanPresetName` and `CustomFanCurve` could still point at the deleted curve and resurrect it on restart.
  Fix: [src/OmenCoreApp/ViewModels/FanControlViewModel.cs](../src/OmenCoreApp/ViewModels/FanControlViewModel.cs) now falls back to Auto after custom deletion and clears stale ad-hoc custom config when the deleted preset was the last active custom curve.
- [x] General tab performance/fan state now syncs from confirmed runtime events.
  Root cause: service-confirmed fan/performance changes updated tray/sidebar/dashboard, but the General quick-profile state could remain on its previous local selection after changes from tray, hotkeys, System Control, or startup restore.
  Fix: [src/OmenCoreApp/ViewModels/GeneralViewModel.cs](../src/OmenCoreApp/ViewModels/GeneralViewModel.cs) exposes a runtime sync path, and [src/OmenCoreApp/ViewModels/MainViewModel.cs](../src/OmenCoreApp/ViewModels/MainViewModel.cs) calls it from fan/performance service events and tray quick-profile actions.
- [x] Startup hardware restore is now visible and consistently guarded.
  Root cause: `EnableStartupHardwareRestore` and `AllowStartupRestoreOnOmen16OrVictus` existed in config, but users could not see or change them in Settings, and startup fan reapply could run from one path while another path logged that hardware restore was disabled.
  Fix: [src/OmenCoreApp/Views/SettingsView.xaml](../src/OmenCoreApp/Views/SettingsView.xaml) and [src/OmenCoreApp/ViewModels/SettingsViewModel.cs](../src/OmenCoreApp/ViewModels/SettingsViewModel.cs) now expose both startup restore controls and status text; [src/OmenCoreApp/ViewModels/MainViewModel.cs](../src/OmenCoreApp/ViewModels/MainViewModel.cs) uses the same guard before automatic fan/GPU startup reapply.
- [x] Fan diagnostics restore the previous curve immediately after Apply & Verify.
  Root cause: diagnostic tests restored the prior preset without immediate curve application, leaving slow firmware handoff windows where fans could appear stuck at the test duty.
  Fix: [src/OmenCoreApp/ViewModels/FanDiagnosticsViewModel.cs](../src/OmenCoreApp/ViewModels/FanDiagnosticsViewModel.cs) now restores the pre-test preset with immediate curve application.
- [x] Fan calibration cleanup retries BIOS auto-control restore.
  Root cause: the calibration wizard restored auto control once after completion/cancel. On some WMI V1 systems, that single handoff could be missed after testing the second fan, leaving fan behavior visually stuck until another mode change.
  Fix: [src/OmenCoreApp/Services/FanCalibration/FanCalibrationService.cs](../src/OmenCoreApp/Services/FanCalibration/FanCalibrationService.cs) now retries calibration cleanup restore up to three times and logs whether BIOS auto-control was confirmed.
- [x] Win+F12 now opens OmenCore even when focused hotkeys are disabled for the background.
  Root cause: the hotkey service had `ShowWindow` and `OpenDashboard` actions, but they were not wired by `MainViewModel`, and only Ctrl+Shift+O was preserved globally in window-focused mode.
  Fix: [src/OmenCoreApp/Services/HotkeyService.cs](../src/OmenCoreApp/Services/HotkeyService.cs) now registers Win+F12 as a show-window fallback, [src/OmenCoreApp/ViewModels/MainViewModel.cs](../src/OmenCoreApp/ViewModels/MainViewModel.cs) wires the show/open-dashboard actions and preserves restore hotkeys globally, and [src/OmenCoreApp/Views/SettingsView.xaml](../src/OmenCoreApp/Views/SettingsView.xaml) documents the shortcut.
- [x] Quick Access now has a direct full-dashboard button.
  Root cause: testers had to right-click the tray icon or double-click the tray icon to open the full dashboard from the compact popup workflow.
  Fix: [src/OmenCoreApp/Views/QuickPopupWindow.xaml](../src/OmenCoreApp/Views/QuickPopupWindow.xaml), [src/OmenCoreApp/Views/QuickPopupWindow.xaml.cs](../src/OmenCoreApp/Views/QuickPopupWindow.xaml.cs), and [src/OmenCoreApp/Utils/TrayIconService.cs](../src/OmenCoreApp/Utils/TrayIconService.cs) now expose a direct dashboard action from the Quick Access popup.
- [x] Linux hp-wmi hwmon manual fan duty now works through the CLI and daemon curve path.
  Root cause: OmenCore detected `pwm1_enable` but did not write the paired `pwm1`/`pwm2` duty files, so modern hp-wmi boards could use Auto/Max policy but not a real manual percentage or custom curve.
  Fix: [src/OmenCore.Linux/Hardware/LinuxEcController.cs](../src/OmenCore.Linux/Hardware/LinuxEcController.cs), [src/OmenCore.Linux/Commands/FanCommand.cs](../src/OmenCore.Linux/Commands/FanCommand.cs), and [src/OmenCore.Linux/Daemon/FanCurveEngine.cs](../src/OmenCore.Linux/Daemon/FanCurveEngine.cs) now use `pwm_enable=1` plus kernel-standard `pwmN` duty writes for manual speed, restore auto via the common restore path, and continue blocking fan-off writes.
- [x] Linux diagnostics now capture the hp-wmi board-support evidence testers are reporting.
  Root cause: `diagnose` reported broad hp-wmi/platform-profile presence but not `hp_wmi.force_multiplex`, `pwm1_enable`, `pwm1`, fan input exposure, or AP-series board-specific guidance.
  Fix: [src/OmenCore.Linux/Commands/DiagnoseCommand.cs](../src/OmenCore.Linux/Commands/DiagnoseCommand.cs), [src/OmenCore.Linux/Hardware/LinuxSysfsPathMap.cs](../src/OmenCore.Linux/Hardware/LinuxSysfsPathMap.cs), and [src/OmenCore.Linux/README.md](../src/OmenCore.Linux/README.md) now surface those paths and add OMEN 16 ap0xxx / NVIDIA ACPI DSM / D3cold triage notes without recommending unsafe legacy EC writes.
- [x] Game-library Create/Edit Profile now opens the profile editor on the intended profile.
  Root cause: `GameLibraryViewModel` raised profile create/edit events, but `MainViewModel` never subscribed to them, so the Games tab buttons appeared inert.
  Fix: [src/OmenCoreApp/ViewModels/MainViewModel.cs](../src/OmenCoreApp/ViewModels/MainViewModel.cs) now opens `GameProfileManagerView` for those events, and [src/OmenCoreApp/ViewModels/GameProfileManagerViewModel.cs](../src/OmenCoreApp/ViewModels/GameProfileManagerViewModel.cs) accepts an initial profile selection.
- [x] Game profile saves now validate the edited profile at save time and refresh tracking state.
  Root cause: the profile editor validation ran when selection changed, but not reliably when fields changed inside the selected profile object.
  Fix: [src/OmenCoreApp/ViewModels/GameProfileManagerViewModel.cs](../src/OmenCoreApp/ViewModels/GameProfileManagerViewModel.cs) now revalidates before saving, blocks invalid profiles with a warning, and routes selected-profile saves through `GameProfileService.UpdateProfileAsync()` so process tracking refreshes.
- [x] Settings General rows wrap long descriptions so toggles remain visible on narrower windows.
  Root cause: several General settings descriptions had no wrapping, which could stretch the text column and push toggle switches out of the visible area at smaller window widths or higher display scaling.
  Fix: [src/OmenCoreApp/Views/SettingsView.xaml](../src/OmenCoreApp/Views/SettingsView.xaml) now wraps those descriptions in the General settings section.
- [x] OMEN 16-ap0xxx ProductId 8E35 now has exact capability and keyboard mappings.
  Root cause: 8E35 systems fell through to model-name inference even though RC1 diagnostics identified the board/SKU.
  Fix: exact 8E35 entries were added to [src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs](../src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs) and [src/OmenCoreApp/Services/KeyboardLighting/KeyboardModelDatabase.cs](../src/OmenCoreApp/Services/KeyboardLighting/KeyboardModelDatabase.cs) with conservative WMI V1 fan and four-zone keyboard defaults.
- [x] Implausible NVIDIA laptop GPU power readings are normalized before display.
  Root cause: some NVAPI laptop readings are surfaced in a unit/scale that made RTX 50-series laptop GPUs appear to draw impossible wattage such as 225W at light load.
  Fix: [src/OmenCoreApp/Hardware/NvapiService.cs](../src/OmenCoreApp/Hardware/NvapiService.cs) now recognizes RTX 50-series laptop fallback TDPs and converts/suppresses implausible PowerTopology readings before they reach tray/dashboard/OSD telemetry.
- [x] RTX 40-series laptop GPU power readings now have the same implausible-value guardrails.
  Root cause: RC1 logs from Victus 16-s0xxx / RTX 4060 Laptop GPU showed ~220W readings, which are not plausible for that laptop GPU class and made tray/dashboard power telemetry look broken.
  Fix: [src/OmenCoreApp/Hardware/NvapiService.cs](../src/OmenCoreApp/Hardware/NvapiService.cs) and [src/OmenCoreApp/Hardware/WmiBiosMonitor.cs](../src/OmenCoreApp/Hardware/WmiBiosMonitor.cs) now normalize/suppress implausible laptop GPU power values across NVAPI, MAHM, and LHM fallback paths, with regression coverage for RTX 4060 Laptop GPU.
- [x] Victus 16-s0xxx ProductId 8BD4 now has exact capability and keyboard mappings.
  Root cause: RC1 logs showed Victus 16-s0xxx (8BD4, Ryzen 7 7840HS + RTX 4060) falling to unknown Victus defaults, which incorrectly reduced fan count and made support status vague.
  Fix: exact 8BD4 entries were added to [src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs](../src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs) and [src/OmenCoreApp/Services/KeyboardLighting/KeyboardModelDatabase.cs](../src/OmenCoreApp/Services/KeyboardLighting/KeyboardModelDatabase.cs) with conservative WMI V1 fan, two-fan, backlight-only, no-GPU-boost defaults pending hardware verification.
- [x] v3.6.2 binary metadata now matches the release tag.
  Root cause: `VERSION.txt` was updated, but project assembly/file versions and installer fallback still reported v3.6.1.
  Fix: app, hardware worker, Linux, Avalonia, desktop project metadata and [installer/OmenCoreInstaller.iss](../installer/OmenCoreInstaller.iss) now report v3.6.2 / 3.6.2.0.
- [x] Hotkey cycle state now resolves canonical slots in the required order: Auto -> Gaming -> Extreme -> Custom -> Quiet.
  Root cause: Gaming and Quiet were not first-class built-in presets, which let alias resolution and quick-access selection drift into the wrong slot.
  Fix: built-in Gaming and Quiet presets now exist in [src/OmenCoreApp/ViewModels/FanControlViewModel.cs](../src/OmenCoreApp/ViewModels/FanControlViewModel.cs), Gaming resolves to the Gaming runtime label in [src/OmenCoreApp/Services/FanService.cs](../src/OmenCoreApp/Services/FanService.cs), and hotkey alias routing now keeps Gaming separate from Extreme in [src/OmenCoreApp/ViewModels/FanControlViewModel.cs](../src/OmenCoreApp/ViewModels/FanControlViewModel.cs).
- [x] Quiet mode no longer manufactures a fake Manual preset.
  Root cause: Quiet requests created a non-built-in manual preset, which polluted runtime slot detection.
  Fix: Quiet now selects the built-in Quiet preset directly.
- [x] Extreme mode now bypasses the normal smoothing path on apply.
  Root cause: the highest thermal-performance mode still entered the normal transition path, which delayed fan ramp-up during escalation.
  Fix: Extreme preset applies now request immediate fan application in [src/OmenCoreApp/ViewModels/FanControlViewModel.cs](../src/OmenCoreApp/ViewModels/FanControlViewModel.cs).
- [x] Battery OSD no longer snaps to 100% during cooldown windows.
  Root cause: the battery provider returned a hardcoded 100 while throttled.
  Fix: [src/OmenCoreApp/Hardware/WmiBiosMonitor.cs](../src/OmenCoreApp/Hardware/WmiBiosMonitor.cs) now caches the last valid charge reading and returns that cached value during cooldown or battery-disable states.
- [x] Thermal authority hardening added for low-temp/high-load CPU telemetry drift (Issue #129).
  Root cause: CPU thermal authority could remain on low-confidence WMI/ACPI values during active load, masking package-sensor divergence and making source transitions hard to audit.
  Fix: [src/OmenCoreApp/Hardware/WmiBiosMonitor.cs](../src/OmenCoreApp/Hardware/WmiBiosMonitor.cs) now enforces mismatch-confirmed LibreHardwareMonitor fallback under suspicious low-temp/high-load conditions, tracks explicit CPU thermal authority source/reason, logs authority transitions, and exposes current authority in monitoring source text for diagnostics export.
- [x] Victus e0xxx capability/identity fallback reduced (Issue #128).
  Root cause: ProductId 88EC fell through to broad Victus family defaults, producing low-confidence identity output and ambiguous keyboard capability messaging.
  Fix: explicit ProductId 88EC entries were added to [src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs](../src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs) and [src/OmenCoreApp/Services/KeyboardLighting/KeyboardModelDatabase.cs](../src/OmenCoreApp/Services/KeyboardLighting/KeyboardModelDatabase.cs) with conservative, non-overstated defaults pending field verification.
- [x] RGB dynamic-effect unsupported paths now degrade gracefully (Issue #130).
  Root cause: effect fanout attempted unsupported provider endpoints without explicit pre-filtering, creating noisy failures on platforms that support static RGB but not dynamic effects.
  Fix: [src/OmenCoreApp/Services/Rgb/RgbManager.cs](../src/OmenCoreApp/Services/Rgb/RgbManager.cs) now resolves known effect types, skips unsupported providers with explicit logs, and exits cleanly when no provider supports the requested effect.
- [x] Focused-window monitoring overhead is lower.
  Root cause: active cadence was too aggressive for the amount of UI-thread work it triggered.
  Fix: [src/OmenCoreApp/Services/HardwareMonitoringService.cs](../src/OmenCoreApp/Services/HardwareMonitoringService.cs) now uses a 2s active cadence instead of 1s.
- [x] Runtime telemetry no longer fans out through the shared mode/projection event pipeline.
  Root cause: `RuntimeStateEngine.StateChanged` was still carrying monitoring samples, so sample noise triggered the same subscriber path used for fan/performance/curve projection.
  Fix: [src/OmenCoreApp/Services/RuntimeStateEngine.cs](../src/OmenCoreApp/Services/RuntimeStateEngine.cs) no longer stores or publishes monitoring samples, and [src/OmenCoreApp/App.xaml.cs](../src/OmenCoreApp/App.xaml.cs) now sends tray telemetry directly from `MainViewModel.LatestMonitoringSample`.
- [x] Main summary bindings no longer redraw on unchanged rendered text.
  Root cause: tiny telemetry noise still raised `PropertyChanged` for CPU/GPU/memory/storage/clock summaries even when the user-visible strings were identical.
  Fix: [src/OmenCoreApp/ViewModels/MainViewModel.cs](../src/OmenCoreApp/ViewModels/MainViewModel.cs) now compares old/new rendered summaries before notifying summary bindings.
- [x] Hidden General surfaces now skip telemetry projection entirely.
  Root cause: `MainViewModel` still pushed every accepted sample into `GeneralViewModel` even when the General tab was not visible.
  Fix: [src/OmenCoreApp/ViewModels/GeneralViewModel.cs](../src/OmenCoreApp/ViewModels/GeneralViewModel.cs) now exposes a visibility gate, and [src/OmenCoreApp/Views/GeneralView.xaml.cs](../src/OmenCoreApp/Views/GeneralView.xaml.cs) toggles telemetry projection based on actual view visibility.
- [x] Hidden and minimized dashboard surfaces now become dormant.
  Root cause: dashboard redraw suppression existed in the control, but [src/OmenCoreApp/ViewModels/DashboardViewModel.cs](../src/OmenCoreApp/ViewModels/DashboardViewModel.cs) still accepted hidden samples and mutated chart/history state.
  Fix: [src/OmenCoreApp/ViewModels/DashboardViewModel.cs](../src/OmenCoreApp/ViewModels/DashboardViewModel.cs) now keeps only the latest queued hidden sample, and [src/OmenCoreApp/Controls/HardwareMonitoringDashboard.xaml.cs](../src/OmenCoreApp/Controls/HardwareMonitoringDashboard.xaml.cs) toggles dashboard projection from actual visibility/minimized state.
- [x] Tray and popup refreshes now skip redundant rendered state.
  Root cause: fixed tray/popup timers still reassigned identical tooltip, menu, icon, and popup text state even when the user-visible output had not changed.
  Fix: [src/OmenCoreApp/Utils/TrayIconService.cs](../src/OmenCoreApp/Utils/TrayIconService.cs) now caches last rendered tray state, and [src/OmenCoreApp/Views/QuickPopupWindow.xaml.cs](../src/OmenCoreApp/Views/QuickPopupWindow.xaml.cs) now caches popup telemetry render state to suppress no-op redraws.
- [x] Linux config ownership is fully unified under TOML.
  Root cause: `battery.profile` still persisted through a separate JSON helper, leaving split config authority in the Linux CLI.
  Fix: [src/OmenCore.Linux/Config/OmenCoreConfig.cs](../src/OmenCore.Linux/Config/OmenCoreConfig.cs) now owns battery profile persistence and the dead JSON `ConfigManager` path was removed from [src/OmenCore.Linux/Program.cs](../src/OmenCore.Linux/Program.cs).

## Changed
- [x] v3.6.2 version metadata updated in [VERSION.txt](../VERSION.txt).
- [x] Regression coverage added for canonical fan-mode identity, hotkey slot resolution, curve safety floor, and battery cooldown behavior.
- [x] Diagnostics now export runtime UI amplification ratios from [src/OmenCoreApp/Services/RuntimeUiPerformanceCounters.cs](../src/OmenCoreApp/Services/RuntimeUiPerformanceCounters.cs) through [src/OmenCoreApp/Services/Diagnostics/DiagnosticExportService.cs](../src/OmenCoreApp/Services/Diagnostics/DiagnosticExportService.cs), including projected-sample, dispatcher, and per-surface acceptance ratios.
- [x] RC field-validation diagnostics expanded with dormancy, hidden-surface suppression, tray/popup render-cache hit rates, and latest-sample replacement counters in [src/OmenCoreApp/Services/RuntimeUiPerformanceCounters.cs](../src/OmenCoreApp/Services/RuntimeUiPerformanceCounters.cs).
- [x] Diagnostics export now includes a bounded snapshot mode (`runtime-performance-bounded.txt`) to capture short-window scenario evidence (focused idle, tray idle, dashboard active, popup active, OSD active) without continuous logging in [src/OmenCoreApp/Services/Diagnostics/DiagnosticExportService.cs](../src/OmenCoreApp/Services/Diagnostics/DiagnosticExportService.cs).
- [x] Regression coverage now includes unchanged-summary suppression in [src/OmenCoreApp.Tests/ViewModels/MainViewModelTests.cs](../src/OmenCoreApp.Tests/ViewModels/MainViewModelTests.cs) and derived counter ratios in [src/OmenCoreApp.Tests/Services/RuntimeUiPerformanceCountersTests.cs](../src/OmenCoreApp.Tests/Services/RuntimeUiPerformanceCountersTests.cs).
- [x] Hidden-surface suppression coverage now exists in [src/OmenCoreApp.Tests/ViewModels/GeneralViewModelTests.cs](../src/OmenCoreApp.Tests/ViewModels/GeneralViewModelTests.cs) and [src/OmenCoreApp.Tests/ViewModels/DashboardViewModelTests.cs](../src/OmenCoreApp.Tests/ViewModels/DashboardViewModelTests.cs).
- [x] Release-gate coverage now locks in tray/popup render-state dedupe hooks in [src/OmenCoreApp.Tests/ReleaseGateCodeHygieneTests.cs](../src/OmenCoreApp.Tests/ReleaseGateCodeHygieneTests.cs).
- [x] Bounded diagnostics now include triage-friendly classifications (amplification class, acceptance class, cache-hit class, CPU window class) and runtime-state summary fields (cadence reason, low-overhead mode, fan control state) in [src/OmenCoreApp/Services/Diagnostics/DiagnosticExportService.cs](../src/OmenCoreApp/Services/Diagnostics/DiagnosticExportService.cs).
- [x] Tray and quick popup now normalize incoming fan/performance mode labels before UI updates and skip redundant no-op state pushes in [src/OmenCoreApp/Utils/TrayIconService.cs](../src/OmenCoreApp/Utils/TrayIconService.cs) and [src/OmenCoreApp/Views/QuickPopupWindow.xaml.cs](../src/OmenCoreApp/Views/QuickPopupWindow.xaml.cs).
- [x] RGB regression coverage now verifies payload-form effect filtering for breathing payloads and off requests in [src/OmenCoreApp.Tests/Services/RgbManagerTests.cs](../src/OmenCoreApp.Tests/Services/RgbManagerTests.cs).

## Validation
- [x] Syntax checks passed on the touched runtime files.
- [x] Post-RC1 Windows app build passed after CPU authority and RGB fanout hardening: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore`.
- [x] Post-RC1 focused runtime tests passed after CPU authority and RGB fanout hardening: **21 passed, 0 failed**.
- [x] RC1 field-fix Windows app build passed after battery capacity and OSD source-of-truth hardening: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore`.
- [x] RC1 field-fix focused UI/runtime tests passed: **27 passed, 0 failed**.
- [x] RC1 fan/profile/identity hotfix Windows app build passed: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore`.
- [x] RC1 fan/profile/identity focused tests passed: **70 passed, 0 failed**.
- [x] Deep-sweep custom-curve/game-profile/UI safeguard Windows app build passed: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore -p:UseSharedCompilation=false`.
- [x] Deep-sweep focused tests passed: **37 passed, 0 failed**.
- [x] RC1 hotkey/Quick Access follow-up Windows app build passed: `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore -p:UseSharedCompilation=false`.
- [x] RC1 hotkey/Quick Access focused tests passed: **81 passed, 0 failed**.
- [x] Linux hp-wmi/hwmon follow-up build passed: `dotnet build src/OmenCore.Linux/OmenCore.Linux.csproj -c Debug --no-restore -p:UseSharedCompilation=false`.
- [x] Focused regression tests added for fan-mode identity, hotkey cycle resolution, curve ordering, and battery cache behavior.
- [x] `dotnet build src/OmenCoreApp/OmenCoreApp.csproj -c Debug --no-restore` passed after the runtime telemetry fanout removal and summary delta-gating edits.
- [x] `dotnet build src/OmenCore.Linux/OmenCore.Linux.csproj -c Debug --no-restore` passed after removing the final JSON config path.
- [x] Focused test execution passed for [src/OmenCoreApp.Tests/ViewModels/MainViewModelTests.cs](../src/OmenCoreApp.Tests/ViewModels/MainViewModelTests.cs) and [src/OmenCoreApp.Tests/Services/RuntimeUiPerformanceCountersTests.cs](../src/OmenCoreApp.Tests/Services/RuntimeUiPerformanceCountersTests.cs).
- [x] Post-hardening targeted regression run passed: **17 passed, 0 failed** (dashboard/general suppression + counters + release-gate tests).
- [x] Post-hardening Linux build validation passed (`OmenCore.Linux.csproj`, Debug, no-restore).
- [ ] Full Windows suite rerun should be executed in the final RC operator environment before release tag.

## Known Limitations (RC)
- The thermal-limit saturation + low-temperature power-lock anomaly remains a field intake item until reproduced or disproven by Scenario A evidence.
- Windows scaling and DPI behavior (100/125/150/175 + multi-monitor) still require explicit manual sign-off coverage.
- Larger fan-control architecture changes remain deferred to v3.7.0 to keep v3.6.2 low risk.

## Notes
- This release intentionally avoids broad UI refactors.
- Remaining follow-up work should stay centered on hidden/minimized projection suppression, remaining tray/popup refresh cost, and focused-window render load reduction.

## Follow-up Field Evidence
- [x] New field report confirms severe UI frame collapse persists in some real-world sessions ("UI runs at 0.3fps").
- [x] This is tracked as a top-tier architecture/performance investigation, not a cosmetic issue.
- [x] Root-cause audit and decoupling plan documented in [docs/3.7.0-UI-PERFORMANCE-AUDIT.md](3.7.0-UI-PERFORMANCE-AUDIT.md).

## Additional Field Intake (Thermal/Power Anomaly)
- [x] New community reports indicate a rare state where CPU package power appears capped (around 20-30W) while thermal-limit indicators report 100% at only 40-50C.
- [x] Affected users described behavior during/after custom fan preset use in older builds (notably 3.2.5 and 3.4.0), including occasional settings resets and intermittent fan-control instability.
- [x] Reports are currently treated as field evidence, not yet reproduced as a confirmed 3.6.2 regression.
- [x] Validation impact for v3.6.2 RC: prioritize thermal-limit sanity checks (reported thermal-limit reason vs measured temperature/power) and custom-preset persistence/stability scenarios in [docs/3.6.2-RC-VALIDATION.md](3.6.2-RC-VALIDATION.md).
- [x] Triage impact: if thermal-limit reason saturates at low temperature, capture bounded snapshot + HWiNFO evidence and tag the scenario in [docs/3.6.2-PERFORMANCE-TRIAGE.md](3.6.2-PERFORMANCE-TRIAGE.md).
- [x] One-pass execution runbook added for operators: [docs/3.6.2-SCENARIO-A-OPERATOR-RUNBOOK.md](3.6.2-SCENARIO-A-OPERATOR-RUNBOOK.md).
