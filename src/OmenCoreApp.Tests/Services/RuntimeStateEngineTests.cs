using System;
using FluentAssertions;
using OmenCore.Services;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class RuntimeStateEngineTests
    {
        [Fact]
        public void PublishProjection_WhenSubscriberThrows_StillNotifiesRemainingSubscribers()
        {
            var engine = new RuntimeStateEngine();
            RuntimeStateSnapshot? delivered = null;

            engine.StateChanged += (_, _) => throw new InvalidOperationException("surface failed");
            engine.StateChanged += (_, snapshot) => delivered = snapshot;

            engine.PublishProjection("Gaming", "Performance", "Gaming", isFanPerformanceLinked: true);

            delivered.Should().NotBeNull();
            delivered!.FanMode.Should().Be("Gaming");
            delivered.PerformanceMode.Should().Be("Performance");
            delivered.CurvePresetName.Should().Be("Gaming");
            delivered.IsFanPerformanceLinked.Should().BeTrue();
        }

        [Fact]
        public void PublishProjection_WhenStateUnchanged_DoesNotNotifySubscribers()
        {
            var engine = new RuntimeStateEngine();
            var notifications = 0;

            engine.StateChanged += (_, _) => notifications++;

            engine.PublishProjection("Auto", "Balanced", "Auto", isFanPerformanceLinked: false);

            notifications.Should().Be(0);
        }
    }
}
