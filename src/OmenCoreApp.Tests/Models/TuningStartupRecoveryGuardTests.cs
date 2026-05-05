using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    public class TuningStartupRecoveryGuardTests
    {
        [Fact]
        public void Undervolt_ShouldSafeReset_WhenPendingTestApplyTrue()
        {
            var prefs = new UndervoltPreferences
            {
                PendingTestApply = true,
                StartupPendingConfirmation = false
            };

            TuningStartupRecoveryGuard.ShouldSafeReset(prefs).Should().BeTrue();
        }

        [Fact]
        public void Undervolt_ShouldSafeReset_WhenStartupPendingConfirmationTrue()
        {
            var prefs = new UndervoltPreferences
            {
                PendingTestApply = false,
                StartupPendingConfirmation = true
            };

            TuningStartupRecoveryGuard.ShouldSafeReset(prefs).Should().BeTrue();
        }

        [Fact]
        public void Undervolt_ApplySafeReset_ClearsOffsetsAndFlags()
        {
            var prefs = new UndervoltPreferences
            {
                DefaultOffset = new UndervoltOffset { CoreMv = -100, CacheMv = -80 },
                EnablePerCoreUndervolt = true,
                PerCoreOffsetsMv = new int?[] { -75, -75 },
                ApplyOnStartup = true,
                PendingTestApply = true,
                StartupPendingConfirmation = true,
                LastStartupHadUnconfirmedState = false
            };

            TuningStartupRecoveryGuard.ApplySafeReset(prefs);

            prefs.DefaultOffset.CoreMv.Should().Be(0);
            prefs.DefaultOffset.CacheMv.Should().Be(0);
            prefs.EnablePerCoreUndervolt.Should().BeFalse();
            prefs.PerCoreOffsetsMv.Should().BeNull();
            prefs.ApplyOnStartup.Should().BeFalse();
            prefs.PendingTestApply.Should().BeFalse();
            prefs.StartupPendingConfirmation.Should().BeFalse();
            prefs.LastStartupHadUnconfirmedState.Should().BeTrue();
        }

        [Fact]
        public void GpuOc_ShouldSafeReset_WhenEitherPendingFlagSet()
        {
            var settingsA = new GpuOcSettings
            {
                PendingTestApply = true,
                StartupPendingConfirmation = false
            };
            var settingsB = new GpuOcSettings
            {
                PendingTestApply = false,
                StartupPendingConfirmation = true
            };

            TuningStartupRecoveryGuard.ShouldSafeReset(settingsA).Should().BeTrue();
            TuningStartupRecoveryGuard.ShouldSafeReset(settingsB).Should().BeTrue();
        }

        [Fact]
        public void GpuOc_ApplySafeReset_RestoresSafeDefaultsAndClearsFlags()
        {
            var settings = new GpuOcSettings
            {
                CoreClockOffsetMHz = 150,
                MemoryClockOffsetMHz = 500,
                PowerLimitPercent = 115,
                VoltageOffsetMv = 30,
                ApplyOnStartup = true,
                PendingTestApply = true,
                StartupPendingConfirmation = true,
                LastStartupHadUnconfirmedState = false
            };

            TuningStartupRecoveryGuard.ApplySafeReset(settings);

            settings.CoreClockOffsetMHz.Should().Be(0);
            settings.MemoryClockOffsetMHz.Should().Be(0);
            settings.PowerLimitPercent.Should().Be(100);
            settings.VoltageOffsetMv.Should().Be(0);
            settings.ApplyOnStartup.Should().BeFalse();
            settings.PendingTestApply.Should().BeFalse();
            settings.StartupPendingConfirmation.Should().BeFalse();
            settings.LastStartupHadUnconfirmedState.Should().BeTrue();
        }

        [Fact]
        public void ShouldSafeReset_ReturnsFalse_WhenNoPendingFlags()
        {
            var prefs = new UndervoltPreferences
            {
                PendingTestApply = false,
                StartupPendingConfirmation = false
            };
            var settings = new GpuOcSettings
            {
                PendingTestApply = false,
                StartupPendingConfirmation = false
            };

            TuningStartupRecoveryGuard.ShouldSafeReset(prefs).Should().BeFalse();
            TuningStartupRecoveryGuard.ShouldSafeReset(settings).Should().BeFalse();
        }
    }
}
