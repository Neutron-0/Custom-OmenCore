using FluentAssertions;
using OmenCore.Services;

namespace OmenCoreApp.Tests.Services;

public class OsdFpsDisplayFormatterTests
{
    [Fact]
    public void FromRtss_UsesInstantFps_WhenAvailable()
    {
        var snapshot = OsdFpsDisplayFormatter.FromRtss(
            instantFps: 144.2f,
            averageFps: 138.4f,
            minFps: 91.8f,
            onePercentLowFps: 92.3f,
            frametimeMs: 6.9f,
            processName: @"C:\Games\TestGame.exe");

        snapshot.Label.Should().Be("FPS");
        snapshot.Display.Should().Be("144");
        snapshot.Detail.Should().Contain("avg 138");
        snapshot.Detail.Should().Contain("1% 92");
        snapshot.Detail.Should().Contain("TestGame.exe");
        snapshot.FrametimeDisplay.Should().Be("6.9ms");
        snapshot.State.Should().Be(OsdFpsDisplayState.Good);
    }

    [Fact]
    public void FromRtss_FallsBackToAverageFps_WhenInstantFpsIsZero()
    {
        var snapshot = OsdFpsDisplayFormatter.FromRtss(
            instantFps: 0,
            averageFps: 59.7f,
            minFps: 45.2f,
            onePercentLowFps: 42.1f,
            frametimeMs: 0,
            processName: "game.exe");

        snapshot.Display.Should().Be("60");
        snapshot.FrametimeDisplay.Should().Be("16.8ms");
        snapshot.Detail.Should().Contain("1% 42");
        snapshot.State.Should().Be(OsdFpsDisplayState.Warning);
    }

    [Fact]
    public void FromRtss_ClassifiesLowFpsAsCritical()
    {
        var snapshot = OsdFpsDisplayFormatter.FromRtss(
            instantFps: 24.4f,
            averageFps: 25.1f,
            minFps: 18.5f,
            onePercentLowFps: 0,
            frametimeMs: 0,
            processName: null);

        snapshot.Display.Should().Be("24");
        snapshot.FrametimeDisplay.Should().Be("41.0ms");
        snapshot.State.Should().Be(OsdFpsDisplayState.Critical);
    }

    [Fact]
    public void FromRtss_ReturnsUnavailable_WhenNoFrameDataExists()
    {
        var snapshot = OsdFpsDisplayFormatter.FromRtss(
            instantFps: 0,
            averageFps: 0,
            minFps: 0,
            onePercentLowFps: 0,
            frametimeMs: 0,
            processName: "");

        snapshot.Display.Should().Be("N/A");
        snapshot.Detail.Should().Be("RTSS waiting for frames");
        snapshot.FrametimeDisplay.Should().Be("--");
        snapshot.State.Should().Be(OsdFpsDisplayState.Unavailable);
    }

    [Fact]
    public void Unavailable_NormalizesBlankDetail()
    {
        var snapshot = OsdFpsDisplayFormatter.Unavailable(" ");

        snapshot.Display.Should().Be("N/A");
        snapshot.Detail.Should().Be("FPS unavailable");
        snapshot.Fps.Should().Be(0);
        snapshot.State.Should().Be(OsdFpsDisplayState.Unavailable);
    }
}
