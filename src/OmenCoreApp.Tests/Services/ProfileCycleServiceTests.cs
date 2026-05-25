using FluentAssertions;
using OmenCore.Services;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class ProfileCycleServiceTests
    {
        [Theory]
        [InlineData(null, false, "Performance")]
        [InlineData("", false, "Performance")]
        [InlineData("Balanced", false, "Performance")]
        [InlineData("Performance", false, "Quiet")]
        [InlineData("Quiet", false, "Balanced")]
        [InlineData("Custom", false, "Balanced")]
        [InlineData("Quiet", true, "Custom")]
        [InlineData("Custom", true, "Balanced")]
        public void ResolveNextPerformanceProfile_UsesDeterministicCycle(string? current, bool hasCustomCurve, string expected)
        {
            ProfileCycleService.ResolveNextPerformanceProfile(current, hasCustomCurve)
                .Should().Be(expected);
        }

        [Fact]
        public void ResolveNextFanMode_SkipsCustom_WhenNoCustomCurveExists()
        {
            var next = ProfileCycleService.ResolveNextFanMode("extreme", false, null, out var targetMode);

            next.Should().Be("Quiet");
            targetMode.Should().Be("Quiet");
        }

        [Fact]
        public void ResolveNextFanMode_UsesCustomTarget_WhenCustomCurveExists()
        {
            var next = ProfileCycleService.ResolveNextFanMode("Extreme", true, "Field curve", out var targetMode);

            next.Should().Be("Custom");
            targetMode.Should().Be("Field curve");
        }
    }
}
