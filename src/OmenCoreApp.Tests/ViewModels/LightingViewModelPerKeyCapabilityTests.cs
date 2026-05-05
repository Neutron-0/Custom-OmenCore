using FluentAssertions;
using OmenCore.ViewModels;
using Xunit;

namespace OmenCoreApp.Tests.ViewModels
{
    public class LightingViewModelPerKeyCapabilityTests
    {
        [Fact]
        public void BuildPerKeyCapabilitySummary_WhenPerKeyEditorActive_ReturnsActiveMessage()
        {
            var summary = LightingViewModel.BuildPerKeyCapabilitySummary(
                isPerKeyLightingAvailable: true,
                isPerKeyHardwareCapable: true);

            summary.Should().Contain("editor is active");
        }

        [Fact]
        public void BuildPerKeyCapabilitySummary_WhenHardwareCapableButEditorUnavailable_ReturnsBackendUnavailableMessage()
        {
            var summary = LightingViewModel.BuildPerKeyCapabilitySummary(
                isPerKeyLightingAvailable: false,
                isPerKeyHardwareCapable: true);

            summary.Should().Contain("capable hardware detected");
            summary.Should().Contain("cannot open the per-key editor");
            summary.Should().Contain("zone lighting remains available");
        }

        [Fact]
        public void BuildPerKeyCapabilitySummary_WhenNotCapable_ReturnsUnsupportedHardwareMessage()
        {
            var summary = LightingViewModel.BuildPerKeyCapabilitySummary(
                isPerKeyLightingAvailable: false,
                isPerKeyHardwareCapable: false);

            summary.Should().Contain("does not support per-key control");
        }
    }
}
