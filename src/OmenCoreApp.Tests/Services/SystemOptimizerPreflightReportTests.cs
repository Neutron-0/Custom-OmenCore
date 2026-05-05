using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OmenCore.Services;
using OmenCore.Services.SystemOptimizer;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class SystemOptimizerPreflightReportTests
    {
        [Fact]
        public async Task GeneratePreflightReportAsync_IncludesHighRiskByDefault()
        {
            var logger = new LoggingService();
            var service = new SystemOptimizerService(logger, () => true);

            var report = await service.GeneratePreflightReportAsync();

            report.Items.Should().NotBeEmpty();
            report.HasHighRisk.Should().BeTrue();
            report.HighRiskItems.Should().Contain(i => i.Id == "storage_short_names");
        }

        [Fact]
        public async Task GeneratePreflightReportAsync_ExcludeHighRisk_RemovesHighRiskItems()
        {
            var logger = new LoggingService();
            var service = new SystemOptimizerService(logger, () => true);

            var report = await service.GeneratePreflightReportAsync(includeHighRisk: false);

            report.HasHighRisk.Should().BeFalse();
            report.Items.Should().OnlyContain(i => i.Risk != OptimizationRisk.High);
            report.Items.Should().NotContain(i => i.Id == "storage_short_names");
        }

        [Fact]
        public async Task GeneratePreflightReportAsync_ProvidesRiskAndWarningSummary()
        {
            var logger = new LoggingService();
            var service = new SystemOptimizerService(logger, () => true);

            var report = await service.GeneratePreflightReportAsync();

            report.LowRiskCount.Should().BeGreaterThan(0);
            report.MediumRiskCount.Should().BeGreaterThan(0);
            report.Warnings.Should().NotBeEmpty();
            report.Warnings.Should().Contain(w => w.Contains("not recommended on battery"));
        }
    }
}
