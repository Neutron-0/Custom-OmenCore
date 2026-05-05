using System;
using System.Linq;
using FluentAssertions;
using OmenCore.Services.Diagnostics;
using Xunit;

namespace OmenCoreApp.Tests.Services;

/// <summary>
/// Regression coverage for BackgroundTimerRegistry.
/// Verifies that UpdateDescription mutates the description in-place without
/// changing the registration timestamp or interval, and that Unregister/Register
/// is still required to change the interval.
/// </summary>
[Collection("NonParallel")]
public class BackgroundTimerRegistryTests : IDisposable
{
    // Clean up any test-created entries after each test.
    private const string TimerName = "Test_BackgroundTimerRegistry_Timer";

    public void Dispose()
    {
        BackgroundTimerRegistry.Unregister(TimerName);
    }

    [Fact]
    public void UpdateDescription_ChangesDescriptionWithoutReRegister()
    {
        BackgroundTimerRegistry.Register(TimerName, "TestService", "original", 1000, BackgroundTimerTier.Optional);
        var before = BackgroundTimerRegistry.GetAll().First(t => t.Name == TimerName);

        BackgroundTimerRegistry.UpdateDescription(TimerName, "updated");

        var after = BackgroundTimerRegistry.GetAll().First(t => t.Name == TimerName);
        after.Description.Should().Be("updated");
        after.IntervalMs.Should().Be(before.IntervalMs);
        after.RegisteredUtc.Should().Be(before.RegisteredUtc);
    }

    [Fact]
    public void UpdateDescription_NoOp_WhenTimerNotRegistered()
    {
        // Should not throw even if the timer name does not exist.
        var act = () => BackgroundTimerRegistry.UpdateDescription("does_not_exist_xyz", "anything");
        act.Should().NotThrow();
    }

    [Fact]
    public void Register_OverwritesPreviousEntry()
    {
        BackgroundTimerRegistry.Register(TimerName, "SvcA", "desc1", 500, BackgroundTimerTier.Critical);
        BackgroundTimerRegistry.Register(TimerName, "SvcA", "desc2", 2000, BackgroundTimerTier.Critical);

        var entry = BackgroundTimerRegistry.GetAll().First(t => t.Name == TimerName);
        entry.IntervalMs.Should().Be(2000);
        entry.Description.Should().Be("desc2");
    }

    [Fact]
    public void Unregister_RemovesEntry()
    {
        BackgroundTimerRegistry.Register(TimerName, "SvcA", "desc", 1000, BackgroundTimerTier.Optional);
        BackgroundTimerRegistry.Unregister(TimerName);

        BackgroundTimerRegistry.GetAll().Should().NotContain(t => t.Name == TimerName);
    }
}
