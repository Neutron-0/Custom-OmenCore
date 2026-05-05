using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    public class FanModeNameResolverTests
    {
        [Theory]
        [InlineData("max", FanMode.Max)]
        [InlineData("maximum", FanMode.Max)]
        [InlineData("performance", FanMode.Performance)]
        [InlineData("turbo", FanMode.Performance)]
        [InlineData("gaming", FanMode.Performance)]
        [InlineData("extreme", FanMode.Performance)]
        [InlineData("boost", FanMode.Performance)]
        [InlineData("quiet", FanMode.Quiet)]
        [InlineData("silent", FanMode.Quiet)]
        [InlineData("cool", FanMode.Quiet)]
        [InlineData("battery", FanMode.Quiet)]
        [InlineData("manual", FanMode.Manual)]
        [InlineData("custom", FanMode.Manual)]
        [InlineData("balanced", FanMode.Auto)]
        [InlineData("auto", FanMode.Auto)]
        [InlineData("unknown", FanMode.Auto)]
        public void ResolveBuiltInFanMode_MapsAliases(string name, FanMode expected)
        {
            var resolved = FanModeNameResolver.ResolveBuiltInFanMode(name);
            resolved.Should().Be(expected);
        }

        [Theory]
        [InlineData("Max", FanMode.Max, "Max")]
        [InlineData("Extreme", FanMode.Performance, "Extreme")]
        [InlineData("Gaming", FanMode.Performance, "Gaming")]
        [InlineData("Quiet", FanMode.Manual, "Silent")]
        [InlineData("Silent", FanMode.Manual, "Silent")]
        [InlineData("Auto", FanMode.Auto, "Auto")]
        [InlineData("My Curve", FanMode.Manual, "Custom")]
        [InlineData("Unknown", FanMode.Performance, "Auto")]
        public void ResolveCardMode_MapsPresetToUiCard(string presetName, FanMode mode, string expected)
        {
            var preset = new FanPreset
            {
                Name = presetName,
                Mode = mode
            };

            var cardMode = FanModeNameResolver.ResolveCardMode(preset);
            cardMode.Should().Be(expected);
        }

        [Theory]
        [InlineData("Turbo Max Profile", true)]
        [InlineData("My Quiet Curve", true)]
        [InlineData("Battery Saver", true)]
        [InlineData("Balanced_Default", true)]
        [InlineData("Office", false)]
        public void AliasMatchers_SupportTokenizedPresetNames(string presetName, bool shouldMatchAnyAlias)
        {
            var matched = FanModeNameResolver.IsMaxAlias(presetName)
                || FanModeNameResolver.IsQuietAlias(presetName)
                || FanModeNameResolver.IsAutoAlias(presetName)
                || FanModeNameResolver.IsPerformanceAlias(presetName);

            matched.Should().Be(shouldMatchAnyAlias);
        }

        [Theory]
        [InlineData("Turbo Max Profile", "Performance")]
        [InlineData("Silent Curve", "Quiet")]
        [InlineData("Balanced_Default", "Balanced")]
        [InlineData("Manual Custom", "Custom")]
        [InlineData("UnknownPreset", "Custom")]
        public void ResolveGeneralProfileFromPresetName_MapsExpectedProfile(string presetName, string expected)
        {
            FanModeNameResolver.ResolveGeneralProfileFromPresetName(presetName)
                .Should().Be(expected);
        }
    }
}
