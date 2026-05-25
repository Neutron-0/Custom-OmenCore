<div align="center">

<img src="docs/screenshots/githublogo.png" alt="OmenCore Logo" width="520" />

# OmenCore

## A Modern, Lightweight Control Center for HP OMEN & Victus Gaming Laptops

</div>

---

**OmenCore** is an **independent control center** for HP OMEN and Victus laptops. It runs without OMEN Gaming Hub installed, avoids bloatware/outbound telemetry/ads, and uses local WMI BIOS, EC, and platform backends where the hardware exposes them.

### Why OmenCore?

| Feature | Status |
|---------|--------|
| **100% OGH-Independent** | ✅ Works without OMEN Gaming Hub installed |
| **Zero Bloatware** | ✅ Self-contained artifacts, no runtime installs |
| **No Outbound Telemetry** | ✅ Diagnostics and config stay on your machine |
| **Ad-Free** | ✅ Clean, focused interface |
| **Offline Operation** | ✅ No sign-in required, fully local control |
| **Cross-Platform** | ✅ Windows WPF + Linux CLI & Avalonia GUI |

---

### ⚡ Quick Links

[![Version](https://img.shields.io/badge/version-3.7.0-red.svg?style=for-the-badge)](docs/CHANGELOG_v3.7.0.md)
[![License](https://img.shields.io/badge/license-MIT-green.svg?style=for-the-badge)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg?style=for-the-badge)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Discord](https://img.shields.io/badge/Discord-Join%20Server-5865F2.svg?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/9WhJdabGk8)
[![Donate](https://img.shields.io/badge/Donate-PayPal-00457C.svg?style=for-the-badge&logo=paypal&logoColor=white)](https://www.paypal.com/donate/?business=XH8CKYF8T7EBU&no_recurring=0&item_name=Thank+you+for+your+generous+donation%2C+this+will+allow+me+to+continue+developing+my+programs.&currency_code=AUD)

---

### 📸 Interface Preview

![OmenCore Main Window](docs/screenshots/main-window.png)

## 🚀 **Quick Start**

### Windows

1. Open the [Releases](https://github.com/theantipopau/omencore/releases) page
2. Download the latest `OmenCoreSetup-<version>.exe` or `OmenCore-<version>-win-x64.zip`
3. Run OmenCore as Administrator

See the [Full Installation Guide](INSTALL.md#-windows-installation).

### Linux (CachyOS • Arch • Ubuntu • Fedora)

```bash
# Replace VERSION with the published release you want to install.
VERSION=<release-version>
wget "https://github.com/theantipopau/omencore/releases/download/v${VERSION}/OmenCore-${VERSION}-linux-x64.zip"
mkdir -p OmenCore-linux-x64
unzip "OmenCore-${VERSION}-linux-x64.zip" -d OmenCore-linux-x64
cd OmenCore-linux-x64
chmod +x omencore-cli omencore-gui

# CLI: Check status
sudo ./omencore-cli status

# GUI: Launch Avalonia
sudo ./omencore-gui
```

See the [Complete Linux Guide](docs/LINUX_INSTALL_GUIDE.md) or the [Quick Reference](INSTALL.md#-linux-installation).

### Linux issue reporting (one-command triage bundle)

When reporting Linux model support issues (for example missing `hp-wmi` fan interfaces), run:

```bash
./qa/collect-linux-triage.sh
```

This generates a timestamped folder with:
- `omencore-linux-triage.txt` (kernel/OS, CLI status/diagnose output, sysfs snapshots)
- optional `acpidump.dat` when `acpidump` is available

Attach those files to your GitHub issue for faster triage.

## 🔥 **What's New in v3.7.0**

v3.7.0 is the current stabilization release, focused on deterministic fan/profile behavior, clearer telemetry truth, OMEN MAX 16 power/RGB identity fixes, Linux diagnose improvements, and release hardening after the v3.6.x safety line.

### v3.7.0 Highlights

- **Profile/fan authority fixes** for OMEN 16 `8BCD`, including restored high-temperature curve endpoints and General profile sync.
- **OMEN MAX 16 `8D41` recovery** with WMI thermal-policy fallback and exact keyboard identity so RTX 50-series systems no longer fall through unknown paths.
- **Linux diagnose improvements** for board `8787`, board `8C30`, hp-wmi profile-path variants, and safer fan-curve handling when sensors disappear.
- **Telemetry truthfulness** with measured-only dashboard power, no synthetic OSD load fallback, and structured backend health reporting.
- **PawnIO-only low-level access cleanup** after removing the remaining WinRing0 fallback paths.

### Release Notes

Current release is **v3.7.0**.

→ **[v3.7.0 Changelog](docs/CHANGELOG_v3.7.0.md)**

→ **[v3.6.3 Changelog](docs/CHANGELOG_v3.6.3.md)**

→ **[v3.6.2 Changelog](docs/CHANGELOG_v3.6.2.md)**

→ **[v3.6.1 Changelog](docs/CHANGELOG-3.6.1.md)**

---

## 📦 **Downloads & Artifacts**

**Version:** v3.7.0 | **Status:** Release Candidate rebuild pending

Release artifact names:

| Download | Platform | Details |
|----------|----------|----------|
| **OmenCoreSetup-3.7.0.exe** | Windows | Installer (Recommended) — Includes .NET 8 runtime |
| **OmenCore-3.7.0-win-x64.zip** | Windows | Portable — Extract and run, no installation |
| **OmenCore-3.7.0-linux-x64.zip** | Linux | CLI + Avalonia GUI, self-contained runtime |

### SHA256

Pending rebuild after the late GitHub #134 CPU-temperature authority fix.

---

## 🔧 **Features**

### Thermal & Fan Management

- Custom fan curves with temperature breakpoints — CPU and GPU fans controlled independently
- WMI BIOS control — no driver required, works on AMD and Intel models
- EC-backed presets (Max, Auto, Manual) for instant fan switching
- Real-time monitoring with live CPU/GPU temperature history charts
- Per-fan telemetry — RPM and duty cycle for each cooling zone
- System tray badge — live CPU temperature on the notification icon
- CPU Temperature Limit — TCC offset control (Intel only)
- Fan preset save/load — name, export, import, and share `.omencore` profiles
- 0% duty remapping — curve interpolation can never stall fans below the configured minimum (v3.2.0)

### Performance Control

- CPU undervolting via Intel MSR with independent core/cache offset sliders (typical safe range: -80 to -125 mV)
- Performance modes (Quiet, Balanced, Performance, Turbo) — CPU/GPU wattage envelope management (decoupled from fan mode in v3.3.0)
- GPU Power Boost — +15W Dynamic Boost (PPAB)
- GPU mux switching — Hybrid, Discrete (dGPU), and Integrated (iGPU)
- Per-game profiles — auto-switch on game process detection
- External tool detection — defers MSR control when ThrottleStop/Intel XTU is active

### RGB Lighting

- Keyboard lighting profiles — Static, Breathing, Wave, Reactive (multi-zone)
- 4-zone OMEN keyboards with per-zone color and intensity
- Per-key RGB on OMEN Max 16 (individual key addressing)
- Peripheral sync — apply themes to Corsair/Logitech/Razer devices
- Linux sysfs-based RGB capability detection (v3.2.0)

### Hardware Monitoring

- Real-time telemetry — CPU/GPU temp, load, clocks, RAM usage, SSD temp
- Telemetry state model: `Valid`, `Inactive`, `Unavailable`, `Stale`, `Degraded`, `Invalid`
- Dashboard banners for Stale and Degraded states with contextual messaging (v3.2.0)
- Rolling 60-sample history charts with 0.5° / 0.5% change threshold
- Low overhead mode — disables charts; reduces idle CPU from ~2% to <0.5%

### System Optimization

- HP OMEN Gaming Hub removal — guided cleanup with dry-run mode
- Gaming Mode — one-click service/animation toggle
- Battery care — adjustable charge limit (60–100%)
- OSD in-game overlay — click-through, configurable metrics
- Memory optimizer — smart/deep RAM clean using Windows native API
- Bloatware scanner — AppX detection, startup item manager, scheduled task cleaner

### Auto-Update

- Polls GitHub Releases every 6 hours
- SHA256 verification required (updates rejected without hash for security)
- One-click download with progress indicator and integrity validation
- Manual fallback if SHA256 is absent from release notes

---

## 🎮 HP Gaming Hub Feature Parity

OmenCore is designed to replace the core local-control workflows of OMEN Gaming Hub on supported hardware.

| HP Gaming Hub Feature | OmenCore | Notes |
|----------------------|---------|-------|
| Fan Control | Supported models | Custom curves + WMI BIOS/EC presets where firmware exposes control |
| Performance Modes | Supported models | CPU/GPU power envelope via WMI/profile backends where available |
| CPU Undervolting | Intel-supported systems | Intel MSR with safety clamping; hidden when runtime access is blocked |
| GPU Power Boost | Supported OMEN models | +15W Dynamic Boost (PPAB) where BIOS exposes it |
| Keyboard RGB | Supported keyboards | Per-zone + per-key on supported models |
| Hardware Monitoring | ✅ Full | LibreHardwareMonitor integration |
| Gaming Mode | ✅ Full | Service/animation optimization |
| Battery Care | Supported models | Adjustable charge limit where firmware exposes it |
| Peripheral Control | Beta | Corsair/Logitech/Razer hardware detection ready |
| Hub Cleanup | ✅ Exclusive | Safe OGH removal tool |
| Per-Game Profiles | ✅ Full | Auto-switch on process detection |
| In-Game Overlay | ✅ Full | Click-through OSD |
| Network Booster | ✅ Out of scope | Use router/Windows QoS |
| Game Library | ✅ Out of scope | Use Steam/Epic/Xbox app |
| Omen Oasis | ✅ Out of scope | Cloud gaming out of scope |

**OmenCore covers the essential local-control Gaming Hub workflows on supported OMEN/Victus hardware** with better performance, no outbound telemetry, no ads, and full offline operation. Unsupported or unverified features are gated clearly rather than presented as guaranteed.

---

## 📋 Requirements

### System

- **OS:** Windows 10 (build 19041+) or Windows 11
- **Runtime:** Self-contained — .NET 8 embedded, no separate installation needed
- **Privileges:** Administrator for WMI BIOS/EC/MSR operations
- **Disk:** ~120 MB for app + ~50 MB logs/config
- **OGH:** NOT required — OmenCore works without OMEN Gaming Hub

### Hardware

- **CPU:** Intel 6th-gen+ for undervolting/TCC offset; AMD Ryzen supported for monitoring and fan control
- **Laptop:** HP OMEN 15/16/17 series and HP Victus (2019–2025 models)
  - ? Tested: OMEN 15-dh, 16-b, 16-k, 17-ck (2023/2024), Victus 15/16
  - ? OMEN Max 16 (2025): per-key RGB, RTX 50-series, full support
  - ? OMEN Transcend 14/16: WMI BIOS support
  - ? 2023+ models: full WMI BIOS support, no OGH needed
- **Desktop:** HP OMEN 25L/30L/40L/45L (limited support; monitoring, profiles, and OGH cleanup functional)

### Fan Control Backend Priority

1. **WMI BIOS** (default) — no driver, works on all OMEN laptops
2. **EC via PawnIO** — Secure Boot compatible
3. **OGH Proxy** — last resort fallback

### Optional Drivers

- **PawnIO** — recommended for advanced EC/MSR access (Secure Boot compatible)

> **Antivirus note:** Some AV products flag low-level hardware drivers as suspicious. Current OmenCore builds use PawnIO for direct EC/MSR access; WinRing0 alerts usually indicate older leftovers or another hardware utility. See [ANTIVIRUS_FAQ.md](docs/ANTIVIRUS_FAQ.md) and [DEFENDER_FALSE_POSITIVE.md](docs/DEFENDER_FALSE_POSITIVE.md).

**Compatibility:**
- HP Spectre: fan control and monitoring work; CPU/GPU power limits unavailable (different EC layout)
- HP Victus: fan control, monitoring, and keyboard backlight work; GPU TGP/PPAB and CPU undervolting unavailable (BIOS does not expose these on Victus)
- Non-OMEN HP laptops: monitoring only
- Other brands: not supported
- Virtual machines: monitoring-only mode

---

## 🏗️ Architecture

**Stack:** .NET 8.0 / WPF (Windows) / Avalonia (Linux) / LibreHardwareMonitor / EC Direct / Intel MSR

```
OmenCore/
+-- src/OmenCoreApp/              # Windows WPF app (ViewModels, Views, Services, Controls)
+-- src/OmenCore.HardwareWorker/  # Out-of-process hardware worker — crash isolation
+-- src/OmenCore.Avalonia/        # Avalonia cross-platform UI (ViewModels, Services)
+-- src/OmenCore.Desktop/         # Archived prototype (not part of OmenCore.sln shipping builds)
+-- src/OmenCore.Linux/           # Linux hardware: hp-wmi, ec_sys, sysfs RGB probing
+-- installer/                    # Inno Setup script
+-- config/                       # default_config.json
+-- docs/                         # Changelogs, audit reports, guides
+-- VERSION.txt                   # Current release/version marker
```

**Principles:** Safety-first EC write allowlist · Async by default · Telemetry change-detection (0.5°/0.5%) · Graceful per-service degradation · Out-of-process crash isolation

---

## 🛠️ Development

### Requirements

- Visual Studio 2022 (Community+), workload: .NET Desktop Development
- .NET 8 SDK — [download](https://dotnet.microsoft.com/download/dotnet/8.0)
- Inno Setup (installer only) — [download](https://jrsoftware.org/isdl.php)

### Build

```powershell
git clone https://github.com/theantipopau/omencore.git
cd omencore
dotnet restore OmenCore.sln
dotnet build OmenCore.sln --configuration Release

# Run (Administrator required)
cd src\OmenCoreApp\bin\Release\net8.0-windows10.0.19041.0
.\OmenCore.exe
```

### Build Installer

```powershell
pwsh ./build-installer.ps1
# Optional: -Configuration Release -Runtime win-x64 (these are the defaults)
# Outputs: artifacts/OmenCoreSetup-3.7.0.exe and artifacts/OmenCore-3.7.0-win-x64.zip
```

### Tests

```powershell
dotnet test OmenCore.sln
dotnet test OmenCore.sln --collect:"XPlat Code Coverage"
```

### Linux triage bundle (maintainers/reporters)

```bash
./qa/collect-linux-triage.sh [output_dir] [bin_dir]
# Example:
./qa/collect-linux-triage.sh ./triage ./
```

### Release Process

1. Update `VERSION.txt`
2. Add changelog under `docs/CHANGELOG_vX.Y.Z.md`
3. Tag and push: `git tag vX.Y.Z && git push origin main --tags`
4. Include SHA256 hash in GitHub Release notes — required for the in-app auto-updater

---

## 🔧 Troubleshooting

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| Fan control has no effect | WMI not supported on this model | Try PawnIO/ec_sys mode; check logs |
| Access denied errors | Not running as Administrator | Right-click ? Run as administrator |
| PawnIO not detected | Driver missing or awaiting reboot | Install PawnIO and restart Windows |
| Undervolting not working | MSR locked in BIOS | Check BIOS overclocking settings; verify with Intel XTU |
| Auto-update fails | SHA256 missing from release notes | Download manually from the Releases page |
| High CPU at idle | Charts polling too aggressively | Enable Low Overhead Mode in Dashboard settings |
| Linux: permission denied | Hardware access needs root | Run with `sudo` |
| Linux: ec_sys not found | Module not in this kernel | Use `hp-wmi` on 2023+ models |

Detailed logs are in `%LOCALAPPDATA%\OmenCore\`. On Linux, use `sudo omencore-cli --report > report.txt` for a diagnostics bundle.

> **AMD undervolting:** Ryzen does not support Intel-style MSR undervolting. Use BIOS Curve Optimizer or Ryzen Master. OmenCore still provides full fan control, monitoring, RGB, and performance modes on AMD systems.

---

## 📜 Version History

| Version | Key Changes |
|---------|------------|
| **v3.7.0** | Runtime recovery, fan/profile authority fixes, OMEN MAX 16 8D41 power/RGB identity, Linux diagnose improvements, telemetry truthfulness, and PawnIO-only cleanup |
| **v3.6.3** | Safety hotfix rollup: desktop fan-write gate, conservative WMI fan handoff, OSD startup hardening, 8D2F identity correction, UI polish, and runtime CPU/RAM reductions |
| **v3.6.2** | Stabilization release: runtime source-of-truth hardening, fan/performance confirmation fixes, RGB fallback reliability, Linux diagnostics/package updates, and UI responsiveness cleanup |
| **v3.6.1** | Stabilization release: fan/performance sync, tray/OSD consistency, WMI fan CPU reduction, EC coordination, capability fallback hardening |
| **v3.6.0** | Lightweight runtime behavior, hardware-worker reliability, fan/RGB/hotkey hardening, and release packaging improvements |
| **v3.5.0** | Reliability release: fan/tuning diagnostics clarity, requested-vs-confirmed UI hardening, conflict/recovery safety guardrails, and roadmap split for deferred scope |
| **v3.4.1** | Hotfix for fan/profile regressions, brightness hotkeys, RGB reliability, Linux startup diagnostics, and 15-en0038ur support |
| **v3.4.0** | Correctness and reliability sweep: fan/power fixes, update safety hardening, CI/package alignment, model/support matrix expansion |
| **v3.3.0** | Fan curve stability, sleep recovery, OSD DPI/visual, RGB hardening, AMD power tuning, Lite Mode (74 items) |
| **v3.2.5** | Worker reconnect fix, fan/performance decoupling, 8BB1 model support, Quick Access improvements |
| **v3.2.1** | 23-fix hotfix rollup: telemetry hardening, OSD/premium UI polish, portable log hygiene, CPU temp oscillation guard |
| **v3.2.0** | Dashboard row fix, fan 0% safety, frozen temp watchdog, Avalonia preset save, Linux RGB detection |
| **v3.1.1** | CPU temp regression (17-ck1xxx), fan 0-RPM guard, worker crash on GPU driver install, PE header validation |
| **v3.1.0** | Telemetry state model, sleep/suspend fan hardening (#77), OMEN MAX 16 CPU temp override (#78) |
| **v3.0.2** | Hotfix: PE header validation, WinRing0 hash check |
| **v3.0.0** | Multi-project architecture, out-of-process HardwareWorker, full Avalonia Linux GUI |
| **v2.9.0** | Intel Core Ultra CPU temp fix, EC write reduction, memory optimizer, Afterburner coexistence |
| **v2.8.0** | AMD GPU OC (ADL2), OMEN desktop support, game library, Linux hwmon PWM control |

Older release notes: [docs/](docs/)

---

## 📚 Documentation

- [INSTALL.md](INSTALL.md) — Full installation guide for Windows and Linux
- [docs/LINUX_INSTALL_GUIDE.md](docs/LINUX_INSTALL_GUIDE.md) — Detailed Linux setup
- [docs/ANTIVIRUS_FAQ.md](docs/ANTIVIRUS_FAQ.md) — Antivirus false positive handling
- [docs/DEFENDER_FALSE_POSITIVE.md](docs/DEFENDER_FALSE_POSITIVE.md) — Windows Defender exclusion steps
- [drivers/PawnIO/README.md](drivers/PawnIO/README.md) — PawnIO direct hardware backend setup
- [docs/CHANGELOG_v3.7.0.md](docs/CHANGELOG_v3.7.0.md) — Current stabilization changelog
- [docs/3.7.0-RC-VALIDATION.md](docs/3.7.0-RC-VALIDATION.md) — v3.7.0 field validation checklist
- [docs/CHANGELOG_v3.6.3.md](docs/CHANGELOG_v3.6.3.md) — Prior safety and stability hotfix changelog
- [docs/CHANGELOG_v3.6.2.md](docs/CHANGELOG_v3.6.2.md) — Prior stabilization release changelog
- [docs/CHANGELOG-3.6.1.md](docs/CHANGELOG-3.6.1.md) — Earlier stabilization release changelog

---

## 🤝 Contributing

Contributions welcome! Priority areas:

- [ ] Corsair iCUE / Logitech G HUB SDK implementations (replace stubs)
- [ ] EC register maps for models not yet in the allowlist
- [ ] Testing on OMEN Max 16/17 (2025) with RTX 50-series
- [ ] Testing on OMEN 15-en, 16-n series
- [ ] Localization / translations

---

## ⚠️ Disclaimer

This software is provided "as is" without warranty. Modifying EC registers, undervolting, and mux switching can potentially damage hardware. Always create system restore points before making changes. The developers are not responsible for hardware damage, data loss, or warranty voids. HP does not endorse this project; use at your own risk.

---

## 🔗 Links

- **GitHub:** https://github.com/theantipopau/omencore
- **Releases:** https://github.com/theantipopau/omencore/releases/latest
- **Issues:** https://github.com/theantipopau/omencore/issues
- **Discord:** https://discord.gg/9WhJdabGk8
- **Donate:** https://www.paypal.com/donate/?business=XH8CKYF8T7EBU

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for details.

**Third-party components:**
- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) — MPL 2.0
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) — CPOL
- PawnIO driver and modules — see [drivers/PawnIO/README.md](drivers/PawnIO/README.md)

---

*Made with care for the HP OMEN community.*
