using FluentAssertions;
using OmenCore.Services;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class FanCalibrationStorageServiceTests
    {
        [Theory]
        [InlineData("HP OMEN 16-n0xxx", "hp_omen_16_n0xxx")]
        [InlineData("OMEN by HP Gaming Laptop 16-n0xxx", "omen_by_hp_gaming_laptop_16_n0xxx")]
        [InlineData("HP.OMEN(16)", "hpomen16")]
        [InlineData("", "")]
        public void NormalizeModelId_ProducesStableStorageKey(string input, string expected)
        {
            FanCalibrationStorageService.NormalizeModelId(input)
                .Should().Be(expected);
        }
    }
}
