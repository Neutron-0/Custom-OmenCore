# v3.7.1 - Stable Release

Thanks Jonathan, ZeroMentu, Mr.Carrot, Jack Sparrow, Fickert, and OsamaBiden for the reports/logs!

## Bug Fixes

- **Quick Access profiles** - Quiet/Balanced/Performance are one-click combined profile buttons again
- **8BCD OMEN 16-xd0xxx** - Auto mode can clear stale WMI V1 fan floors after load
- **8D2F OMEN 16-am0xxx** - WMI V1 Auto handoff can release stuck fan floors safely
- **8BD4 Victus 16** - Guarded fan-floor clear for long-session WMI V1 ramp-down
- **8E41 Transcend 14** - Blocks unsupported custom curve/fixed manual fan writes
- **Hybrid AMD+NVIDIA crash** - Quarantines unstable AMD ADL frame metrics while keeping NVIDIA telemetry alive
- **Model identity** - Exact ProductId matches now report correctly
- **Hotkey docs** - Settings lists `Ctrl + Shift + E` profile cycling

## Enhancements

- **Restore OEM Auto** - Returns firmware fan ownership and logs the recovery path
- **Launch diagnostics** - Adds fan recovery, performance routing, CPU authority, RGB backend, and AMD ADL quarantine state
- **Profile-only UI** - Clearer badges and disabled controls on curve-disabled systems
- **OMEN Max RGB** - Detects per-key-capable hardware without claiming the HID editor is finished
- **Release docs/packages** - README, install guide, changelog, checklist, hashes, and Linux package verification are aligned

## Still Tracked

- Dedicated OMEN Max HID per-key editor/backend
- OGH Eco equivalent, GPU power parity, RGB behavior, and exclusive fullscreen OSD validation

**Download:** <https://github.com/theantipopau/omencore/releases/tag/v3.7.1>

```text
1A1A2012  OmenCoreSetup-3.7.1.exe
F53E22F6  OmenCore-3.7.1-win-x64.zip
FCED4637  OmenCore-3.7.1-linux-x64.zip
```

Full hashes + changelog: <https://github.com/theantipopau/omencore/blob/main/docs/CHANGELOG_v3.7.1.md>
