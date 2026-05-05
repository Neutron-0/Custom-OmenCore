using FluentAssertions;
using OmenCore.Hardware;
using Xunit;

namespace OmenCoreApp.Tests.Hardware
{
    /// <summary>
    /// Validates the WinRing0 EC access allowlist via the public static property.
    /// Tests deliberately avoid instantiating WinRing0EcAccess to prevent the class
    /// static initializer from creating the Global\Access_EC mutex and loading
    /// WinRing0 IOCTL codes into the process — patterns that trigger Defender heuristics
    /// during test runs on developer machines.
    /// </summary>
    public class WinRing0EcAccessTests
    {
        [Theory]
        [InlineData((ushort)0x44)] // Fan 1 duty cycle
        [InlineData((ushort)0x45)] // Fan 2 duty cycle
        [InlineData((ushort)0x46)] // Fan control mode
        [InlineData((ushort)0xCE)] // Performance mode register
        public void AllowedWriteAddresses_ContainsFanControlRegisters(ushort address)
        {
            WinRing0EcAccess.AllowedWriteAddresses.Should().Contain(address,
                $"0x{address:X2} is a fan-control register and must be in the safety allowlist");
        }

        [Theory]
        [InlineData((ushort)0xFF)] // Battery charger (dangerous)
        [InlineData((ushort)0x12)] // VRM control (dangerous)
        [InlineData((ushort)0x00)] // System control register (dangerous)
        [InlineData((ushort)0x99)] // Arbitrary unknown address
        public void AllowedWriteAddresses_ExcludesDangerousRegisters(ushort address)
        {
            WinRing0EcAccess.AllowedWriteAddresses.Should().NotContain(address,
                $"0x{address:X2} is a dangerous register that must never be in the allowlist");
        }

        [Fact]
        public void AllowedWriteAddresses_IsNonEmpty()
        {
            WinRing0EcAccess.AllowedWriteAddresses.Should().NotBeEmpty(
                "at least the core fan duty-cycle registers must be present");
        }
    }
}
