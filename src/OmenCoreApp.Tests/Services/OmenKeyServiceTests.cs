using System.Reflection;
using FluentAssertions;
using OmenCore.Services;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class OmenKeyServiceTests
    {
        private const uint VkF12 = 0x7B;
        private const uint DedicatedOmenLaunchScan = 0xE045;

        [Fact]
        public void FnF12_WithDedicatedOmenLaunchScan_IsNotMarkedNeverIntercept()
        {
            using var service = CreateService();

            var (neverIntercept, reason) = InvokeTryGetNeverInterceptReason(service, VkF12, DedicatedOmenLaunchScan);

            neverIntercept.Should().BeFalse();
            reason.Should().BeEmpty();
        }

        [Fact]
        public void FnF12_WithDedicatedOmenLaunchScan_IsAcceptedAsOmenKey()
        {
            using var service = CreateService();

            InvokeIsOmenKey(service, VkF12, DedicatedOmenLaunchScan).Should().BeTrue();
        }

        [Fact]
        public void PlainF12_RemainsNeverInterceptFunctionKey()
        {
            using var service = CreateService();

            var (neverIntercept, reason) = InvokeTryGetNeverInterceptReason(service, VkF12, 0x0058);

            neverIntercept.Should().BeTrue();
            reason.Should().Be("never-intercept-function-key");
            InvokeIsOmenKey(service, VkF12, 0x0058).Should().BeFalse();
        }

        private static OmenKeyService CreateService()
        {
            var logging = new LoggingService();
            logging.Initialize();
            return new OmenKeyService(logging);
        }

        private static (bool Result, string Reason) InvokeTryGetNeverInterceptReason(OmenKeyService service, uint vkCode, uint scanCode)
        {
            var method = typeof(OmenKeyService).GetMethod("TryGetNeverInterceptReason", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Should().NotBeNull();

            var args = new object?[] { vkCode, scanCode, null };
            var result = (bool)method!.Invoke(service, args)!;
            return (result, (string)args[2]!);
        }

        private static bool InvokeIsOmenKey(OmenKeyService service, uint vkCode, uint scanCode)
        {
            var method = typeof(OmenKeyService).GetMethod("IsOmenKey", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Should().NotBeNull();

            return (bool)method!.Invoke(service, new object[] { vkCode, scanCode })!;
        }
    }
}
