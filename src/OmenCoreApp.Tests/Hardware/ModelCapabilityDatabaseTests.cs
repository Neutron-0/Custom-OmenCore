using FluentAssertions;
using OmenCore.Hardware;
using Xunit;

namespace OmenCoreApp.Tests.Hardware
{
    public class ModelCapabilityDatabaseTests
    {
        [Fact]
        public void GetCapabilitiesByModelName_Returns_OmenMaxAk0003nr()
        {
            var caps = ModelCapabilityDatabase.GetCapabilitiesByModelName("OMEN MAX 16 ak0003nr");
            caps.Should().NotBeNull();
            caps!.ModelName.Should().Contain("OMEN MAX 16");
            caps.ModelName.Should().Contain("ak0003nr");
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.FanZoneCount.Should().Be(2);
            caps.HasPerKeyRgb.Should().BeTrue();
        }

        [Theory]
        [InlineData("8A43", OmenModelFamily.OMEN16)]
        [InlineData("8A44", OmenModelFamily.OMEN16)]
        [InlineData("8C76", OmenModelFamily.OMEN16)]
        [InlineData("8A3E", OmenModelFamily.Victus)]
        [InlineData("8C30", OmenModelFamily.Victus)]
        [InlineData("8A26", OmenModelFamily.Victus)]
        [InlineData("8C58", OmenModelFamily.Transcend)]
        [InlineData("8E41", OmenModelFamily.Transcend)]
        [InlineData("8D87", OmenModelFamily.OMEN2024Plus)]
        [InlineData("8574", OmenModelFamily.Legacy)]
        [InlineData("8787", OmenModelFamily.Legacy)]
        [InlineData("88D2", OmenModelFamily.Legacy)]
        public void GetCapabilities_Returns_NewlyAdded_ModelEntries(string productId, OmenModelFamily expectedFamily)
        {
            var caps = ModelCapabilityDatabase.GetCapabilities(productId);

            caps.Should().NotBeNull();
            caps.ProductId.Should().Be(productId);
            caps.Family.Should().Be(expectedFamily);
            caps.UserVerified.Should().BeFalse();
        }

        [Fact]
        public void GetCapabilities_Transcend14Entries_DisableDirectEcAndCurves()
        {
            var caps8c58 = ModelCapabilityDatabase.GetCapabilities("8C58");
            var caps8e41 = ModelCapabilityDatabase.GetCapabilities("8E41");

            caps8c58.SupportsFanControlEc.Should().BeFalse();
            caps8c58.SupportsFanCurves.Should().BeFalse();
            caps8e41.SupportsFanControlEc.Should().BeFalse();
            caps8e41.SupportsFanCurves.Should().BeFalse();
        }

        [Fact]
        public void GetCapabilities_8D87_OmenMaxAk0xxx_UsesMaxSeriesSafetyProfile()
        {
            var caps = ModelCapabilityDatabase.GetCapabilities("8D87");

            caps.ProductId.Should().Be("8D87");
            caps.ModelName.Should().Contain("OMEN MAX 16");
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.HasPerKeyRgb.Should().BeTrue();
            caps.UserVerified.Should().BeFalse();
        }

        [Fact]
        public void GetCapabilities_8D41_OmenMaxAh0xxx_UsesWmiPolicyFallback()
        {
            var caps = ModelCapabilityDatabase.GetCapabilities("8D41");

            caps.ProductId.Should().Be("8D41");
            caps.ModelName.Should().Contain("ah0xxx");
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsFanCurves.Should().BeFalse();
            caps.AllowDecoupledWmiThermalPolicyFallback.Should().BeTrue(
                "8D41 cannot safely use legacy EC power/fan writes, so Quick Profiles need the OEM WMI thermal-policy path when direct limits are unavailable");
            caps.HasPerKeyRgb.Should().BeTrue();
            caps.UserVerified.Should().BeTrue();
        }

        [Fact]
        public void GetCapabilities_8787_Omen15En0038ur_UsesReportedSafeCapabilities()
        {
            var caps = ModelCapabilityDatabase.GetCapabilities("8787");

            caps.ProductId.Should().Be("8787");
            caps.ModelName.Should().Contain("15-en0038ur");
            caps.HasFourZoneRgb.Should().BeTrue();
            caps.HasMuxSwitch.Should().BeTrue();
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsRpmReadback.Should().BeFalse("GitHub #120 reports accepted fan commands but 0 RPM readback");
            caps.MaxFanLevel.Should().Be(55);
        }

        [Fact]
        public void GetCapabilities_8574_Omen15Dc1xxx_UsesConservativeEcFirstProfile()
        {
            var caps = ModelCapabilityDatabase.GetCapabilities("8574");

            caps.ProductId.Should().Be("8574");
            caps.ModelName.Should().Contain("15-dc1");
            caps.Family.Should().Be(OmenModelFamily.Legacy);
            caps.SupportsFanControlWmi.Should().BeFalse();
            caps.SupportsFanControlEc.Should().BeTrue();
            caps.SupportsFanCurves.Should().BeTrue();
            caps.HasKeyboardBacklight.Should().BeTrue();
            caps.HasFourZoneRgb.Should().BeFalse("RGB capability is held back until this board's protocol is verified");
            caps.SupportsTccOffset.Should().BeFalse();
            caps.SupportsPowerLimits.Should().BeFalse();
            caps.UserVerified.Should().BeFalse();
        }

