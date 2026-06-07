# OmenCore v3.7.1 - 3.7.0 Stable Follow-up and Quick Access Hotfix

**Release Date:** 2026-06-07  
**Release Status:** Stable Release  
**Type:** Patch release  
**Base Version:** v3.7.0

---

## Summary

v3.7.1 starts as a targeted post-3.7.0 stable follow-up for Jonathan's HP OMEN 16-am0xxx / ProductId `8D2F` report, ZeroMentu's OMEN Transcend 14-fb1xxx / ProductId `8E41` report, Mr.Carrot's Victus 16-s0xxx / ProductId `8BD4` report, Jack Sparrow's OMEN 15-dc1077tx / ProductId `8574` legacy support report, and Fickert's OMEN 16 Max per-key RGB support request. The first fixes focus on Quick Access behavior, WMI V1 fan ramp-down on reported V1 profiles, profile-only fan capability gating, hardware-worker crash containment, and clearer capability/identity reporting.

OGH parity issues around FPS, fan noise, Eco mode, CPU temperature authority, and direct power-limit availability remain tracked for field validation and follow-up work.

---

## Fixed

### Quick Access Uses One-Click Quick Profiles Again

- Reworked the left-click Quick Access popup so Quiet, Balanced, and Performance are combined one-click profile buttons.
- The popup now routes those buttons through the same combined quick-profile path as the tray context menu.
- Kept manual fan-only controls for Auto, Curve, and Max in the popup for advanced adjustments.

### `8D2F` WMI V1 Auto Handoff Can Clear Stale Fan Floors

- Added a model capability override for `OMEN 16-am0xxx (8D2F)` to allow WMI V1 auto-mode floor clear.
- This keeps direct EC fan writes and independent curves disabled, but permits the safe WMI handoff needed to let fans ramp down after load.
- Updated capability notes with the 2026-06-02 Discord follow-up.
- Added regression coverage to keep the `8D2F` override in place.

### `8E41` Transcend 14 No Longer Restores Unsupported Curve Control

- Added a guarded WMI V1 auto-mode floor clear override for `OMEN Transcend 14-fb1xxx (8E41)`.
- Kept direct EC writes and custom fan curves disabled for `8E41`.
- FanService now strips curve payloads from built-in fan presets on curve-disabled models so they apply OEM profile modes only.
- Manual/custom curve presets and direct custom curve calls are rejected on curve-disabled models instead of being saved as active.
- Direct fixed-speed fan writes are also blocked on profile-only models so unsupported manual fan levels cannot reach the controller.
- Fan Control no longer loads saved custom curve presets or restores ad-hoc custom curves when the current model is profile-only.
- Fan Control now shows a profile-only capability badge, disables the Custom card/editor, and changes profile subtitles from curve temperature targets to OEM profile labels where curves are unavailable.
- Added regression coverage for the `8E41` capability override and curve-disabled FanService behavior.

### `8BD4` Victus 16 Can Clear Stale WMI V1 Fan Floors

- Added a model capability override for `HP Victus 16-s0xxx AMD (8BD4)` to allow WMI V1 auto-mode floor clear.
- This keeps direct EC fan writes disabled, but permits the safe WMI handoff needed to let fans ramp down after long gaming sessions.
- Updated capability notes with the 2026-06-03 Discord follow-up.
- Added regression coverage to keep the `8BD4` override and exact identity behavior in place.

### Hardware Worker Avoids AMD ADL Frame-Metrics Crashes On Hybrid Laptops

- Hardware worker now detects hybrid AMD+NVIDIA systems and quarantines AMD GPU telemetry before LibreHardwareMonitor calls the AMD ADL update path.
- This avoids native `ADL2_Adapter_FrameMetrics_Get` access violations seen on `8BD4` while keeping NVIDIA GPU, CPU, fan, memory, battery, and storage telemetry active.
- Startup hardware logging now skips AMD GPU `Update()` calls once AMD telemetry is quarantined.

### Launch Diagnostics And Recovery Are Easier To Validate

- Diagnostics now include `launch-readiness.txt`, a single 3.7.1 field-validation snapshot covering fan recovery state, recent performance-mode routing, CPU temperature authority, HP keyboard RGB backend status, and hardware-worker AMD ADL quarantine expectations.
- Performance mode applies now keep a bounded in-memory trace showing requested/effective mode, model overrides, EC power-limit availability or skip reason, WMI thermal-policy fallback use, fan backend, and fan-policy action.
- Fan Control now exposes a Restore OEM Auto action that clears OmenCore fan ownership, returns the controller to firmware auto mode, and records the recovery path in fan command history.
- RGB diagnostics now explicitly call out OMEN Max/per-key-capable hardware when the dedicated HID per-key backend/editor is still pending, so launch reports do not imply OpenRGB or the current fallback backend has confirmed built-in keyboard per-key support.
- Release-facing README copy now points at v3.7.1 artifacts and no longer presents OMEN Max per-key RGB as fully implemented before the dedicated HID backend/editor is field verified.
- Final release checklist is updated for v3.7.1 with version consistency, install/upgrade, PawnIO/Secure Boot, reported-model, diagnostics, and Linux validation gates.

### `8BCD` OMEN 16-xd0xxx Fan Handoff And Hotkey Polish

- ProductId `8BCD` keeps the 63-level WMI V1 fan ceiling and now opts into WMI V1 auto-mode floor clear so profile/reset handoff can release stale manual fan floors after load.
- Model identity summary coverage now verifies `8BCD` reports `Exact ProductId` with medium confidence instead of low-confidence model-name inference.
- Settings > Hotkeys now lists `Ctrl + Shift + E` for General profile cycling, matching the already-registered runtime hotkey.

