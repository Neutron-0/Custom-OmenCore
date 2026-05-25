# PawnIO Driver Files

This directory contains files needed for PawnIO-based EC/MSR access.

## What Is PawnIO?

PawnIO is a digitally signed kernel driver that provides low-level hardware access while being compatible with Secure Boot-oriented systems. It is OmenCore's supported backend for direct EC/MSR access.

## Installation

### Option 1: Install PawnIO (Recommended)

1. Download the official PawnIO installer from https://pawnio.eu/
2. Run the installer.
3. Restart Windows if the driver or modules do not activate immediately.
4. OmenCore will automatically detect and use PawnIO for supported direct hardware access.

### Option 2: Manual Module Placement

If PawnIO is already installed but modules are not in the default location:

1. Download `LpcACPIEC.amx` from https://github.com/namazso/PawnIO.Modules/releases
2. Place it in `%ProgramFiles%\PawnIO\modules\` or OmenCore's `drivers` folder.
3. Restart OmenCore.

## Why PawnIO?

| Feature | PawnIO |
| --- | --- |
| Secure Boot-oriented systems | Supported backend |
| Signed driver | Yes |
| OmenCore EC/MSR access | Supported |
| HVCI-oriented installs | Preferred path |

## Benefits

- Secure Boot compatible path for supported low-level hardware access.
- Signed driver, so test-signing mode is not required.
- Current OmenCore backend for direct EC/MSR features.
- Modular architecture through PawnIO modules.

## Module: LpcACPIEC

The `LpcACPIEC.amx` module provides access to the ACPI Embedded Controller through standard ports `0x62` (data) and `0x66` (command/status). This enables supported direct EC features such as fan speed control, keyboard backlight control, performance mode switching, and temperature sensor reading.

## Troubleshooting

### PawnIO Not Detected

1. Ensure PawnIO is installed from https://pawnio.eu/
2. Check that the PawnIO service is running.
3. Verify `%ProgramFiles%\PawnIO\PawnIOLib.dll` exists.
4. Reboot if PawnIO was just installed.

### Module Load Failed

1. Download the latest `LpcACPIEC.amx` from the PawnIO module releases.
2. Place it in `%ProgramFiles%\PawnIO\modules\`.
3. Restart OmenCore.

### Access Denied Errors

1. Run OmenCore as Administrator.
2. Ensure no other software is using the EC.
3. Reboot after driver installation or service changes.

## Licensing

PawnIO is available under multiple licenses:

- Official binary: proprietary redistribution terms.
- Open source: GPL 2 with exception for the driver, LGPL 2.1 for the library.

Users should download PawnIO from https://pawnio.eu/ for the best experience.
