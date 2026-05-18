using FluentAssertions;
using OmenCore.Hardware;
using OmenCore.Services;
using System.Reflection;
using Xunit;

namespace OmenCoreApp.Tests.Hardware
{
    public class NvapiServiceTests
    {
        [Fact]
        public void Initialize_DoesNotThrow_AndReflectsSupportsOverclocking()
        {
            var logging = new LoggingService(); logging.Initialize();
            var svc = new NvapiService(logging);
            // initialization should not throw regardless of environment
            var result = svc.Initialize();
            svc.SupportsOverclocking.Should().Be(result, "SupportsOverclocking flag matches result");
            // result may be true on machines with NVIDIA hardware, but should never crash
        }

        [Fact]
        public void ResolvePowerTopologyWatts_NormalizesRtx4060LaptopImplausibleReading()
        {
            var logging = new LoggingService();
            logging.Initialize();
            var svc = new NvapiService(logging);
            typeof(NvapiService)
                .GetProperty(nameof(NvapiService.GpuName))!
                .SetValue(svc, "NVIDIA GeForce RTX 4060 Laptop GPU");

            var resolver = typeof(NvapiService)
                .GetMethod("ResolvePowerTopologyWatts", BindingFlags.Instance | BindingFlags.NonPublic);
            resolver.Should().NotBeNull();

            var watts = (double)resolver!.Invoke(svc, new object[] { 220.0, 48.0 })!;

            watts.Should().Be(22.0);
        }
    }
}
