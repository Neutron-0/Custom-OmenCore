using FluentAssertions;
using OmenCore.Models;
using Xunit;

namespace OmenCoreApp.Tests.Models
{
    public class DependencyAuditTests
    {
        [Fact]
        public void HasCriticalDegradation_IsTrue_WhenRequiredMissing()
        {
            var audit = new DependencyAudit
            {
                Checks =
                {
                    new DependencyCheck { Name = "HP WMI BIOS", IsRequired = true, IsDetected = false }
                }
            };

            audit.HasCriticalDegradation.Should().BeTrue();
            audit.HasOptionalDegradation.Should().BeFalse();
        }

        [Fact]
        public void HasOptionalDegradation_IsTrue_WhenOnlyOptionalMissing()
        {
            var audit = new DependencyAudit
            {
                Checks =
                {
                    new DependencyCheck { Name = "OGH", IsOptional = true, IsDetected = false },
                    new DependencyCheck { Name = "HP WMI BIOS", IsRequired = true, IsDetected = true }
                }
            };

            audit.HasCriticalDegradation.Should().BeFalse();
            audit.HasOptionalDegradation.Should().BeTrue();
        }
    }
}
