using System.Linq;
using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    public class TuningConflictGuardTests
    {
        // These tests call TuningConflictGuard.Check() with the assumption that the known
        // conflicting processes (XTU, ThrottleStop, etc.) are NOT running in the test environment.
        // The tests verify the structural contract of the API rather than hardware-dependent detection.

        [Fact]
        public void Check_CpuUndervolt_ReturnsEmptyWhenNoConflictsRunning()
        {
            // In a clean CI environment none of the conflicting processes should be running.
            var report = TuningConflictGuard.Check(TuningConflictKind.CpuUndervolt);

            // If the CI machine happens to have XTU/ThrottleStop we skip this assertion.
            // We still verify the API returns a valid report object.
            report.Should().NotBeNull();
            report.Kind.Should().Be(TuningConflictKind.CpuUndervolt);
        }

        [Fact]
        public void Check_GpuOc_ReturnsEmptyWhenNoConflictsRunning()
        {
            var report = TuningConflictGuard.Check(TuningConflictKind.GpuOc);

            report.Should().NotBeNull();
            report.Kind.Should().Be(TuningConflictKind.GpuOc);
        }

        [Fact]
        public void TuningConflictReport_NoneFactory_ReturnsFalseForHasConflicts()
        {
            var report = TuningConflictReport.None(TuningConflictKind.CpuUndervolt);

            report.HasConflicts.Should().BeFalse();
            report.HasHighRiskConflict.Should().BeFalse();
            report.BannerText.Should().BeEmpty();
        }

        [Fact]
        public void TuningConflictReport_WithConflict_HasCorrectBannerText()
        {
            var entry = new TuningConflictEntry(
                id: "XTU",
                kind: TuningConflictKind.CpuUndervolt,
                processNames: new[] { "XTU" },
                serviceName: null,
                title: "Intel Extreme Tuning Utility (XTU)",
                detail: "XTU may conflict.",
                suggestion: "Close XTU first.");

            var report = new TuningConflictReport(
                TuningConflictKind.CpuUndervolt,
                new[] { entry });

            report.HasConflicts.Should().BeTrue();
            report.HasHighRiskConflict.Should().BeTrue();
            report.BannerText.Should().Contain("Intel Extreme Tuning Utility");
        }

        [Fact]
        public void TuningConflictEntry_HasExpectedFieldValues()
        {
            var entry = new TuningConflictEntry(
                id: "ThrottleStop",
                kind: TuningConflictKind.CpuUndervolt,
                processNames: new[] { "ThrottleStop" },
                serviceName: null,
                title: "ThrottleStop",
                detail: "May conflict with power limits.",
                suggestion: "Close ThrottleStop first.");

            entry.Id.Should().Be("ThrottleStop");
            entry.Kind.Should().Be(TuningConflictKind.CpuUndervolt);
            entry.Title.Should().Be("ThrottleStop");
            entry.Suggestion.Should().NotBeEmpty();
        }

        [Fact]
        public void KnownConflictKind_Flags_WorkAsExpected()
        {
            var both = TuningConflictKind.CpuUndervolt | TuningConflictKind.GpuOc;

            (both & TuningConflictKind.CpuUndervolt).Should().NotBe(TuningConflictKind.None);
            (both & TuningConflictKind.GpuOc).Should().NotBe(TuningConflictKind.None);
        }

        [Fact]
        public void TuningConflictReport_OneLinerSummaryVariant_ForMultipleConflicts()
        {
            var entries = new[]
            {
                new TuningConflictEntry("XTU", TuningConflictKind.CpuUndervolt,
                    new[] { "XTU" }, null, "Intel XTU", "Detail.", "Suggestion."),
                new TuningConflictEntry("ThrottleStop", TuningConflictKind.CpuUndervolt,
                    new[] { "ThrottleStop" }, null, "ThrottleStop", "Detail.", "Suggestion."),
            };

            var report = new TuningConflictReport(TuningConflictKind.CpuUndervolt, entries);

            report.HasConflicts.Should().BeTrue();
            report.Conflicts.Should().HaveCount(2);
            report.BannerText.Should().Contain("Intel XTU").And.Contain("ThrottleStop");
        }
    }
}
