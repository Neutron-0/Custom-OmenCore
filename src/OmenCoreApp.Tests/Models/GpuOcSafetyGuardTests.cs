using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    public class GpuOcSafetyGuardTests
    {
        [Fact]
        public void IsIncreaseRequest_ReturnsFalse_WhenRequestedEqualsCurrent()
        {
            var result = GpuOcSafetyGuard.IsIncreaseRequest(
                requestedCore: 100,
                requestedMemory: 400,
                requestedPowerLimit: 110,
                requestedVoltage: 25,
                currentCore: 100,
                currentMemory: 400,
                currentPowerLimit: 110,
                currentVoltage: 25);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIncreaseRequest_ReturnsTrue_WhenCoreIncreases()
        {
            var result = GpuOcSafetyGuard.IsIncreaseRequest(
                requestedCore: 125,
                requestedMemory: 400,
                requestedPowerLimit: 110,
                requestedVoltage: 25,
                currentCore: 100,
                currentMemory: 400,
                currentPowerLimit: 110,
                currentVoltage: 25);

            result.Should().BeTrue();
        }

        [Fact]
        public void IsIncreaseRequest_ReturnsTrue_WhenMemoryIncreases()
        {
            var result = GpuOcSafetyGuard.IsIncreaseRequest(
                requestedCore: 100,
                requestedMemory: 450,
                requestedPowerLimit: 110,
                requestedVoltage: 25,
                currentCore: 100,
                currentMemory: 400,
                currentPowerLimit: 110,
                currentVoltage: 25);

            result.Should().BeTrue();
        }

        [Fact]
        public void IsIncreaseRequest_ReturnsTrue_WhenPowerLimitIncreases()
        {
            var result = GpuOcSafetyGuard.IsIncreaseRequest(
                requestedCore: 100,
                requestedMemory: 400,
                requestedPowerLimit: 115,
                requestedVoltage: 25,
                currentCore: 100,
                currentMemory: 400,
                currentPowerLimit: 110,
                currentVoltage: 25);

            result.Should().BeTrue();
        }

        [Fact]
        public void IsIncreaseRequest_ReturnsTrue_WhenVoltageIncreases()
        {
            var result = GpuOcSafetyGuard.IsIncreaseRequest(
                requestedCore: 100,
                requestedMemory: 400,
                requestedPowerLimit: 110,
                requestedVoltage: 35,
                currentCore: 100,
                currentMemory: 400,
                currentPowerLimit: 110,
                currentVoltage: 25);

            result.Should().BeTrue();
        }

        [Fact]
        public void IsIncreaseRequest_ReturnsFalse_WhenAllRequestedValuesDecrease()
        {
            var result = GpuOcSafetyGuard.IsIncreaseRequest(
                requestedCore: 75,
                requestedMemory: 300,
                requestedPowerLimit: 100,
                requestedVoltage: 0,
                currentCore: 100,
                currentMemory: 400,
                currentPowerLimit: 110,
                currentVoltage: 25);

            result.Should().BeFalse();
        }
    }
}
