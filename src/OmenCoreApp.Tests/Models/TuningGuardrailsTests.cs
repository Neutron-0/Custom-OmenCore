using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    public class TuningGuardrailsTests
    {
        [Theory]
        [InlineData(-250, -150)]
        [InlineData(-75, -75)]
        [InlineData(25, 0)]
        public void ClampCpuUndervoltMv_Intel_StaysWithinSafeUndervoltRange(double requested, double expected)
        {
            TuningGuardrails.ClampCpuUndervoltMv(requested, amdCurveOptimizer: false)
                .Should().Be(expected);
        }

        [Theory]
        [InlineData(-250, -120)]
        [InlineData(-96, -96)]
        [InlineData(10, 0)]
        public void ClampCpuUndervoltMv_AmdCurveOptimizer_StaysWithinCoEquivalentRange(double requested, double expected)
        {
            TuningGuardrails.ClampCpuUndervoltMv(requested, amdCurveOptimizer: true)
                .Should().Be(expected);
        }

        [Fact]
        public void ClampCpuUndervoltOffset_ClampsGlobalAndPerCoreOffsets()
        {
            var offset = new UndervoltOffset
            {
                CoreMv = -300,
                CacheMv = 15,
                PerCoreOffsetsMv = new int?[] { -250, -100, 20, null }
            };

            var safe = TuningGuardrails.ClampCpuUndervoltOffset(offset, amdCurveOptimizer: false);

            safe.CoreMv.Should().Be(-150);
            safe.CacheMv.Should().Be(0);
            safe.PerCoreOffsetsMv.Should().Equal(-150, -100, 0, null);
        }

        [Theory]
        [InlineData(-500, -200)]
        [InlineData(-75, -75)]
        [InlineData(250, 100)]
        public void ClampGpuVoltageOffsetMv_StaysWithinProviderRange(int requested, int expected)
        {
            TuningGuardrails.ClampGpuVoltageOffsetMv(requested).Should().Be(expected);
        }
    }
}
