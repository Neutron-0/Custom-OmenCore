using System;
using System.IO;
using FluentAssertions;
using OmenCore.ViewModels;
using Xunit;

namespace OmenCoreApp.Tests.ViewModels
{
    public class SystemOptimizerReportActionGuardTests
    {
        [Fact]
        public void CanCopyPath_ReturnsFalse_ForNullOrWhitespace()
        {
            SystemOptimizerReportActionGuard.CanCopyPath(null).Should().BeFalse();
            SystemOptimizerReportActionGuard.CanCopyPath(string.Empty).Should().BeFalse();
            SystemOptimizerReportActionGuard.CanCopyPath("   ").Should().BeFalse();
        }

        [Fact]
        public void CanCopyPath_ReturnsTrue_ForNonEmptyPath()
        {
            SystemOptimizerReportActionGuard.CanCopyPath(@"C:\reports\optimizer-report.txt").Should().BeTrue();
        }

        [Fact]
        public void CanOpenReport_ReturnsFalse_WhenMissing()
        {
            var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.txt");
            SystemOptimizerReportActionGuard.CanOpenReport(path).Should().BeFalse();
        }

        [Fact]
        public void CanOpenReport_ReturnsTrue_WhenFileExists()
        {
            var path = Path.Combine(Path.GetTempPath(), $"report-{Guid.NewGuid():N}.txt");
            File.WriteAllText(path, "ok");
            try
            {
                SystemOptimizerReportActionGuard.CanOpenReport(path).Should().BeTrue();
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void NormalizePath_HandlesNullAndWhitespace()
        {
            SystemOptimizerReportActionGuard.NormalizePath(null).Should().BeEmpty();
            SystemOptimizerReportActionGuard.NormalizePath("  C:\\a\\b.txt  ").Should().Be(@"C:\a\b.txt");
        }
    }
}
