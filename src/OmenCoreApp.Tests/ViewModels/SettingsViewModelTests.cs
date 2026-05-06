using System;
using System.IO;
using FluentAssertions;
using OmenCore.Services;
using OmenCore.Services.Diagnostics;
using OmenCore.ViewModels;
using Xunit;

namespace OmenCoreApp.Tests.ViewModels
{
    [Collection("Config Isolation")]
    public class SettingsViewModelTests : IDisposable
    {
        private readonly string _tempDir;

        public SettingsViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "omen_test_config_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            Environment.SetEnvironmentVariable("OMENCORE_CONFIG_DIR", _tempDir);
        }

        public void Dispose()
        {
            try
            {
                Environment.SetEnvironmentVariable("OMENCORE_CONFIG_DIR", null);
                if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
            }
            catch { }
        }

        [Fact]
        public void CorsairDisableIcueFallback_Toggle_PersistsToConfig()
        {
            var logging = new OmenCore.Services.LoggingService();
            var cfgService = new ConfigurationService();

            // Sanity: default false
            cfgService.Config.CorsairDisableIcueFallback.Should().BeFalse();

            var sysInfo = new OmenCore.Services.SystemInfoService(logging);
            var fanCleaning = new OmenCore.Services.FanCleaningService(logging, null, sysInfo);
            var bios = new OmenCore.Services.BiosUpdateService(logging);
            var profileExport = new OmenCore.Services.ProfileExportService(logging, cfgService);
            var diagnosticsExport = new DiagnosticExportService(logging, System.IO.Path.GetTempPath());

            var vm = new SettingsViewModel(logging, cfgService, sysInfo, fanCleaning, bios, profileExport, diagnosticsExport)
            {

                // Enable the HID-only mode
                CorsairDisableIcueFallback = true
            };

            // ConfigurationService writes to disk on Save; reload from disk to verify persistence
            var cfgReload = new ConfigurationService();
            cfgReload.Config.CorsairDisableIcueFallback.Should().BeTrue();

            // Toggle back to false
            vm.CorsairDisableIcueFallback = false;
            var cfgReload2 = new ConfigurationService();
            cfgReload2.Config.CorsairDisableIcueFallback.Should().BeFalse();
        }

        [Fact]
        public void HotkeysWindowFocused_DefaultTrueAndPersists()
        {
            var logging = new OmenCore.Services.LoggingService();
            var cfgService = new ConfigurationService();
            
            // default should be true (window-focused behaviour enabled)
            cfgService.Config.Monitoring.WindowFocusedHotkeys.Should().BeTrue();
            
            var sysInfo = new OmenCore.Services.SystemInfoService(logging);
            var fanCleaning = new OmenCore.Services.FanCleaningService(logging, null, sysInfo);
            var bios = new OmenCore.Services.BiosUpdateService(logging);
            var profileExport = new OmenCore.Services.ProfileExportService(logging, cfgService);
            var diagnosticsExport = new DiagnosticExportService(logging, System.IO.Path.GetTempPath());

            var vm = new SettingsViewModel(logging, cfgService, sysInfo, fanCleaning, bios, profileExport, diagnosticsExport)
            {
                HotkeysWindowFocused = false
            };

            var cfgReload = new ConfigurationService();
            cfgReload.Config.Monitoring.WindowFocusedHotkeys.Should().BeFalse();

            // flip back to true and verify persistence again
            vm.HotkeysWindowFocused = true;
            var cfgReload2 = new ConfigurationService();
            cfgReload2.Config.Monitoring.WindowFocusedHotkeys.Should().BeTrue();
        }

        [Fact]
        public void LowOverheadMode_PersistsCanonicalLegacyMonitoringValues()
        {
            var logging = new OmenCore.Services.LoggingService();
            var cfgService = new ConfigurationService();
            var sysInfo = new OmenCore.Services.SystemInfoService(logging);
            var fanCleaning = new OmenCore.Services.FanCleaningService(logging, null, sysInfo);
            var bios = new OmenCore.Services.BiosUpdateService(logging);
            var profileExport = new OmenCore.Services.ProfileExportService(logging, cfgService);
            var diagnosticsExport = new DiagnosticExportService(logging, System.IO.Path.GetTempPath());

            var vm = new SettingsViewModel(logging, cfgService, sysInfo, fanCleaning, bios, profileExport, diagnosticsExport)
            {
                LowOverheadMode = true
            };

            var cfgReload = new ConfigurationService();
            cfgReload.Config.Monitoring.LowOverheadMode.Should().BeTrue();
            cfgReload.Config.Monitoring.PollingProfile.Should().Be("Low overhead");
            cfgReload.Config.Monitoring.PollIntervalMs.Should().Be(2000);
        }

        [Fact]
        public void DisablingLowOverheadMode_PersistsBalancedCompatibilityValues()
        {
            var logging = new OmenCore.Services.LoggingService();
            var cfgService = new ConfigurationService();
            cfgService.Config.Monitoring.LowOverheadMode = true;
            cfgService.Config.Monitoring.PollingProfile = "Performance";
            cfgService.Config.Monitoring.PollIntervalMs = 500;
            cfgService.Save(cfgService.Config);

            var sysInfo = new OmenCore.Services.SystemInfoService(logging);
            var fanCleaning = new OmenCore.Services.FanCleaningService(logging, null, sysInfo);
            var bios = new OmenCore.Services.BiosUpdateService(logging);
            var profileExport = new OmenCore.Services.ProfileExportService(logging, cfgService);
            var diagnosticsExport = new DiagnosticExportService(logging, System.IO.Path.GetTempPath());

            var vm = new SettingsViewModel(logging, cfgService, sysInfo, fanCleaning, bios, profileExport, diagnosticsExport)
            {
                LowOverheadMode = false
            };

            var cfgReload = new ConfigurationService();
            cfgReload.Config.Monitoring.LowOverheadMode.Should().BeFalse();
            cfgReload.Config.Monitoring.PollingProfile.Should().Be("Balanced");
            cfgReload.Config.Monitoring.PollIntervalMs.Should().Be(1000);
        }
    }
}
