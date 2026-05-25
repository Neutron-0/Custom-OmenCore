using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class PowerAutomationServiceTests
    {
        [Fact]
        public void BuiltInPerformanceCurve_ReachesMaxAtSeventyFiveC()
        {
            var curve = FanModeNameResolver.BuildBuiltInCurve("Performance", FanMode.Performance)
                .OrderBy(p => p.TemperatureC)
                .ToList();

            curve.Single(p => p.FanPercent == 100).TemperatureC.Should().Be(75,
                "power automation fallback Performance should match the restored aggressive cooling endpoint");
        }
    }
}
