using FluentAssertions;
using OmenCore.ViewModels;
using Xunit;

namespace OmenCoreApp.Tests.ViewModels
{
    /// <summary>
    /// Regression tests for the RGB ownership visibility strip properties added in 3.5.0.
    /// These test the static/pure logic paths that can be exercised without hardware.
    /// </summary>
    public class LightingViewModelOwnershipTests
    {
        [Fact]
        public void RgbConflictWarningText_WhenNoConflict_ReturnsEmptyString()
        {
            // The static DetectRgbConflictProcess method is private, but HasRgbConflictWarning
            // and RgbConflictWarningText are computed properties on LightingViewModel.
            // On a clean CI machine, neither OGH nor OMEN Light Studio should be running,
            // so the warning text should be empty.
            // This test validates the property contract (empty = no conflict).
            // On a machine with OGH running, HasRgbConflictWarning may be true — that is
            // expected behavior, not a test failure.
            var vm = BuildMinimalLightingViewModel();
            if (!vm.HasRgbConflictWarning)
            {
                vm.RgbConflictWarningText.Should().BeEmpty(
                    because: "when no conflict process is detected the warning text should be empty");
            }
            else
            {
                vm.RgbConflictWarningText.Should().NotBeEmpty(
                    because: "when HasRgbConflictWarning is true the text should describe the conflict");
            }
        }

        [Fact]
        public void HasRgbConflictWarning_AndText_AreConsistent()
        {
            var vm = BuildMinimalLightingViewModel();
            if (vm.HasRgbConflictWarning)
                vm.RgbConflictWarningText.Should().NotBeEmpty();
            else
                vm.RgbConflictWarningText.Should().BeEmpty();
        }

        [Fact]
        public void RgbOwnershipSummary_WhenNoControllersAvailable_ReturnsNoneDetectedMessage()
        {
            // With no services wired, the summary should say nothing is detected.
            var vm = BuildMinimalLightingViewModel();
            vm.RgbOwnershipSummary.Should().Be("No active RGB controllers detected");
        }

        [Fact]
        public void HpKeyboardActiveBackend_WhenNoServiceWired_ReturnsNotAvailable()
        {
            var vm = BuildMinimalLightingViewModel();
            vm.HpKeyboardActiveBackend.Should().Be("N/A",
                because: "when KeyboardLightingService is not provided the backend should report N/A");
        }

        private static LightingViewModel BuildMinimalLightingViewModel()
        {
            // Construct with null services — tests the no-hardware code path.
            var logging = new OmenCore.Services.LoggingService();
            return new LightingViewModel(
                corsairService: null,
                logitechService: null,
                logging: logging);
        }
    }
}
