using FluentAssertions;
using OmenCore.Hardware;
using OmenCore.Models;
using OmenCore.Services.Diagnostics;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class ModelIdentityResolutionSummaryTests
    {
        [Fact]
        public void Build_SeparatesBoardProductIdFromHpSupportProductNumber()
        {
            var systemInfo = new SystemInfo
            {
                Manufacturer = "HP",
                Model = "OMEN Gaming Laptop 16-n0xxx",
                ProductName = "8A43",
                SystemSku = "6G103EA#ABU",
                BiosVersion = "F.17"
            };
            var capabilities = new DeviceCapabilities
            {
                ProductId = "8A43",
                ModelName = systemInfo.Model,
                ModelFamily = OmenModelFamily.OMEN16,
                IsKnownModel = true,
                ModelConfig = ModelCapabilityDatabase.GetCapabilities("8A43")
            };

            var summary = ModelIdentityResolutionService.Build(systemInfo, capabilities);

            summary.RawBaseboardProduct.Should().Be("8A43");
            summary.RawSystemSku.Should().Be("6G103EA#ABU");
            summary.HpSupportProductNumber.Should().Be("6G103EA");
            summary.RawIdentitySummary.Should().Contain("Baseboard ProductId: 8A43");
            summary.RawIdentitySummary.Should().Contain("HP support product: 6G103EA");
            summary.ClipboardSummary.Should().Contain("HP support product number: 6G103EA");
            summary.TraceText.Should().Contain("Baseboard ProductId drives OmenCore capability lookup");
        }

        [Fact]
        public void Build_8D2FExactProductId_IsHighConfidenceWithoutVerificationWarnings()
        {
            var systemInfo = new SystemInfo
            {
                Manufacturer = "HP",
                Model = "OMEN Gaming Laptop 16-am0xxx",
                ProductName = "8D2F",
                SystemSku = "",
                BiosVersion = "F.01"
            };
            var capabilities = new DeviceCapabilities
            {
                ProductId = "8D2F",
                ModelName = systemInfo.Model,
                ModelFamily = OmenModelFamily.OMEN16,
                IsKnownModel = true,
                ModelConfig = ModelCapabilityDatabase.GetCapabilities("8D2F")
            };

            var summary = ModelIdentityResolutionService.Build(systemInfo, capabilities);

            summary.ResolutionSource.Should().Be("Exact ProductId");
            summary.Confidence.Should().Be("High");
            summary.WarningText.Should().BeEmpty();
            summary.KeyboardResolutionSource.Should().Be("Exact ProductId");
            summary.KeyboardConfidence.Should().Be("High");
            summary.KeyboardWarningText.Should().BeEmpty();
            summary.ClipboardSummary.Should().NotContain("Capability warning:");
            summary.ClipboardSummary.Should().NotContain("Keyboard warning:");
        }

        [Fact]
        public void Build_8D41ExactProductId_ResolvesKeyboardProfile()
        {
            var systemInfo = new SystemInfo
            {
                Manufacturer = "HP",
                Model = "OMEN MAX Gaming Laptop 16t-ah000",
                ProductName = "8D41",
                SystemSku = "1H9533H07X",
                BiosVersion = "F.01"
            };
            var capabilities = new DeviceCapabilities
            {
                ProductId = "8D41",
                ModelName = systemInfo.Model,
                ModelFamily = OmenModelFamily.OMEN2024Plus,
                IsKnownModel = true,
                ModelConfig = ModelCapabilityDatabase.GetCapabilities("8D41")
            };

            var summary = ModelIdentityResolutionService.Build(systemInfo, capabilities);

            summary.ResolutionSource.Should().Be("Exact ProductId");
            summary.Confidence.Should().Be("High");
            summary.KeyboardResolutionSource.Should().Be("Exact ProductId");
            summary.KeyboardModel.Should().Contain("OMEN MAX 16");
            summary.KeyboardConfidence.Should().Be("Medium");
            summary.KeyboardWarningText.Should().Contain("not user-verified");
            summary.ClipboardSummary.Should().NotContain("Keyboard model: Unknown");
        }
    }
}
