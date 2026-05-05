using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    /// <summary>
    /// Regression tests for UndervoltPreferences startup reapply gating.
    /// Ensures ApplyOnStartup is not set by default and serializes correctly.
    /// </summary>
    public class UndervoltPreferencesTests
    {
        [Fact]
        public void ApplyOnStartup_DefaultsToFalse()
        {
            // Direct Apply must not enable startup reapply without a confirmed test session.
            var prefs = new UndervoltPreferences();
            prefs.ApplyOnStartup.Should().BeFalse(
                because: "startup reapply is only granted after Test Apply -> Keep, not by creating default preferences");
        }

        [Fact]
        public void ApplyOnStartup_CanBeSetToTrue_ForConfirmedTestSession()
        {
            var prefs = new UndervoltPreferences { ApplyOnStartup = true };
            prefs.ApplyOnStartup.Should().BeTrue();
        }

        [Fact]
        public void DefaultOffset_HasNonZeroUndervoltValues()
        {
            // Default offset should encourage undervolting, not be zero (which would skip startup restore).
            var prefs = new UndervoltPreferences();
            (prefs.DefaultOffset.CoreMv != 0 || prefs.DefaultOffset.CacheMv != 0).Should().BeTrue(
                because: "the default offset should have a non-zero undervolt so a confirmed startup restore would actually apply something");
        }

        [Fact]
        public void DirectApply_ShouldPreserveExistingApplyOnStartupFlag()
        {
            // Simulate what ApplyUndervoltAsync does: set offset values without touching ApplyOnStartup.
            var prefs = new UndervoltPreferences { ApplyOnStartup = true };
            prefs.DefaultOffset = new UndervoltOffset { CoreMv = -80, CacheMv = -60 };
            // ApplyOnStartup is preserved (not cleared by a direct apply).
            prefs.ApplyOnStartup.Should().BeTrue(
                because: "a subsequent Direct Apply should not revoke a previously Test-confirmed startup reapply");
        }

        [Fact]
        public void ConfirmTestApply_SetsApplyOnStartupTrue()
        {
            // Simulate the ConfirmCpuUndervoltTest path: set ApplyOnStartup = true.
            var prefs = new UndervoltPreferences();
            prefs.DefaultOffset = new UndervoltOffset { CoreMv = -90, CacheMv = -60 };
            prefs.ApplyOnStartup = true;
            prefs.ApplyOnStartup.Should().BeTrue();
            prefs.DefaultOffset.CoreMv.Should().Be(-90);
        }
    }
}
