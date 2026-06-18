using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    public class StartupRestorePolicyTests
    {
        [Fact]
        public void IsEnabled_ReturnsFalseForEveryCategory_WhenGlobalGateDisabled()
        {
            var config = new AppConfig
            {
                EnableStartupHardwareRestore = false,
                StartupRestoreFansEnabled = true,
                StartupRestorePerformanceEnabled = true,
                StartupRestoreRgbEnabled = true,
                StartupRestoreTuningEnabled = true
            };

            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Fans).Should().BeFalse();
            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Performance).Should().BeFalse();
            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Rgb).Should().BeFalse();
            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Tuning).Should().BeFalse();
        }

        [Fact]
        public void IsEnabled_TreatsMissingCategoryFlagsAsEnabled_WhenGlobalGateEnabled()
        {
            var config = new AppConfig
            {
                EnableStartupHardwareRestore = true,
                StartupRestoreFansEnabled = null,
                StartupRestorePerformanceEnabled = null,
                StartupRestoreRgbEnabled = null,
                StartupRestoreTuningEnabled = null
            };

            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Fans).Should().BeTrue();
            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Performance).Should().BeTrue();
            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Rgb).Should().BeTrue();
            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Tuning).Should().BeTrue();
        }

        [Fact]
        public void IsEnabled_HonorsExplicitCategoryOptOuts()
        {
            var config = new AppConfig
            {
                EnableStartupHardwareRestore = true,
                StartupRestoreFansEnabled = true,
                StartupRestorePerformanceEnabled = false,
                StartupRestoreRgbEnabled = false,
                StartupRestoreTuningEnabled = true
            };

            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Fans).Should().BeTrue();
            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Performance).Should().BeFalse();
            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Rgb).Should().BeFalse();
            StartupRestorePolicy.IsEnabled(config, StartupRestoreCategory.Tuning).Should().BeTrue();
            StartupRestorePolicy.BuildSummary(config).Should().Be("Fans=on; Performance=off; RGB=off; Tuning=on");
        }
    }
}
