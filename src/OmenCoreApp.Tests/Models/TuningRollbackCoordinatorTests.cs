using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    public class TuningRollbackCoordinatorTests
    {
        [Fact]
        public void ApplySafeRollback_NormalizesPersistedTuningState()
        {
            var config = new AppConfig
            {
                LastPerformanceModeName = "Performance",
                LastGpuPowerBoostLevel = "Maximum",
                LastFanPresetName = "Max",
                LastTccOffset = 12,
                LastCpuPl1Watts = 85,
                LastCpuPl2Watts = 120,
                Undervolt = new UndervoltPreferences
                {
                    DefaultOffset = new UndervoltOffset { CoreMv = -110, CacheMv = -90 },
                    EnablePerCoreUndervolt = true,
                    PerCoreOffsetsMv = new int?[] { -80, null, -70 },
                    ApplyOnStartup = true,
                    PendingTestApply = true,
                    StartupPendingConfirmation = true,
                    LastStartupHadUnconfirmedState = true
                },
                GpuOc = new GpuOcSettings
                {
                    CoreClockOffsetMHz = 120,
                    MemoryClockOffsetMHz = 450,
                    PowerLimitPercent = 115,
                    VoltageOffsetMv = 25,
                    ApplyOnStartup = true,
                    PendingTestApply = true,
                    StartupPendingConfirmation = true,
                    LastStartupHadUnconfirmedState = true
                },
                AmdPowerLimits = new AmdPowerLimits
                {
                    StapmLimitWatts = 45,
                    TempLimitC = 88
                }
            };

            var outcome = TuningRollbackCoordinator.ApplySafeRollback(config, startupPl1Watts: 45, startupPl2Watts: 65);

            outcome.ConfigChanged.Should().BeTrue();
            outcome.ChangedAreas.Should().Contain(new[]
            {
                "Performance mode",
                "GPU power boost",
                "Fan preset",
                "TCC offset",
                "CPU power limits",
                "CPU undervolt",
                "GPU OC",
                "AMD power limits"
            });

            config.LastPerformanceModeName.Should().Be("Balanced");
            config.LastGpuPowerBoostLevel.Should().Be("Minimum");
            config.LastFanPresetName.Should().Be("Auto");
            config.LastTccOffset.Should().Be(0);
            config.LastCpuPl1Watts.Should().Be(45);
            config.LastCpuPl2Watts.Should().Be(65);

            config.Undervolt.DefaultOffset.CoreMv.Should().Be(0);
            config.Undervolt.DefaultOffset.CacheMv.Should().Be(0);
            config.Undervolt.EnablePerCoreUndervolt.Should().BeFalse();
            config.Undervolt.PerCoreOffsetsMv.Should().BeNull();
            config.Undervolt.ApplyOnStartup.Should().BeFalse();
            config.Undervolt.PendingTestApply.Should().BeFalse();
            config.Undervolt.StartupPendingConfirmation.Should().BeFalse();
            config.Undervolt.LastStartupHadUnconfirmedState.Should().BeFalse();

            config.GpuOc!.CoreClockOffsetMHz.Should().Be(0);
            config.GpuOc.MemoryClockOffsetMHz.Should().Be(0);
            config.GpuOc.PowerLimitPercent.Should().Be(100);
            config.GpuOc.VoltageOffsetMv.Should().Be(0);
            config.GpuOc.ApplyOnStartup.Should().BeFalse();
            config.GpuOc.PendingTestApply.Should().BeFalse();
            config.GpuOc.StartupPendingConfirmation.Should().BeFalse();
            config.GpuOc.LastStartupHadUnconfirmedState.Should().BeFalse();

            config.AmdPowerLimits!.StapmLimitWatts.Should().Be(25);
            config.AmdPowerLimits.TempLimitC.Should().Be(95);
        }

        [Fact]
        public void ApplySafeRollback_ClearsSavedCpuPowerLimits_WhenStartupValuesUnknown()
        {
            var config = new AppConfig
            {
                LastCpuPl1Watts = 70,
                LastCpuPl2Watts = 100
            };

            var outcome = TuningRollbackCoordinator.ApplySafeRollback(config);

            outcome.CpuPowerLimitsReset.Should().BeTrue();
            config.LastCpuPl1Watts.Should().BeNull();
            config.LastCpuPl2Watts.Should().BeNull();
        }

        [Fact]
        public void ApplySafeRollback_NoChanges_WhenAlreadySafe()
        {
            var config = new AppConfig
            {
                LastPerformanceModeName = "Balanced",
                LastGpuPowerBoostLevel = "Minimum",
                LastFanPresetName = "Auto",
                LastTccOffset = 0,
                LastCpuPl1Watts = 45,
                LastCpuPl2Watts = 65,
                Undervolt = new UndervoltPreferences
                {
                    DefaultOffset = new UndervoltOffset { CoreMv = 0, CacheMv = 0 }
                },
                GpuOc = new GpuOcSettings(),
                AmdPowerLimits = new AmdPowerLimits()
            };

            var outcome = TuningRollbackCoordinator.ApplySafeRollback(config, startupPl1Watts: 45, startupPl2Watts: 65);

            outcome.ConfigChanged.Should().BeFalse();
            outcome.ChangedAreas.Should().BeEmpty();
        }
    }
}