        [Fact]
        public void GetPreferredCapabilities_88D2_Omen15zEn100_UsesConservativeLegacyProfile()
        {
            var caps = ModelCapabilityDatabase.GetPreferredCapabilities("88D2", "OMEN by HP Laptop 15z-en100");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("88D2");
            caps.ModelName.Should().Contain("15z-en100");
            caps.Family.Should().Be(OmenModelFamily.Legacy);
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsIndependentFanCurves.Should().BeFalse();
            caps.MaxFanLevel.Should().Be(55);
            caps.SupportsGpuPowerBoost.Should().BeFalse();
            caps.SupportsUndervolt.Should().BeFalse();
            caps.UserVerified.Should().BeFalse();
        }

        [Theory]
        [InlineData("DESKTOP-25L")]
        [InlineData("DESKTOP-30L")]
        [InlineData("DESKTOP-35L")]
        [InlineData("DESKTOP-40L")]
        [InlineData("DESKTOP-45L")]
        public void GetCapabilities_DesktopProfiles_DisableFanWritesForSafetyGate(string productId)
        {
            var caps = ModelCapabilityDatabase.GetCapabilities(productId);

            caps.Family.Should().Be(OmenModelFamily.Desktop);
            caps.SupportsFanControlWmi.Should().BeFalse();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsFanCurves.Should().BeFalse();
            caps.SupportsRpmReadback.Should().BeTrue();
            caps.SupportsPerformanceModes.Should().BeTrue();
            caps.Notes.Should().Contain("v3.6.3 safety gate");
        }

        [Fact]
        public void GetCapabilities_8C76_Wf1015ns_UsesExactV1WmiProfile()
        {
            var caps = ModelCapabilityDatabase.GetCapabilities("8C76");

            caps.ProductId.Should().Be("8C76");
            caps.ModelName.Should().Contain("wf1xxx");
            caps.Family.Should().Be(OmenModelFamily.OMEN16);
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsFanCurves.Should().BeTrue();
            caps.FanZoneCount.Should().Be(2);
            caps.MaxFanLevel.Should().Be(55);
            caps.HasMuxSwitch.Should().BeTrue();
            caps.SupportsGpuPowerBoost.Should().BeTrue();
            caps.HasFourZoneRgb.Should().BeTrue();
            caps.UserVerified.Should().BeFalse();
        }

        [Fact]
        public void GetCapabilities_8E35_Ap0xxxAmd_UsesExactV1WmiProfile()
        {
            var caps = ModelCapabilityDatabase.GetCapabilities("8E35");

            caps.ProductId.Should().Be("8E35");
            caps.ModelName.Should().Contain("ap0xxx");
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsFanCurves.Should().BeTrue();
            caps.FanZoneCount.Should().Be(2);
            caps.MaxFanLevel.Should().Be(55);
            caps.SupportsUndervolt.Should().BeFalse();
        }

        [Fact]
        public void GetCapabilities_8BD4_Victus16S0xxx_UsesExactConservativeProfile()
        {
            var caps = ModelCapabilityDatabase.GetPreferredCapabilities("8BD4", "Victus by HP Gaming Laptop 16-s0xxx");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("8BD4");
            caps.ModelName.Should().Contain("16-s0");
            caps.Family.Should().Be(OmenModelFamily.Victus);
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanCurves.Should().BeTrue();
            caps.FanZoneCount.Should().Be(2);
            caps.HasFourZoneRgb.Should().BeFalse();
            caps.SupportsGpuPowerBoost.Should().BeFalse();
            caps.SupportsUndervolt.Should().BeFalse();
        }

        [Fact]
        public void GetCapabilitiesByModelName_16Am0IntelFallback_DisablesDirectEc()
        {
            var caps = ModelCapabilityDatabase.GetCapabilitiesByModelName("OMEN Gaming Laptop 16-am0xxx");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("am0xxx_intel_2025_unverified");
            caps.Notes.Should().Contain("GitHub #124");
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsUndervolt.Should().BeFalse();
            caps.AllowDecoupledWmiThermalPolicyFallback.Should().BeTrue();
        }

        [Fact]
        public void GetCapabilitiesByModelName_Victus15Fb1_PrefersExact8C30Profile()
        {
            var caps = ModelCapabilityDatabase.GetCapabilitiesByModelName("Victus by HP Gaming Laptop 15-fb1xxx");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("8C30");
            caps.Family.Should().Be(OmenModelFamily.Victus);
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsFanCurves.Should().BeTrue();
            caps.AllowDecoupledWmiThermalPolicyFallback.Should().BeTrue();
            caps.Notes.Should().Contain("GitHub #135 diagnostics");
        }

