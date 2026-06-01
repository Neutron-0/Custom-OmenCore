# Antivirus False Positives FAQ

## Why Does My Antivirus Flag OmenCore?

OmenCore controls OMEN laptop fans, lighting, telemetry, and tuning through low-level hardware paths. Some security products flag that class of access because it resembles tools that read/write hardware registers.

As of the rebuilt v3.7.0 packages, OmenCore no longer ships or supports the legacy WinRing0 EC backend, and the LibreHardwareMonitor dependency used by `OmenCore.HardwareWorker` has been upgraded to the PawnIO-backed line. Direct EC/MSR access is PawnIO-only.

## Affected Components

| Component | Purpose | Why It May Be Flagged |
| --- | --- | --- |
| PawnIO | Supported EC/MSR hardware access backend | Signed kernel driver for low-level hardware access |
| LibreHardwareMonitor worker | Sensor monitoring fallback | Low-level hardware polling |
| OmenCore binaries | Laptop control and diagnostics | Niche unsigned utility with low reputation on some vendors |

If Windows Defender reports `VulnerableDriver:WinNT/Winring0` against `C:\Program Files\OmenCore\OmenCore.HardwareWorker.sys`, the user is almost certainly running an older 3.7.0 prerelease build or has a leftover file from that build. Reinstall the rebuilt v3.7.0 package, remove the old install folder first, then reboot.

If Windows Defender still reports WinRing0 after that, it is most likely detecting a leftover driver from an older install, LibreHardwareMonitor/another monitoring tool, Intel XTU, ThrottleStop, OmenMon, or another app that installed a WinRing0-compatible service. It should not be coming from the rebuilt v3.7.0 OmenCore EC backend or hardware worker.

## What To Check

1. Remove old OmenCore install folders before installing v3.7.0.
2. Check `C:\Windows\System32\drivers\` for old `WinRing0*.sys` files left by other tools.
3. Check `%TEMP%` for old extracted driver files.
4. Check Intel XTU, ThrottleStop, OmenMon, OpenHardwareMonitor, and LibreHardwareMonitor installs if the alert names WinRing0.
5. If the flagged path is `C:\Program Files\OmenCore\OmenCore.HardwareWorker.sys`, uninstall OmenCore, delete the old install folder, reinstall the rebuilt v3.7.0 package, and reboot.
6. Prefer PawnIO for supported OmenCore EC/MSR features.

## How To Whitelist OmenCore

### Windows Defender

1. Open Windows Security.
2. Go to Virus & threat protection.
3. Open Manage settings.
4. Under Exclusions, choose Add or remove exclusions.
5. Add the OmenCore install folder, normally `C:\Program Files\OmenCore`.

### Bitdefender

Bitdefender may use `Gen:Application.Venus.Cynthia.Winring` for low-level driver access patterns. For OmenCore v3.7.0, the relevant supported driver path is PawnIO:

```text
C:\Program Files\OmenCore\drivers\PawnIO\
```

## Is OmenCore Safe?

OmenCore is open-source. You can inspect the code at:

```text
https://github.com/theantipopau/omencore
```

The supported low-level backend is PawnIO. WinRing0 guidance from old releases is now obsolete for OmenCore itself and for rebuilt v3.7.0 hardware monitoring packages.

## Reporting False Positives

When submitting to an antivirus vendor, describe OmenCore as open-source laptop control software that uses a signed low-level driver for legitimate hardware access. Include the exact file that was flagged so we can distinguish PawnIO, OmenCore, and unrelated leftover WinRing0 artifacts.

*Last updated: 2026-06-01*
