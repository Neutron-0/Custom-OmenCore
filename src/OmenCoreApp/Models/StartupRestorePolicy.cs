namespace OmenCore.Models
{
    public enum StartupRestoreCategory
    {
        Fans,
        Performance,
        Rgb,
        Tuning
    }

    public static class StartupRestorePolicy
    {
        public static bool IsEnabled(AppConfig config, StartupRestoreCategory category)
        {
            if (!config.EnableStartupHardwareRestore)
            {
                return false;
            }

            return category switch
            {
                StartupRestoreCategory.Fans => config.StartupRestoreFansEnabled ?? true,
                StartupRestoreCategory.Performance => config.StartupRestorePerformanceEnabled ?? true,
                StartupRestoreCategory.Rgb => config.StartupRestoreRgbEnabled ?? true,
                StartupRestoreCategory.Tuning => config.StartupRestoreTuningEnabled ?? true,
                _ => true
            };
        }

        public static string BuildSummary(AppConfig config)
        {
            if (!config.EnableStartupHardwareRestore)
            {
                return "Disabled";
            }

            return $"Fans={Format(IsEnabled(config, StartupRestoreCategory.Fans))}; " +
                   $"Performance={Format(IsEnabled(config, StartupRestoreCategory.Performance))}; " +
                   $"RGB={Format(IsEnabled(config, StartupRestoreCategory.Rgb))}; " +
                   $"Tuning={Format(IsEnabled(config, StartupRestoreCategory.Tuning))}";
        }

        private static string Format(bool enabled) => enabled ? "on" : "off";
    }
}
