using System;
using FluentAssertions;
using OmenCore.Services;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class MemoryOptimizerServiceAutoCleanTests
    {
        [Fact]
        public void EvaluateAutoCleanDecision_WaitsForSustainedPressure()
        {
            var now = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
            var info = CreatePressureInfo();

            var decision = MemoryOptimizerService.EvaluateAutoCleanDecision(
                info,
                thresholdPercent: 80,
                MemoryAutoCleanProfile.Balanced,
                now,
                pressureSinceUtc: DateTime.MinValue,
                lastCleanAtUtc: DateTime.MinValue);

            decision.ShouldClean.Should().BeFalse();
            decision.IsThrottled.Should().BeTrue();
            decision.PressureSinceUtc.Should().Be(now);
            decision.Reason.Should().Contain("sustained memory pressure");
        }

        [Fact]
        public void EvaluateAutoCleanDecision_CleansAfterPressureGrace()
        {
            var now = new DateTime(2026, 5, 1, 12, 1, 0, DateTimeKind.Utc);
            var pressureSince = now.AddSeconds(-25);
            var info = CreatePressureInfo();

            var decision = MemoryOptimizerService.EvaluateAutoCleanDecision(
                info,
                thresholdPercent: 80,
                MemoryAutoCleanProfile.Balanced,
                now,
                pressureSince,
                lastCleanAtUtc: DateTime.MinValue);

            decision.ShouldClean.Should().BeTrue();
            decision.IsThrottled.Should().BeFalse();
            decision.PressureSinceUtc.Should().Be(pressureSince);
            decision.Reason.Should().Contain("memory 88% load");
        }

        [Fact]
        public void EvaluateAutoCleanDecision_SkipsDuringCooldown()
        {
            var now = new DateTime(2026, 5, 1, 12, 5, 0, DateTimeKind.Utc);
            var info = CreatePressureInfo();

            var decision = MemoryOptimizerService.EvaluateAutoCleanDecision(
                info,
                thresholdPercent: 80,
                MemoryAutoCleanProfile.Balanced,
                now,
                pressureSinceUtc: now.AddMinutes(-2),
                lastCleanAtUtc: now.AddMinutes(-1));

            decision.ShouldClean.Should().BeFalse();
            decision.IsThrottled.Should().BeTrue();
            decision.Reason.Should().Contain("cooldown active");
        }

        [Fact]
        public void EvaluateAutoCleanDecision_RequiresActionablePressureBeyondLoadSnapshot()
        {
            var now = new DateTime(2026, 5, 1, 12, 10, 0, DateTimeKind.Utc);
            var info = new MemoryInfo
            {
                TotalPhysicalMB = 32768,
                AvailablePhysicalMB = 8192,
                MemoryLoadPercent = 82,
                CommitTotalMB = 16000,
                CommitLimitMB = 32768
            };

            var decision = MemoryOptimizerService.EvaluateAutoCleanDecision(
                info,
                thresholdPercent: 80,
                MemoryAutoCleanProfile.Balanced,
                now,
                pressureSinceUtc: now.AddMinutes(-2),
                lastCleanAtUtc: DateTime.MinValue);

            decision.ShouldClean.Should().BeFalse();
            decision.IsThrottled.Should().BeFalse();
            decision.PressureSinceUtc.Should().Be(DateTime.MinValue);
            decision.Reason.Should().Contain("below actionable threshold");
        }

        private static MemoryInfo CreatePressureInfo()
        {
            return new MemoryInfo
            {
                TotalPhysicalMB = 16384,
                AvailablePhysicalMB = 1536,
                MemoryLoadPercent = 88,
                CommitTotalMB = 14500,
                CommitLimitMB = 16384
            };
        }
    }
}
