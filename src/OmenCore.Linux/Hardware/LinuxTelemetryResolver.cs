namespace OmenCore.Linux.Hardware;

public sealed class LinuxTemperatureReading
{
    public int Temperature { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
}

public static class LinuxTelemetryResolver
{
    private const string CpuEcPath = "ec:0x57";
    private const string GpuEcPath = "ec:0xB7";
    private const int MinPlausibleTemperatureC = 1;
    private const int MaxPlausibleTemperatureC = 125;

    public static LinuxTemperatureReading? GetCpuTemperature(LinuxEcController ec, LinuxHwMonController hwmon)
    {
        return FilterPlausible(hwmon.GetCpuTemperatureReading()) ?? CreateEcReading(ec.GetCpuTemperature(), CpuEcPath);
    }

    public static LinuxTemperatureReading? GetGpuTemperature(LinuxEcController ec, LinuxHwMonController hwmon)
    {
        return FilterPlausible(hwmon.GetGpuTemperatureReading()) ?? CreateEcReading(ec.GetGpuTemperature(), GpuEcPath);
    }

    private static LinuxTemperatureReading? CreateEcReading(int? temperature, string path)
    {
        if (!temperature.HasValue || !IsPlausibleTemperature(temperature.Value))
        {
            return null;
        }

        return new LinuxTemperatureReading
        {
            Temperature = temperature.Value,
            Source = "ec",
            Path = path
        };
    }

    private static LinuxTemperatureReading? FilterPlausible(LinuxTemperatureReading? reading)
    {
        return reading != null && IsPlausibleTemperature(reading.Temperature)
            ? reading
            : null;
    }

    private static bool IsPlausibleTemperature(int temperature)
    {
        return temperature >= MinPlausibleTemperatureC && temperature <= MaxPlausibleTemperatureC;
    }
}
