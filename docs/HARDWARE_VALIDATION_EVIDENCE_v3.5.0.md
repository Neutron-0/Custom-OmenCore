# Hardware Validation Evidence - v3.5.0

## Purpose
This document is the release-gate evidence template for physical hardware validation in v3.5.0.

Use this file to close Gate D and Gate E items referenced in `docs/CHANGELOG_v3.5.0.md`.

## How To Use
1. Copy one full "Validation Run" block per device/OS scenario.
2. Keep command output snippets exact (do not paraphrase pass/fail lines).
3. If a check fails, keep the failure evidence and add mitigation/follow-up.
4. Only mark a gate item PASS when the corresponding criteria below is met.

## Required Gate D Scenarios
- Windows OMEN 16-xd0xxx / ProductId 8BCD (fan hold + RGB)
- Linux OMEN board 8D40 (performance hold persistence)
- At least one tray/minimized telemetry-cadence measurement run on a real Windows machine

## Global Pass Criteria
- Max fan hold and requested-vs-confirmed state behavior are stable for the validation window.
- RGB static apply/restore is physically verified on target affected hardware.
- Linux hold mode remains active without external shell loops.
- Cadence optimization impact (CPU/RAM) is measured and recorded.

---

## Validation Run Template

### Run Metadata
- Date (UTC):
- Tester:
- Device model:
- ProductId/Board:
- BIOS version:
- OS + build/kernel:
- AC power state during run:
- OmenCore build/version:
- Related issue(s):

### Pre-Run Conditions
- OMEN Gaming Hub running: yes/no
- OMEN Light Studio running: yes/no
- Any external fan/RGB/tuning tools running:
- Clean reboot before run: yes/no

### Commands Executed
```powershell
# Windows example
# Add exact commands used
```

```bash
# Linux example
# Add exact commands used
```

### Evidence Snippets (verbatim)
- Test/build result snippet:
- Relevant log snippet(s):
- Diagnostic export files captured:

### Scenario A - Max Fan Hold on Windows (8BCD class)
- Step summary:
  - Apply Max from fan page
  - Apply Max from tray quick action
  - Transition through Auto/Balanced/Performance and return to Max
  - Observe for at least 2 minutes
- Expected:
  - Max remains held unless explicit external reset occurs
  - If reset occurs, requested vs confirmed state and reset reason are visible
- Actual:
- Status: PASS / FAIL
- Notes:

### Scenario B - Cross-Surface State Agreement (Windows)
- Surfaces checked:
  - Sidebar
  - OMEN/System page
  - Fan page
  - Tray header/checkmarks
  - Persisted startup restore result
- Expected:
  - Confirmed state is consistent across all surfaces
- Actual:
- Status: PASS / FAIL
- Notes:

### Scenario C - RGB Physical Validation (4-zone affected hardware)
- Step summary:
  - Close OMEN Light Studio
  - Apply visible static color
  - Trigger failed verification path if reproducible
  - Confirm restore path does not leave keyboard dark
- Expected:
  - Visible color applies or restores to last known good state
- Actual:
- Status: PASS / FAIL
- Notes:

### Scenario D - Linux Hold Persistence (board 8D40)
- Step summary:
  - Apply `perf --mode performance --hold`
  - Observe mode persistence >= 10 minutes
  - Validate behavior across daemon loop intervals
- Expected:
  - No manual shell loop required
  - Drift reapply only when needed
- Actual:
- Status: PASS / FAIL
- Notes:

### Scenario E - Linux Capability/Permission Messaging
- Step summary:
  - Run status/perf paths under expected permission contexts
  - Verify backend capability warnings for unsupported thermal-power writes
- Expected:
  - Clear root/write/capability reason text; no silent fake-success
- Actual:
- Status: PASS / FAIL
- Notes:

### Scenario F - Tray/Minimized Cadence Measurement
- Step summary:
  - Measure baseline CPU/RAM in active window mode
  - Measure tray/minimized idle with no curve/hold
  - Measure tray/minimized with active hold/curve
- Expected:
  - Reduced polling overhead in tray-only scenario
  - Correct cadence override when hold/overlay requires faster sampling
- Actual (record exact numbers):
  - Active mode CPU/RAM:
  - Tray-only CPU/RAM:
  - Tray with hold/curve CPU/RAM:
- Status: PASS / FAIL
- Notes:

### Final Run Outcome
- Gate D items closed by this run:
- Remaining blockers:
- Recommended follow-up actions:

---

## Aggregated Signoff Checklist (fill after all runs)
- [ ] Max fan hold validated on target affected Windows hardware
- [ ] Cross-surface requested/confirmed agreement validated on hardware
- [ ] 4-zone RGB physical apply/restore validated on affected hardware
- [ ] Linux board 8D40 hold persistence validated
- [ ] Linux capability/warning messaging validated under real permission contexts
- [ ] Tray/minimized cadence CPU/RAM measurements recorded and accepted
- [ ] All blocking Gate D evidence attached in this file

## Release Recommendation
- Gate D status: PASS / BLOCKED
- Gate E status: PASS / BLOCKED
- Recommendation: ship / hold
