using System.Reflection;
using FluentAssertions;
using OmenCore.Hardware;
using Xunit;

namespace OmenCoreApp.Tests.Hardware
{
    public class WmiBiosMonitorFallbackTests
    {
        [Theory]
        [InlineData(0, 30)]
        [InlineData(1, 30)]
        [InlineData(2, 60)]
        [InlineData(3, 120)]
        [InlineData(4, 240)]
        [InlineData(5, 300)]
        [InlineData(8, 300)]
        public void CpuFallbackTimeoutCooldown_UsesCappedBackoff(int timeoutStreak, int expectedSeconds)
        {
            var method = typeof(WmiBiosMonitor).GetMethod(
                "CalculateCpuFallbackReadCooldownSeconds",
                BindingFlags.NonPublic | BindingFlags.Static);

            method.Should().NotBeNull();
            var cooldownSeconds = (int)method!.Invoke(null, new object[] { timeoutStreak })!;

            cooldownSeconds.Should().Be(expectedSeconds);
        }

        [Theory]
        [InlineData(79, 30, true)]
        [InlineData(79, 300, true)]
        [InlineData(79, 301, false)]
        [InlineData(0, 30, false)]
        [InlineData(111, 30, false)]
        public void IsRecentWorkerCpuTemperatureUsable_OnlyAcceptsPlausibleRecentWorkerReadings(
            double cpuTemp,
            int ageSeconds,
            bool expected)
        {
            var method = typeof(WmiBiosMonitor).GetMethod(
                "IsRecentWorkerCpuTemperatureUsable",
                BindingFlags.NonPublic | BindingFlags.Static);

            method.Should().NotBeNull();

            var now = new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc);
            var captured = now.AddSeconds(-ageSeconds);
            var usable = (bool)method!.Invoke(null, new object[] { cpuTemp, captured, now })!;

            usable.Should().Be(expected);
        }
    }
}
