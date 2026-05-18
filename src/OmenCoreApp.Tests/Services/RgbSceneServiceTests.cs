using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using OmenCore.Models;
using OmenCore.Services;
using OmenCore.Services.Rgb;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class RgbSceneServiceTests
    {
        [Fact]
        public async Task ApplySceneAsync_WhenRgbManagerHasNoSupportedProviders_DoesNotReportSuccess()
        {
            using var logging = new LoggingService();
            var manager = new RgbManager(logging);
            using var service = new RgbSceneService(logging, manager);
            var sceneChanged = false;
            service.SceneChanged += (_, _) => sceneChanged = true;

            var result = await service.ApplySceneAsync(new RgbScene
            {
                Name = "Static red",
                Effect = RgbSceneEffect.Static,
                PrimaryColor = "#FF0000",
                ApplyToOmenKeyboard = false,
                ApplyToCorsair = true,
                ApplyToLogitech = false,
                ApplyToRazer = false
            });

            result.Success.Should().BeFalse("a scene should not become the confirmed RGB state when no selected RGB provider accepted the write");
            result.ProvidersApplied.Should().Be(0);
            result.ProvidersFailed.Should().Be(1);
            sceneChanged.Should().BeFalse("failed scene applies should not be published as confirmed current RGB state");
            service.CurrentScene.Should().BeNull();
        }

        [Fact]
        public async Task SceneChanged_WhenSubscriberThrows_StillNotifiesRemainingSubscribers()
        {
            using var logging = new LoggingService();
            var manager = new RgbManager(logging);
            manager.RegisterProvider(new SceneRgbProvider());
            using var service = new RgbSceneService(logging, manager);
            RgbSceneChangedEventArgs? delivered = null;

            service.SceneChanged += (_, _) => throw new InvalidOperationException("subscriber failed");
            service.SceneChanged += (_, args) => delivered = args;

            var result = await service.ApplySceneAsync(new RgbScene
            {
                Name = "Static red",
                Effect = RgbSceneEffect.Static,
                PrimaryColor = "#FF0000",
                ApplyToOmenKeyboard = false,
                ApplyToCorsair = true,
                ApplyToLogitech = false,
                ApplyToRazer = false
            });

            result.Success.Should().BeTrue();
            delivered.Should().NotBeNull();
            delivered!.CurrentScene.Name.Should().Be("Static red");
        }

        private sealed class SceneRgbProvider : IRgbProvider
        {
            public string ProviderName => "Scene";
            public string ProviderId => "scene";
            public bool IsAvailable => true;
            public bool IsConnected => true;
            public int DeviceCount => 1;
            public RgbProviderConnectionStatus ConnectionStatus => RgbProviderConnectionStatus.Connected;
            public string StatusDetail => "test";
            public IReadOnlyList<RgbEffectType> SupportedEffects { get; } = new[] { RgbEffectType.Static };
            public Task InitializeAsync() => Task.CompletedTask;
            public Task ApplyEffectAsync(string effectId) => Task.CompletedTask;
            public Task SetStaticColorAsync(System.Drawing.Color color) => Task.CompletedTask;
            public Task SetBreathingEffectAsync(System.Drawing.Color color) => Task.CompletedTask;
            public Task SetSpectrumEffectAsync() => Task.CompletedTask;
            public Task TurnOffAsync() => Task.CompletedTask;
        }
    }
}
