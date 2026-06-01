# Windows Defender False Positive Notes

## Current Status

The rebuilt OmenCore v3.7.0 packages no longer ship or support the legacy WinRing0 EC backend. Direct EC/MSR access is PawnIO-only, and the hardware worker now uses the PawnIO-backed LibreHardwareMonitor package.

If Defender shows `VulnerableDriver:WinNT/Winring0` on `C:\Program Files\OmenCore\OmenCore.HardwareWorker.sys`, the installed build is older than the rebuilt v3.7.0 packages or the file was left behind during upgrade. Remove the old install folder, reinstall the rebuilt package, and reboot.

If Defender still shows a WinRing0 alert after reinstalling, it is probably detecting a leftover driver file or another hardware utility rather than the current OmenCore EC backend or hardware worker.

## Common Sources Of WinRing0 Alerts

- Old files under `C:\Windows\System32\drivers\WinRing0*.sys`
- Temporary extracted driver files under `%TEMP%`
- Intel XTU, ThrottleStop, OmenMon, OpenHardwareMonitor, or LibreHardwareMonitor
- Older OmenCore folders that were not removed before upgrading
- Older OmenCore prerelease packages that extracted `OmenCore.HardwareWorker.sys`

## Recommended Action

1. Install the current v3.7.0 build.
2. Remove old OmenCore install folders.
3. Reboot after removing old driver files or uninstalling conflicting tools.
4. Use PawnIO for supported EC/MSR access.
5. If an alert remains, capture the exact flagged path and detection name in a diagnostic report.

## Supported Driver Direction

PawnIO is the supported direct hardware backend because it is compatible with Secure Boot-oriented systems and avoids the legacy WinRing0 backend path that caused repeated Defender confusion. Rebuilt v3.7.0 packages also avoid the older LibreHardwareMonitor worker path that could create `OmenCore.HardwareWorker.sys`.
