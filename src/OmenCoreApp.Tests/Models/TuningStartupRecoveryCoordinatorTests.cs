using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    public class TuningStartupRecoveryCoordinatorTests
    {
        [Fact]
        public void Recover_ResetsCpuAndGpu_WhenBothPending()
        {
            var config = new AppConfig
            {
                Undervolt = new UndervoltPreferences
                {
                    DefaultOffset = new UndervoltOffset { CoreMv = -90, CacheMv = -70 },
                    ApplyOnStartup = true,
                    PendingTestApply = true,
                    StartupPendingConfirmation = true
                },
                GpuOc = new GpuOcSettings
                {
                    CoreClockOffsetMHz = 120,
                    MemoryClockOffsetMHz = 400,
                    PowerLimitPercent = 112,
                    VoltageOffsetMv = 20,
                    ApplyOnStartup = true,
                    PendingTestApply = true,
                    StartupPendingConfirmation = true
                }
            };

            var outcome = TuningStartupRecoveryCoordinator.Recover(config);

            outcome.ConfigChanged.Should().BeTrue();
            outcome.CpuUndervoltReset.Should().BeTrue();
            outcome.GpuOcReset.Should().BeTrue();

            config.Undervolt.DefaultOffset.CoreMv.Should().Be(0);
            config.Undervolt.DefaultOffset.CacheMv.Should().Be(0);
            config.Undervolt.ApplyOnStartup.Should().BeFalse();

            config.GpuOc.Should().NotBeNull();
            config.GpuOc!.CoreClockOffsetMHz.Should().Be(0);
            config.GpuOc.MemoryClockOffsetMHz.Should().Be(0);
            config.GpuOc.PowerLimitPercent.Should().Be(100);
            config.GpuOc.VoltageOffsetMv.Should().Be(0);
            config.GpuOc.ApplyOnStartup.Should().BeFalse();
        }

        [Fact]
        public void Recover_ResetsOnlyCpu_WhenGpuNotPending()
        {
            var config = new AppConfig
            {
                Undervolt = new UndervoltPreferences
                {
                    DefaultOffset = new UndervoltOffset { CoreMv = -80, CacheMv = -50 },
                    PendingTestApply = true
                },
                GpuOc = new GpuOcSettings
                {
                    CoreClockOffsetMHz = 90,
                    MemoryClockOffsetMHz = 250,
                    PowerLimitPercent = 108,
                    VoltageOffsetMv = 10,
                    PendingTestApply = false,
                    StartupPendingConfirmation = false
                }
            };

            var outcome = TuningStartupRecoveryCoordinator.Recover(config);

            outcome.ConfigChanged.Should().BeTrue();
            outcome.CpuUndervoltReset.Should().BeTrue();
            outcome.GpuOcReset.Should().BeFalse();
            config.GpuOc!.CoreClockOffsetMHz.Should().Be(90);
            config.GpuOc.MemoryClockOffsetMHz.Should().Be(250);
            config.GpuOc.PowerLimitPercent.Should().Be(108);
        }

        [Fact]
        public void Recover_NoChanges_WhenNoPendingState()
        {
            var config = new AppConfig
            {
                Undervolt = new UndervoltPreferences
                {
                    PendingTestApply = false,
                    StartupPendingConfirmation = false
                },
                GpuOc = new GpuOcSettings
                {
                    PendingTestApply = false,
                    StartupPendingConfirmation = false
                }
            };

            var outcome = TuningStartupRecoveryCoordinator.Recover(config);

            outcome.ConfigChanged.Should().BeFalse();
            outcome.CpuUndervoltReset.Should().BeFalse();
            outcome.GpuOcReset.Should().BeFalse();
        }

        [Fact]
        public void Recover_HandlesNullGpuOc()
        {
            var config = new AppConfig
            {
                Undervolt = new UndervoltPreferences
                {
                    PendingTestApply = true
                },
                GpuOc = null
            };

            var outcome = TuningStartupRecoveryCoordinator.Recover(config);

            outcome.ConfigChanged.Should().BeTrue();
            outcome.CpuUndervoltReset.Should().BeTrue();
            outcome.GpuOcReset.Should().BeFalse();
        }
    }
}
