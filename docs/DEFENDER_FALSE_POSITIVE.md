# Windows Defender False Positive Notes

## Current Status

OmenCore v3.7.0 no longer ships or supports the legacy WinRing0 EC backend. Direct EC/MSR access is PawnIO-only.

If Defender still shows a WinRing0 alert, it is probably detecting a leftover driver file or another hardware utility rather than the current OmenCore EC backend.

## Common Sources Of WinRing0 Alerts

- Old files under `C:\Windows\System32\drivers\WinRing0*.sys`
- Temporary extracted driver files under `%TEMP%`
- Intel XTU, ThrottleStop, OmenMon, OpenHardwareMonitor, or LibreHardwareMonitor
- Older OmenCore folders that were not removed before upgrading

## Recommended Action

1. Install the current v3.7.0 build.
2. Remove old OmenCore install folders.
3. Reboot after removing old driver files or uninstalling conflicting tools.
4. Use PawnIO for supported EC/MSR access.
5. If an alert remains, capture the exact flagged path and detection name in a diagnostic report.

## Supported Driver Direction

PawnIO is the supported direct hardware backend because it is compatible with Secure Boot-oriented systems and avoids the legacy WinRing0 backend path that caused repeated Defender confusion.
