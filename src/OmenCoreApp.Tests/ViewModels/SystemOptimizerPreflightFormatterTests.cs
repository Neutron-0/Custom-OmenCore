using FluentAssertions;
using OmenCore.Services.SystemOptimizer;
using OmenCore.ViewModels;
using Xunit;

namespace OmenCoreApp.Tests.ViewModels
{
    public class SystemOptimizerPreflightFormatterTests
    {
        [Fact]
        public void BuildSummary_IncludesRiskCountsAndRebootFlag()
        {
            var report = new PreflightReport
            {
                Items = new[]
                {
                    new PreflightItem { Id = "a", Risk = OptimizationRisk.Low, RequiresReboot = false },
                    new PreflightItem { Id = "b", Risk = OptimizationRisk.Medium, RequiresReboot = true },
                    new PreflightItem { Id = "c", Risk = OptimizationRisk.High, RequiresReboot = false },
                }
            };

            var text = SystemOptimizerPreflightFormatter.BuildSummary(report);

            text.Should().Contain("Low 1");
            text.Should().Contain("Medium 1");
            text.Should().Contain("High 1");
            text.Should().Contain("Reboot: Yes");
        }

        [Fact]
        public void BuildWarningRollup_UsesLimitAndReportsRemainder()
        {
            var report = new PreflightReport
            {
                Items = new[] { new PreflightItem { Id = "x", Risk = OptimizationRisk.Low } }
            };
            report.Items[0].Warning = "warning one";

            var report2 = new PreflightReport
            {
                Items = new[]
                {
                    new PreflightItem { Id = "a", Risk = OptimizationRisk.Low, Warning = "warning one" },
                    new PreflightItem { Id = "b", Risk = OptimizationRisk.Medium, Warning = "warning two" },
                    new PreflightItem { Id = "c", Risk = OptimizationRisk.High, Warning = "warning three" },
                }
            };

            var rollup = SystemOptimizerPreflightFormatter.BuildWarningRollup(report2, maxWarnings: 2);

            rollup.Should().Contain("warning one");
            rollup.Should().Contain("warning two");
            rollup.Should().Contain("(+1 more)");
        }
    }
}
