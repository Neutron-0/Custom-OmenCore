using OmenCore.Models;
using OmenCore.Services;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    public class MonitoringTelemetryAdapterTests
    {
        [Fact]
        public void BuildTelemetry_ValidReading_ProjectsValueAsSupported()
        {
            var sample = new MonitoringSample
            {
                CpuTemperatureC = 71.5,
                CpuTemperatureState = TelemetryDataState.Valid,
                CpuPowerWatts = 42.3,
                CpuPowerState = TelemetryDataState.Valid
            };

            var temp = MonitoringTelemetryAdapter.BuildCpuTemperatureTelemetry(sample, "WMI", monitoringStale: false, lastError: null);
            var power = MonitoringTelemetryAdapter.BuildCpuPowerTelemetry(sample, "WMI", monitoringStale: false, lastError: null);

            Assert.True(temp.IsSupported);
            Assert.False(temp.IsStale);
            Assert.Equal(71.5, temp.Value);

            Assert.True(power.IsSupported);
            Assert.False(power.IsStale);
            Assert.Equal(42.3, power.Value);
        }

        [Fact]
        public void BuildTelemetry_MissingUnsupportedReading_ProjectsNullValue()
        {
            var sample = new MonitoringSample
            {
                CpuTemperatureC = 0,
                CpuTemperatureState = TelemetryDataState.Unavailable,
                CpuPowerWatts = 0,
                CpuPowerState = TelemetryDataState.Unavailable
            };

            var temp = MonitoringTelemetryAdapter.BuildCpuTemperatureTelemetry(sample, "WMI", monitoringStale: false, lastError: null);
            var power = MonitoringTelemetryAdapter.BuildCpuPowerTelemetry(sample, "WMI", monitoringStale: false, lastError: null);

            Assert.False(temp.IsSupported);
            Assert.Null(temp.Value);

            Assert.False(power.IsSupported);
            Assert.Null(power.Value);
        }

        [Fact]
        public void BuildTelemetry_StaleReading_ProjectsStaleFlag()
        {
            var sample = new MonitoringSample
            {
                CpuTemperatureC = 66,
                CpuTemperatureState = TelemetryDataState.Stale,
                CpuPowerWatts = 25,
                CpuPowerState = TelemetryDataState.Valid
            };

            var temp = MonitoringTelemetryAdapter.BuildCpuTemperatureTelemetry(sample, "LHM", monitoringStale: false, lastError: null);
            var power = MonitoringTelemetryAdapter.BuildCpuPowerTelemetry(sample, "LHM", monitoringStale: true, lastError: null);

            Assert.True(temp.IsSupported);
            Assert.True(temp.IsStale);
            Assert.Equal(66, temp.Value);

            Assert.True(power.IsSupported);
            Assert.True(power.IsStale);
            Assert.Equal(25, power.Value);
        }

        [Fact]
        public void BuildTelemetry_ProviderError_CapturesLastError()
        {
            var sample = new MonitoringSample
            {
                CpuTemperatureC = 63,
                CpuTemperatureState = TelemetryDataState.Valid,
                CpuPowerWatts = 18,
                CpuPowerState = TelemetryDataState.Valid
            };

            const string providerError = "ReadSampleAsync timeout after 10000ms";
            var temp = MonitoringTelemetryAdapter.BuildCpuTemperatureTelemetry(sample, "WMI", monitoringStale: false, lastError: providerError);
            var power = MonitoringTelemetryAdapter.BuildCpuPowerTelemetry(sample, "WMI", monitoringStale: false, lastError: providerError);

            Assert.Equal(providerError, temp.LastError);
            Assert.Equal(providerError, power.LastError);
        }
    }
}