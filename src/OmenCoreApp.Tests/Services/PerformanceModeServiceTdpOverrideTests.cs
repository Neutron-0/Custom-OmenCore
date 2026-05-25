using System.Collections.Generic;
using FluentAssertions;
using OmenCore.Hardware;
using OmenCore.Models;
using OmenCore.Services;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    /// <summary>
    /// Regression tests for roadmap #26 — model-aware TDP override in PerformanceModeService.
    /// Verifies that models with explicit TDP fields in ModelCapabilityDatabase receive the
    /// correct power limits instead of the global config defaults.
    ///
    /// Tests use <see cref="PerformanceModeService.ResolveEffectiveMode"/> to inspect override
    /// logic in isolation without executing any hardware calls.
    /// </summary>
    public class PerformanceModeServiceTdpOverrideTests
    {
        // ─── minimal fan controller stub (no-ops) ────────────────────────────────────

        private sealed class NullFanController : IFanController
        {
            public bool IsAvailable => false;
            public string Status => "null";
            public string Backend => "null";
            public bool ApplyPreset(FanPreset preset) => false;
            public bool ApplyCustomCurve(IEnumerable<FanCurvePoint> curve) => false;
            public bool SetFanSpeed(int percent) => false;
            public bool SetFanSpeeds(int cpu, int gpu) => false;
            public bool SetMaxFanSpeed(bool enabled) => false;
            public bool SetPerformanceMode(string modeName) => false;
            public bool RestoreAutoControl() => false;
            public IEnumerable<FanTelemetry> ReadFanSpeeds() => new[] { new FanTelemetry() };
            public void ApplyMaxCooling() { }
            public void ApplyAutoMode() { }
            public void ApplyQuietMode() { }
            public bool ResetEcToDefaults() => false;
            public bool ApplyThrottlingMitigation() => false;
            public bool VerifyMaxApplied(out string details) { details = ""; return false; }
            public void Dispose() { }
        }

        private sealed class RecordingWmiFanController : IFanController
        {
            public bool IsAvailable => true;
            public string Status => "ok";
            public string Backend => "WMI BIOS";
            public int SetPerformanceModeCallCount { get; private set; }
            public int ApplyCustomCurveCallCount { get; private set; }
            public string? LastPerformanceModeName { get; private set; }

            public bool ApplyPreset(FanPreset preset) => false;
            public bool ApplyCustomCurve(IEnumerable<FanCurvePoint> curve)
            {
                ApplyCustomCurveCallCount++;
                return true;
            }
            public bool SetFanSpeed(int percent) => false;
            public bool SetFanSpeeds(int cpu, int gpu) => false;
            public bool SetMaxFanSpeed(bool enabled) => false;
            public bool RestoreAutoControl() => false;
            public IEnumerable<FanTelemetry> ReadFanSpeeds() => new[] { new FanTelemetry() };
            public void ApplyMaxCooling() { }
            public void ApplyAutoMode() { }
            public void ApplyQuietMode() { }
            public bool ResetEcToDefaults() => false;
            public bool ApplyThrottlingMitigation() => false;
            public bool VerifyMaxApplied(out string details) { details = ""; return false; }
            public void Dispose() { }

            public bool SetPerformanceMode(string modeName)
            {
                SetPerformanceModeCallCount++;
                LastPerformanceModeName = modeName;
                return true;
            }
        }

        // ─── helpers ──────────────────────────────────────────────────────────────────

        private static PerformanceModeService BuildService(ModelCapabilities? caps = null)
        {
            // PowerPlanService and PowerLimitController both need an IEcAccess stub.
            // We pass null — Apply() is never called in these tests (ResolveEffectiveMode
            // is a pure helper, no side effects).
            var log = new LoggingService();
            var fan = new NullFanController();
            var plan = new PowerPlanService(log);
            return new PerformanceModeService(fan, plan, null, log, modelCapabilities: caps);
        }

        private static PerformanceModeService BuildService(IFanController fanController, ModelCapabilities? caps = null)
        {
            var log = new LoggingService();
            var plan = new PowerPlanService(log);
            return new PerformanceModeService(fanController, plan, null, log, modelCapabilities: caps);
        }

        private static PerformanceMode MakePerformanceMode(int cpu = 65, int gpu = 150) =>
            new PerformanceMode { Name = "Performance", CpuPowerLimitWatts = cpu, GpuPowerLimitWatts = gpu };

        private static PerformanceMode MakeBalancedMode(int cpu = 45, int gpu = 100) =>
            new PerformanceMode { Name = "Balanced", CpuPowerLimitWatts = cpu, GpuPowerLimitWatts = gpu };

        private static PerformanceMode MakeEcoMode(int cpu = 25, int gpu = 60) =>
            new PerformanceMode { Name = "Eco", CpuPowerLimitWatts = cpu, GpuPowerLimitWatts = gpu };

        // ─── 16-am1xxx database entry ─────────────────────────────────────────────────

        [Fact]
        public void ModelCapabilityDatabase_Am1xxx_HasExpectedTdpFields()
        {
            var caps = ModelCapabilityDatabase.GetCapabilitiesByModelName("OMEN Gaming Laptop 16-am1001nw");

            caps.Should().NotBeNull("16-am1xxx should match the 16-am1 model-name-pattern entry");
            caps!.PerformanceCpuPl1Watts.Should().Be(90, "OGH reference: 90W PL1 for Performance");
            caps.PerformanceCpuPl2Watts.Should().Be(130, "OGH reference: 130W PL2 for Performance");
            caps.BalancedCpuPl1Watts.Should().Be(55, "OGH reference: 55W for Balanced");
            caps.PerformanceGpuTgpWatts.Should().Be(150);
            caps.UserVerified.Should().BeFalse("product ID not yet community-confirmed");
        }

        // ─── override applied for Performance mode ────────────────────────────────────

        [Fact]
        public void ResolveEffectiveMode_WithAm1xxxCaps_OverridesCpuToModelValue()
        {
            var caps = ModelCapabilityDatabase.GetCapabilitiesByModelName("OMEN Gaming Laptop 16-am1001nw")!;
            var service = BuildService(caps);

            var effective = service.ResolveEffectiveMode(MakePerformanceMode(cpu: 65));

            effective.CpuPowerLimitWatts.Should().Be(90,
                "model-specific 90W PL1 should replace global 65W config default");
        }

        [Fact]
        public void ResolveEffectiveMode_WithAm1xxxCaps_OverridesCpuBoostToModelValue()
        {
            var caps = ModelCapabilityDatabase.GetCapabilitiesByModelName("OMEN Gaming Laptop 16-am1001nw")!;
            var service = BuildService(caps);

            var effective = service.ResolveEffectiveMode(MakePerformanceMode(cpu: 65));

            effective.CpuBoostPowerLimitWatts.Should().Be(130,
                "model-specific 130W PL2 should be carried through to the power-limit writer");
        }

        [Fact]
        public void ResolveEffectiveMode_WithAm1xxxCaps_OverridesGpuToModelValue()
        {
            var caps = ModelCapabilityDatabase.GetCapabilitiesByModelName("OMEN Gaming Laptop 16-am1001nw")!;
            var service = BuildService(caps);

            var effective = service.ResolveEffectiveMode(MakePerformanceMode(gpu: 140));

            effective.GpuPowerLimitWatts.Should().Be(150,
                "model-specific 150W GPU override should replace input value");
        }

        // ─── override applied for Balanced mode ───────────────────────────────────────

        [Fact]
        public void ResolveEffectiveMode_WithAm1xxxCaps_BalancedModeUses55WOverride()
        {
            var caps = ModelCapabilityDatabase.GetCapabilitiesByModelName("OMEN Gaming Laptop 16-am1001nw")!;
            var service = BuildService(caps);

            var effective = service.ResolveEffectiveMode(MakeBalancedMode(cpu: 45));

            effective.CpuPowerLimitWatts.Should().Be(55,
                "Balanced mode should apply model-specific 55W rather than global 45W");
        }

        // ─── no override when model has no TDP fields ─────────────────────────────────

        [Fact]
        public void ResolveEffectiveMode_WithoutModelCaps_ReturnsOriginalValues()
        {
            var service = BuildService(caps: null);

            var effective = service.ResolveEffectiveMode(MakePerformanceMode(cpu: 65, gpu: 150));

            effective.CpuPowerLimitWatts.Should().Be(65,
                "without model capabilities, global config values should pass through unchanged");
            effective.CpuBoostPowerLimitWatts.Should().BeNull();
            effective.GpuPowerLimitWatts.Should().Be(150);
        }

        [Fact]
        public void ResolveEffectiveMode_WithCapsHavingNullTdp_ReturnsOriginalValues()
        {
            var caps = new ModelCapabilities
            {
                ProductId = "XXXX",
                ModelName = "Test Model",
                PerformanceCpuPl1Watts = null,
                PerformanceGpuTgpWatts = null,
                BalancedCpuPl1Watts = null
            };

            var service = BuildService(caps);

            var effective = service.ResolveEffectiveMode(MakePerformanceMode(cpu: 65, gpu: 150));

            effective.CpuPowerLimitWatts.Should().Be(65);
            effective.CpuBoostPowerLimitWatts.Should().BeNull();
            effective.GpuPowerLimitWatts.Should().Be(150);
        }

        [Fact]
        public void ResolveEffectiveMode_WithNonPositiveTdpOverrides_IgnoresInvalidValues()
        {
            var caps = new ModelCapabilities
            {
                ModelName = "Test",
                PerformanceCpuPl1Watts = 0,
                PerformanceCpuPl2Watts = -5,
                PerformanceGpuTgpWatts = 0
            };

            var service = BuildService(caps);

            var effective = service.ResolveEffectiveMode(
                new PerformanceMode
                {
                    Name = "Performance",
                    CpuPowerLimitWatts = 65,
                    CpuBoostPowerLimitWatts = 95,
                    GpuPowerLimitWatts = 140
                });

            effective.CpuPowerLimitWatts.Should().Be(65);
            effective.CpuBoostPowerLimitWatts.Should().Be(95);
            effective.GpuPowerLimitWatts.Should().Be(140);
        }

        // ─── mode name matching is case-insensitive ───────────────────────────────────

        [Theory]
        [InlineData("performance")]
        [InlineData("Performance")]
        [InlineData("PERFORMANCE")]
        public void ResolveEffectiveMode_ModeNameCasing_DoesNotAffectOverride(string modeName)
        {
            var caps = new ModelCapabilities
            {
                ModelName = "Test",
                PerformanceCpuPl1Watts = 90,
                PerformanceGpuTgpWatts = 150
            };

            var service = BuildService(caps);
            var effective = service.ResolveEffectiveMode(
                new PerformanceMode { Name = modeName, CpuPowerLimitWatts = 65, GpuPowerLimitWatts = 140 });

            effective.CpuPowerLimitWatts.Should().Be(90);
        }

        // ─── original mode object is not mutated ─────────────────────────────────────

        [Fact]
        public void ResolveEffectiveMode_DoesNotMutateOriginalMode()
        {
            var caps = new ModelCapabilities { ModelName = "Test", PerformanceCpuPl1Watts = 90 };
            var service = BuildService(caps);
            var originalMode = MakePerformanceMode(cpu: 65);

            service.ResolveEffectiveMode(originalMode);

            originalMode.CpuPowerLimitWatts.Should().Be(65,
                "ResolveEffectiveMode() must not mutate the caller's PerformanceMode instance");
        }

        [Fact]
        public void Apply_LinkDisabled_AndNoValidEcLimits_DoesNotUseWmiThermalPolicyFallback_ByDefault()
        {
            var fan = new RecordingWmiFanController();
            var service = BuildService(fan);

            service.LinkFanToPerformanceMode = false;
            service.Apply(new PerformanceMode
            {
                Name = "Performance",
                CpuPowerLimitWatts = 0,
                GpuPowerLimitWatts = 0
            });

            fan.SetPerformanceModeCallCount.Should().Be(0,
                "performance-mode changes must preserve the decoupled fan policy unless the legacy fallback is explicitly enabled");
            fan.LastPerformanceModeName.Should().BeNull();
        }

        [Fact]
        public void Apply_LinkDisabled_AndNoValidEcLimits_UsesWmiThermalPolicyFallback_WhenExplicitlyEnabled()
        {
            var fan = new RecordingWmiFanController();
            var service = BuildService(fan);

            service.LinkFanToPerformanceMode = false;
            service.AllowDecoupledWmiThermalPolicyFallback = true;
            service.Apply(new PerformanceMode
            {
                Name = "Performance",
                CpuPowerLimitWatts = 0,
                GpuPowerLimitWatts = 0
            });

            fan.SetPerformanceModeCallCount.Should().Be(1,
                "the legacy WMI thermal-policy fallback remains available for explicitly opted-in compatibility profiles");
            fan.LastPerformanceModeName.Should().Be("Performance");
        }

        [Fact]
        public void Constructor_With8D2FModelCapabilities_EnablesWmiThermalPolicyFallback()
        {
            var fan = new RecordingWmiFanController();
            var caps = ModelCapabilityDatabase.GetCapabilities("8D2F");
            var service = BuildService(fan, caps);

            service.AllowDecoupledWmiThermalPolicyFallback.Should().BeTrue();
            service.ControlCapabilityDescription.Should().Contain("WMI Performance Policy Fallback");

            service.LinkFanToPerformanceMode = false;
            service.Apply(new PerformanceMode
            {
                Name = "Performance",
                CpuPowerLimitWatts = 0,
                GpuPowerLimitWatts = 0
            });

            fan.SetPerformanceModeCallCount.Should().Be(1,
                "8D2F keeps performance-profile control through the OEM WMI policy when direct EC/MSR limits are unavailable");
            fan.LastPerformanceModeName.Should().Be("Performance");
        }

        [Fact]
        public void Constructor_With8D41ModelCapabilities_EnablesWmiThermalPolicyFallback()
        {
            var fan = new RecordingWmiFanController();
            var caps = ModelCapabilityDatabase.GetCapabilities("8D41");
            var service = BuildService(fan, caps);

            service.AllowDecoupledWmiThermalPolicyFallback.Should().BeTrue();

            service.LinkFanToPerformanceMode = false;
            service.Apply(new PerformanceMode
            {
                Name = "Performance",
                CpuPowerLimitWatts = 0,
                GpuPowerLimitWatts = 0
            });

            fan.SetPerformanceModeCallCount.Should().Be(1,
                "8D41 must still send the OEM WMI thermal policy when direct EC/MSR power limits report unavailable");
            fan.LastPerformanceModeName.Should().Be("Performance");
        }

        [Fact]
        public void ModeApplied_WhenSubscriberThrows_StillNotifiesRemainingSubscribers()
        {
            var service = BuildService();
            string? delivered = null;

            service.ModeApplied += (_, _) => throw new InvalidOperationException("subscriber failed");
            service.ModeApplied += (_, mode) => delivered = mode;

            var act = () => service.Apply(new PerformanceMode
            {
                Name = "Performance",
                CpuPowerLimitWatts = 65,
                GpuPowerLimitWatts = 100
            });

            act.Should().NotThrow("subscriber failures must not make a confirmed performance apply look failed");
            delivered.Should().Be("Performance");
        }

        [Fact]
        public void Apply_LinkDisabled_AndNoValidEcLimits_DoesNotUseWmiThermalPolicyFallback_ForBalanced()
        {
            var fan = new RecordingWmiFanController();
            var service = BuildService(fan);

            service.LinkFanToPerformanceMode = false;
            service.Apply(new PerformanceMode
            {
                Name = "Balanced",
                CpuPowerLimitWatts = 0,
                GpuPowerLimitWatts = 0
            });

            fan.SetPerformanceModeCallCount.Should().Be(0,
                "Balanced/Default should continue leaving the decoupled fan policy untouched");
        }

        // ─── aliases ("Extreme", "Turbo") map to Performance overrides ───────────────

        [Fact]
        public void Apply_LinkEnabled_DoesNotWriteFanPolicy_WhenModelBlocksFanControl()
        {
            var fan = new RecordingWmiFanController();
            var desktopCaps = new ModelCapabilities
            {
                Family = OmenModelFamily.Desktop,
                SupportsFanControlWmi = false,
                SupportsFanControlEc = false
            };
            var service = BuildService(fan, desktopCaps);

            service.LinkFanToPerformanceMode = true;
            service.Apply(new PerformanceMode
            {
                Name = "Performance",
                CpuPowerLimitWatts = 0,
                GpuPowerLimitWatts = 0
            });

            fan.SetPerformanceModeCallCount.Should().Be(0);
            fan.ApplyCustomCurveCallCount.Should().Be(0);
            service.ControlCapabilityDescription.Should().NotContain("Fan Policy");
        }

        [Fact]
        public void Apply_LegacyWmiThermalFallback_DoesNotWriteFanPolicy_WhenModelBlocksFanControl()
        {
            var fan = new RecordingWmiFanController();
            var desktopCaps = new ModelCapabilities
            {
                Family = OmenModelFamily.Desktop,
                SupportsFanControlWmi = false,
                SupportsFanControlEc = false
            };
            var service = BuildService(fan, desktopCaps);

            service.LinkFanToPerformanceMode = false;
            service.AllowDecoupledWmiThermalPolicyFallback = true;
            service.Apply(new PerformanceMode
            {
                Name = "Performance",
                CpuPowerLimitWatts = 0,
                GpuPowerLimitWatts = 0
            });

            fan.SetPerformanceModeCallCount.Should().Be(0);
            fan.ApplyCustomCurveCallCount.Should().Be(0);
        }

        [Theory]
        [InlineData("extreme")]
        [InlineData("turbo")]
        public void ResolveEffectiveMode_Aliases_MapToPerformanceOverride(string modeName)
        {
            var caps = new ModelCapabilities { ModelName = "Test", PerformanceCpuPl1Watts = 90 };
            var service = BuildService(caps);

            var effective = service.ResolveEffectiveMode(
                new PerformanceMode { Name = modeName, CpuPowerLimitWatts = 65, GpuPowerLimitWatts = 140 });

            effective.CpuPowerLimitWatts.Should().Be(90);
        }

        // ─── eco/quiet aliases map correctly ─────────────────────────────────────────

        [Theory]
        [InlineData("quiet")]
        [InlineData("silent")]
        [InlineData("powersaver")]
        public void ResolveEffectiveMode_EcoAliases_MapToEcoOverride(string modeName)
        {
            var caps = new ModelCapabilities { ModelName = "Test", EcoCpuPl1Watts = 20 };
            var service = BuildService(caps);

            var effective = service.ResolveEffectiveMode(
                new PerformanceMode { Name = modeName, CpuPowerLimitWatts = 25, GpuPowerLimitWatts = 60 });

            effective.CpuPowerLimitWatts.Should().Be(20);
        }
    }
}

