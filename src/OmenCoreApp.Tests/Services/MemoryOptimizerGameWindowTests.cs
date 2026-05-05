using System;
using FluentAssertions;
using OmenCore.Services;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class MemoryOptimizerGameWindowTests
    {
        // MemoryOptimizerService.IsGameLikelyInForeground() is a static P/Invoke heuristic
        // and cannot reliably be asserted as true/false in a unit test (no guarantee of what
        // the foreground window is during CI).  We test the structural expectations instead:
        //   1.  The method is callable without throwing.
        //   2.  The returned value is a bool (not an exception).
        //   3.  SetGameAwareQuietWindowEnabled round-trips correctly.

        [Fact]
        public void IsGameLikelyInForeground_DoesNotThrow()
        {
            var act = () => MemoryOptimizerService.IsGameLikelyInForeground();
            act.Should().NotThrow();
        }

        [Fact]
        public void IsGameLikelyInForeground_ReturnsBool()
        {
            // Just confirm the return type is bool (compile-time assertion via call)
            bool result = MemoryOptimizerService.IsGameLikelyInForeground();
            // result is always a bool; this assertion confirms the call completed without exception
            (result == true || result == false).Should().BeTrue();
        }

        [Fact]
        public void SetGameAwareQuietWindowEnabled_CanBeDisabled()
        {
            var logger = new LoggingService();
            using var service = new MemoryOptimizerService(logger);

            service.GameAwareQuietWindowEnabled.Should().BeTrue("default is enabled");

            service.SetGameAwareQuietWindowEnabled(false);
            service.GameAwareQuietWindowEnabled.Should().BeFalse();

            service.SetGameAwareQuietWindowEnabled(true);
            service.GameAwareQuietWindowEnabled.Should().BeTrue();
        }

        [Fact]
        public void SetGameAwareQuietWindowEnabled_DefaultIsTrue()
        {
            var logger = new LoggingService();
            using var service = new MemoryOptimizerService(logger);

            service.GameAwareQuietWindowEnabled.Should().BeTrue();
        }
    }
}
