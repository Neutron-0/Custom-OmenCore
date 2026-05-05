using FluentAssertions;
using OmenCore.Models;
using OmenCore.ViewModels;
using Xunit;

namespace OmenCoreApp.Tests.ViewModels
{
    public class TuningStatusFormatterTests
    {
        [Fact]
        public void BuildUndervoltStatusText_WhenReadbackMatchesRequested_ReturnsVerifiedMatch()
        {
            var status = new UndervoltStatus
            {
                CurrentCoreOffsetMv = -80,
                CurrentCacheOffsetMv = -80,
                ControlledByOmenCore = true
            };

            var text = TuningStatusFormatter.BuildUndervoltStatusText(-80, -80, status, isAmdCpu: false);

            text.Should().Contain("Requested:");
            text.Should().Contain("Applied:");
            text.Should().Contain("Verified: readback matches requested");
        }

        [Fact]
        public void BuildUndervoltStatusText_WhenExternalControllerPresent_ReturnsBlockedVerification()
        {
            var status = new UndervoltStatus
            {
                CurrentCoreOffsetMv = 0,
                CurrentCacheOffsetMv = 0,
                ExternalController = "Intel XTU"
            };

            var text = TuningStatusFormatter.BuildUndervoltStatusText(-60, -60, status, isAmdCpu: false);

            text.Should().Contain("Verified: blocked by external controller (Intel XTU)");
        }

        [Fact]
        public void BuildGpuOcStatusText_WhenReadbackDiffers_ReturnsMismatchVerification()
        {
            var text = TuningStatusFormatter.BuildGpuOcStatusText(
                requestedCore: 150,
                requestedMemory: 300,
                requestedPower: 110,
                requestedVoltage: 20,
                appliedCore: 100,
                appliedMemory: 250,
                appliedPower: 110,
                appliedVoltage: 20,
                backendAvailable: true,
                allWritesSucceeded: true);

            text.Should().Contain("Requested:");
            text.Should().Contain("Applied:");
            text.Should().Contain("Verified: mismatch between requested and readback");
        }
    }
}