### Model Identity Summary Prefers Exact ProductId Matches

- Fixed diagnostics reporting where a profile with both a ProductId match and model-name pattern match could be shown as a low-confidence model-name inference.
- Non-ambiguous exact ProductId matches now report `Exact ProductId` and medium/high confidence depending on user verification state.

### Undervolt Detection No Longer Advertises PawnIO Support Before Model Veto

- Capability detection now checks `SupportsUndervolt = false` from the model database before runtime PawnIO/MSR undervolt probing.
- `8D2F` diagnostics should now consistently report model-disabled undervolt from the start instead of first saying PawnIO undervolt is available and then disabling it later.

---

## Known Limitations And Follow-up

- OGH parity on `8D2F`: reported FPS deltas of about 2% Balanced, 6% Performance, and 3% Custom Performance+Auto in Cyberpunk 2077.
- OGH parity on fan noise: user reports fans are louder in OmenCore across Balanced, Performance, and Custom scenarios.
- CPU temperature authority: logs show WMI/ACPI vs LHM fallback switching and frozen/recovered sensor events.
- Eco mode: OGH Eco combines reduced CPU/GPU performance with 60 Hz display behavior; OmenCore does not yet expose an equivalent combined profile.
- Direct PL1/PL2 controls: still firmware/MSR gated on `8D2F`; WMI policy fallback remains the safe path until a validated OEM power path is found.
- `8E41` Fn+P and Fn+F12 behavior: Win+F12 is registered and observed working, but supplied logs do not show raw Fn+F12/Fn+P events reaching OmenCore.
- `8E41` advanced lighting effects: static/per-key ColorTable backend initializes, but advanced effect backend remains unconfirmed.
- `8BD4` field validation: confirm fans ramp down after long gaming sessions and confirm AMD ADL quarantine stops hardware-worker crashes without losing NVIDIA telemetry.
- `8574` legacy support validation: 3.6.3 logs show HP WMI present but command-nonfunctional, direct EC fan cleaner writes logged success, and the user reports 3.7.0 still has no effective controls except low fan-cleaner output. Treat current `8574` support as partial until a fresh 3.7.1 diagnostic bundle confirms RPM/level readback after Restore OEM Auto, Fan Cleaner, Max, Auto, and profile changes.
- OMEN 16 Max per-key RGB: OmenCore recognizes mapped OMEN Max per-key-capable hardware, but the dedicated `HidPerKey` backend/editor is not implemented yet. Keep zone/light-bar fallback available where supported and avoid presenting OpenRGB as confirmed built-in keyboard support without field evidence.
- WPF UI framework suggestion: community suggested evaluating `wpfui.lepo.co` for a cleaner Windows shell. Track for a later design pass, not as a 3.7.1 blocker.

---

## Release Artifact Hashes

```text
1A1A2012DD1C603FB6BF5480FCF97AC2C67401B1670FFF23D853F0C0F6A1A109  OmenCoreSetup-3.7.1.exe
F53E22F6A1D047ACD09F3734DED02C516ECD900016E0FA2345835437634368DB  OmenCore-3.7.1-win-x64.zip
FCED463725791E95BBCBE203D4E7B4B30D16BD94E62A8AF38E8CBE721CC368E6  OmenCore-3.7.1-linux-x64.zip
```

---

## Files Changed So Far

- `src/OmenCoreApp/Views/QuickPopupWindow.xaml`
- `src/OmenCoreApp/Views/QuickPopupWindow.xaml.cs`
- `src/OmenCoreApp/Utils/TrayIconService.cs`
- `src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs`
- `src/OmenCoreApp/Services/Diagnostics/ModelIdentityResolutionSummary.cs`
- `src/OmenCoreApp/Services/Diagnostics/DiagnosticExportService.cs`
- `src/OmenCore.HardwareWorker/Program.cs`
- `src/OmenCoreApp/Hardware/FanControllerFactory.cs`
- `src/OmenCoreApp/Hardware/CapabilityDetectionService.cs`
- `src/OmenCoreApp/Services/PerformanceModeService.cs`
- `src/OmenCoreApp/Services/FanService.cs`
- `src/OmenCoreApp/ViewModels/FanControlViewModel.cs`
- `src/OmenCoreApp/Views/FanControlView.xaml`
- `src/OmenCoreApp.Tests/Hardware/ModelCapabilityDatabaseTests.cs`
- `src/OmenCoreApp.Tests/Services/FanSmoothingTests.cs`
- `README.md`
- `INSTALL.md`
- `docs/DISCORD_ANNOUNCEMENT_v3.7.1.md`
- `VERSION.txt`
- `installer/OmenCoreInstaller.iss`
- `installer/generate-wizard-images.ps1`
- `src/OmenCoreApp/OmenCoreApp.csproj`
- `src/OmenCore.HardwareWorker/OmenCore.HardwareWorker.csproj`
- `src/OmenCore.Linux/OmenCore.Linux.csproj`
- `src/OmenCore.Avalonia/OmenCore.Avalonia.csproj`
- `src/OmenCore.Avalonia/ViewModels/MainWindowViewModel.cs`
- `docs/FINAL_RELEASE_CHECKLIST.md`
- `docs/3.7.1-BUG-REPORTS.md`
- `docs/CHANGELOG_v3.7.1.md`
