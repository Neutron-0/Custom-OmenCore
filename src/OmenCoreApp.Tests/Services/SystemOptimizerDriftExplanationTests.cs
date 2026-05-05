using System.Linq;
using FluentAssertions;
using OmenCore.Services.SystemOptimizer;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class SystemOptimizerDriftExplanationTests
    {
        // Helper: a fully-applied expected state
        private static OptimizationState FullyApplied() => new()
        {
            Power = new PowerOptimizationState
            {
                UltimatePerformancePlan = true,
                HardwareGpuScheduling = true,
                GameModeEnabled = true,
                ForegroundPriority = true,
            },
            Services = new ServiceOptimizationState
            {
                TelemetryDisabled = true,
                SysMainDisabled = true,
                SearchIndexingDisabled = true,
                DiagTrackDisabled = true,
            },
            Network = new NetworkOptimizationState
            {
                TcpNoDelay = true,
                TcpAckFrequency = true,
                DeliveryOptimizationDisabled = true,
                NagleDisabled = true,
            },
            Input = new InputOptimizationState
            {
                MouseAccelerationDisabled = true,
                GameDvrDisabled = true,
                GameBarDisabled = true,
                FullscreenOptimizationsDisabled = true,
            },
            Visual = new VisualOptimizationState
            {
                AnimationsDisabled = true,
                TransparencyDisabled = true,
            },
            Storage = new StorageOptimizationState
            {
                TrimEnabled = true,
                DefragDisabled = true,
                ShortNamesDisabled = true,
                LastAccessDisabled = true,
            }
        };

        [Fact]
        public void NoDrift_WhenExpectedAndActualMatch()
        {
            var state = FullyApplied();
            var result = SystemOptimizerService.GetDriftExplanations(state, state);

            result.HasDrift.Should().BeFalse();
            result.DriftedItems.Should().BeEmpty();
            result.OneLinerSummary.Should().BeEmpty();
        }

        [Fact]
        public void SysMain_DriftDetected_WithWindowsUpdateExplanation()
        {
            var expected = FullyApplied();
            var actual = FullyApplied();
            actual.Services.SysMainDisabled = false; // simulates Windows Update re-enabling it

            var result = SystemOptimizerService.GetDriftExplanations(expected, actual);

            result.HasDrift.Should().BeTrue();
            result.DriftCount.Should().Be(1);
            var item = result.DriftedItems.Single();
            item.Id.Should().Be("service_sysmain");
            item.Category.Should().Be("Services");
            item.Explanation.Should().Contain("Windows Update");
        }

        [Fact]
        public void PowerPlan_DriftDetected_WhenUltimatePerformanceMissing()
        {
            var expected = FullyApplied();
            var actual = FullyApplied();
            actual.Power.UltimatePerformancePlan = false;

            var result = SystemOptimizerService.GetDriftExplanations(expected, actual);

            result.HasDrift.Should().BeTrue();
            result.DriftedItems.Should().Contain(i => i.Id == "power_ultimate_performance");
        }

        [Fact]
        public void Nagle_DriftDetected_WhenTcpNoDelayReset()
        {
            var expected = FullyApplied();
            var actual = FullyApplied();
            actual.Network.TcpNoDelay = false;
            actual.Network.NagleDisabled = false;

            var result = SystemOptimizerService.GetDriftExplanations(expected, actual);

            result.DriftedItems.Should().Contain(i => i.Id == "network_tcp_nodelay");
            result.DriftedItems.Should().Contain(i => i.Id == "network_nagle");
            result.Explanation_ContainsNetworkDriverHint();
        }

        [Fact]
        public void HAGS_DriftDetected_WithDriverResetExplanation()
        {
            var expected = FullyApplied();
            var actual = FullyApplied();
            actual.Power.HardwareGpuScheduling = false;

            var result = SystemOptimizerService.GetDriftExplanations(expected, actual);

            result.DriftedItems.Should().Contain(i => i.Id == "power_gpu_scheduling");
            result.DriftedItems
                .Single(i => i.Id == "power_gpu_scheduling")
                .Explanation.Should().ContainAny("driver", "HAGS", "registry");
        }

        [Fact]
        public void MultipleDrifts_AllReported()
        {
            var expected = FullyApplied();
            var actual = FullyApplied();
            actual.Services.SysMainDisabled = false;
            actual.Network.TcpNoDelay = false;
            actual.Visual.AnimationsDisabled = false;

            var result = SystemOptimizerService.GetDriftExplanations(expected, actual);

            result.DriftCount.Should().Be(3);
            result.OneLinerSummary.Should().Contain("3");
        }

        [Fact]
        public void NoDrift_WhenExpectedIsFalseAndActualIsFalse()
        {
            // Expected was NOT applied for this field; actual is also false — no drift
            var expected = FullyApplied();
            expected.Services.SysMainDisabled = false;
            var actual = FullyApplied();
            actual.Services.SysMainDisabled = false;

            var result = SystemOptimizerService.GetDriftExplanations(expected, actual);

            result.DriftedItems.Should().NotContain(i => i.Id == "service_sysmain");
        }

        [Fact]
        public void NoDrift_WhenActualExceedsExpected_IsNotReported()
        {
            // Expected was false but actual is true — not a drift (beneficial extra state)
            var expected = FullyApplied();
            expected.Services.SysMainDisabled = false;
            var actual = FullyApplied();
            actual.Services.SysMainDisabled = true; // extra; not a drift

            var result = SystemOptimizerService.GetDriftExplanations(expected, actual);

            result.DriftedItems.Should().NotContain(i => i.Id == "service_sysmain");
        }
    }

    // Fluent assertion extension to keep tests readable
    internal static class OptimizationDriftSummaryExtensions
    {
        public static void Explanation_ContainsNetworkDriverHint(this OptimizationDriftSummary summary)
        {
            var hasHint = summary.DriftedItems.Any(i =>
                i.Explanation.Contains("driver", System.StringComparison.OrdinalIgnoreCase) ||
                i.Explanation.Contains("network", System.StringComparison.OrdinalIgnoreCase));
            hasHint.Should().BeTrue("at least one network drift explanation should mention drivers or network stack");
        }
    }
}
