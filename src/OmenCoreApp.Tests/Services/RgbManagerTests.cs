using System;
using System.Drawing;
using System.Threading.Tasks;
using FluentAssertions;
using OmenCore.Services.Rgb;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class RgbManagerTests
    {
        [Fact]
        public async Task SyncStaticColorAsync_ReportsProviderFailuresWithoutStoppingOtherProviders()
        {
            var manager = new RgbManager();
            var workingProvider = new TestRgbProvider("working");
            var failingProvider = new TestRgbProvider("failing")
            {
                ThrowOnStaticColor = true
            };
            RgbSyncEventArgs? completed = null;

            manager.RegisterProvider(workingProvider);
            manager.RegisterProvider(failingProvider);
            manager.SyncCompleted += (_, args) => completed = args;

            await manager.SyncStaticColorAsync(Color.FromArgb(0x12, 0x34, 0x56));

            workingProvider.LastEffect.Should().Be("static:#123456");
            completed.Should().NotBeNull();
            completed!.ProvidersAffected.Should().Be(2);
            completed.ProvidersSucceeded.Should().Be(1);
            completed.ProvidersFailed.Should().Be(1);
        }

        [Fact]
        public void GetStatus_IncludesProviderConnectionStatusAndDetail()
        {
            var manager = new RgbManager();
            manager.RegisterProvider(new TestRgbProvider("keyboard")
            {
                Detail = "4-zone keyboard connected"
            });

            var status = manager.GetStatus();

            status.ProviderStatuses.Should().ContainSingle(provider =>
                provider.ProviderId == "keyboard" &&
                provider.ConnectionStatus == RgbProviderConnectionStatus.Connected &&
                provider.StatusDetail == "4-zone keyboard connected");
        }

        [Fact]
        public async Task InitializeAllAsync_WhenCalledTwice_InitializesProvidersOnlyOnce()
        {
            var manager = new RgbManager();
            var provider = new TestRgbProvider("keyboard")
            {
                IsAvailable = false,
                AvailableAfterInitialize = true
            };

            manager.RegisterProvider(provider);

            await manager.InitializeAllAsync();
            await manager.InitializeAllAsync();

            provider.InitializeCount.Should().Be(1);
            provider.IsAvailable.Should().BeTrue();
        }

        [Fact]
        public async Task SyncStaticColorAsync_LazilyInitializesProviderBeforeFirstWrite()
        {
            var manager = new RgbManager();
            var provider = new TestRgbProvider("keyboard")
            {
                IsAvailable = false,
                AvailableAfterInitialize = true
            };

            manager.RegisterProvider(provider);

            await manager.SyncStaticColorAsync(Color.FromArgb(0xAA, 0xBB, 0xCC));

            provider.InitializeCount.Should().Be(1);
            provider.LastEffect.Should().Be("static:#AABBCC");
        }

        private sealed class TestRgbProvider : IRgbProvider
        {
            public TestRgbProvider(string id)
            {
                ProviderId = id;
                ProviderName = id;
            }

            public string ProviderName { get; }
            public string ProviderId { get; }
            public bool IsAvailable { get; set; } = true;
            public bool IsConnected => IsAvailable;
            public int DeviceCount => IsAvailable ? 1 : 0;
            public RgbProviderConnectionStatus ConnectionStatus =>
                IsAvailable ? RgbProviderConnectionStatus.Connected : RgbProviderConnectionStatus.Disabled;
            public string StatusDetail => Detail;
            public string Detail { get; set; } = "1 device connected";
            public bool ThrowOnStaticColor { get; set; }
            public bool AvailableAfterInitialize { get; set; } = true;
            public int InitializeCount { get; private set; }
            public string? LastEffect { get; private set; }
            public System.Collections.Generic.IReadOnlyList<RgbEffectType> SupportedEffects { get; } =
                new[] { RgbEffectType.Static, RgbEffectType.Breathing, RgbEffectType.Spectrum, RgbEffectType.Off };

            public Task InitializeAsync()
            {
                InitializeCount++;
                IsAvailable = AvailableAfterInitialize;
                return Task.CompletedTask;
            }

            public Task ApplyEffectAsync(string effectId)
            {
                LastEffect = effectId;
                return Task.CompletedTask;
            }

            public Task SetStaticColorAsync(Color color)
            {
                if (ThrowOnStaticColor)
                    throw new InvalidOperationException("device write failed");

                LastEffect = $"static:#{color.R:X2}{color.G:X2}{color.B:X2}";
                return Task.CompletedTask;
            }

            public Task SetBreathingEffectAsync(Color color)
            {
                LastEffect = $"breathing:#{color.R:X2}{color.G:X2}{color.B:X2}";
                return Task.CompletedTask;
            }

            public Task SetSpectrumEffectAsync()
            {
                LastEffect = "spectrum";
                return Task.CompletedTask;
            }

            public Task TurnOffAsync()
            {
                LastEffect = "off";
                return Task.CompletedTask;
            }
        }
    }
}