        [Fact]
        public void GetPreferredCapabilities_8C30_Victus15Fb1_UsesExactProductProfile()
        {
            var caps = ModelCapabilityDatabase.GetPreferredCapabilities("8C30", "Victus by HP Gaming Laptop 15-fb1xxx");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("8C30");
            caps.Family.Should().Be(OmenModelFamily.Victus);
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsFanCurves.Should().BeTrue();
            caps.AllowDecoupledWmiThermalPolicyFallback.Should().BeTrue();
            caps.Notes.Should().Contain("GitHub #135 diagnostics");
        }

        [Fact]
        public void GetPreferredCapabilities_8D2F_UsesConfirmedConservativeAm0Profile()
        {
            var caps = ModelCapabilityDatabase.GetPreferredCapabilities("8D2F", "OMEN Gaming Laptop 16-am0xxx");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("8D2F");
            caps.ModelName.Should().Contain("16-am0");
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse();
            caps.SupportsIndependentFanCurves.Should().BeFalse();
            caps.SupportsUndervolt.Should().BeFalse();
            caps.AllowDecoupledWmiThermalPolicyFallback.Should().BeTrue();
            caps.UserVerified.Should().BeTrue("the exact 8D2F board identity has field confirmation, even though risky direct EC features stay disabled");
        }

        [Fact]
        public void GetPreferredCapabilities_8A43_WithN0xxModel_PrefersExactProductIdOverSiblingPattern()
        {
            var caps = ModelCapabilityDatabase.GetPreferredCapabilities("8A43", "OMEN Gaming Laptop 16-n0xxx");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("8A43");
            caps.MaxFanLevel.Should().Be(60, "8A43 diagnostics show practical fan-level ceiling near 60 (GPU ~60, CPU ~58)");
            caps.Notes.Should().Contain("16-n0002ni");
            caps.Notes.Should().Contain("6G103EA");
        }

        [Fact]
        public void GetPreferredCapabilities_Ambiguous8Bb1_UsesModelNameDisambiguation()
        {
            var caps = ModelCapabilityDatabase.GetPreferredCapabilities("8BB1", "Victus by HP Gaming Laptop 15-fa1xxx");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("8BB1-VICTUS15");
            caps.Family.Should().Be(OmenModelFamily.Victus);
            ModelCapabilityDatabase.IsAmbiguousProductId("8BB1").Should().BeTrue();
        }

        /// <summary>
        /// Issue #128: ProductId 88EC must resolve to explicit Victus e0xxx mapping,
        /// not a broad family fallback. This ensures consistent identity on field systems.
        /// </summary>
        [Fact]
        public void GetCapabilities_88EC_ResolvesToExplicitVictusE0xxxMapping()
        {
            var caps = ModelCapabilityDatabase.GetCapabilities("88EC");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("88EC", "explicit 88EC entry must exist");
            caps.Family.Should().Be(OmenModelFamily.Victus);
            caps.ModelName.Should().Contain("Victus 16");
            caps.ModelNamePattern.Should().Be("16-e0");
            caps.UserVerified.Should().BeFalse("pending field verification");
            caps.Notes.Should().Contain("Issue #128");
        }

        /// <summary>
        /// Issue #128: Victus 88EC capability flags are conservative pending field verification.
        /// No speculation about features without hardware evidence.
        /// </summary>
        [Fact]
        public void GetCapabilities_88EC_UsesConservativeFeatureFlags()
        {
            var caps = ModelCapabilityDatabase.GetCapabilities("88EC");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("88EC");
            caps.SupportsFanControlWmi.Should().BeTrue("WMI fan control is expected on Victus");
            caps.SupportsFanCurves.Should().BeTrue("curve support expected");
            caps.HasFourZoneRgb.Should().BeFalse("no RGB proof yet");
            caps.SupportsGpuPowerBoost.Should().BeFalse("no power boost proof yet");
            caps.SupportsUndervolt.Should().BeFalse("no undervolt proof yet");
            caps.HasKeyboardBacklight.Should().BeTrue("keyboard backlight expected");
        }

        [Fact]
        public void GetCapabilities_8BCD_UsesConservativeWmiV1FanProfile()
        {
            var caps = ModelCapabilityDatabase.GetPreferredCapabilities("8BCD", "OMEN by HP Gaming Laptop 16-xd0xxx");

            caps.Should().NotBeNull();
            caps!.ProductId.Should().Be("8BCD");
            caps.SupportsFanControlWmi.Should().BeTrue();
            caps.SupportsFanControlEc.Should().BeFalse("8BCD field evidence points at V1 WMI fan control, not validated direct EC fan writes");
            caps.SupportsIndependentFanCurves.Should().BeFalse("independent curve UI requires validated independent fan ownership");
            caps.MaxFanLevel.Should().Be(63, "latest field evidence shows this board reaches the 63-level ceiling (~6300 RPM)");
            caps.UserVerified.Should().BeFalse("2026-05-20 Discord report still needs physical follow-up after the 3.7.0 fixes");
            caps.Notes.Should().Contain("2026-05-29");
        }
    }
}
