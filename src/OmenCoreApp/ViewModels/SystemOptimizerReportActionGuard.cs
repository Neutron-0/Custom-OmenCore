using System;
using System.IO;

namespace OmenCore.ViewModels
{
    /// <summary>
    /// Small testable guard for optimizer report actions.
    /// Keeps command-can-execute logic deterministic and unit-testable.
    /// </summary>
    public static class SystemOptimizerReportActionGuard
    {
        public static bool CanCopyPath(string? reportPath)
        {
            return !string.IsNullOrWhiteSpace(reportPath);
        }

        public static bool CanOpenReport(string? reportPath)
        {
            if (string.IsNullOrWhiteSpace(reportPath))
            {
                return false;
            }

            return File.Exists(reportPath);
        }

        public static string NormalizePath(string? reportPath)
        {
            return reportPath?.Trim() ?? string.Empty;
        }
    }
}
