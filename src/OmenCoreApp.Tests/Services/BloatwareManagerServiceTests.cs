using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OmenCore.Services;
using OmenCore.Services.BloatwareManager;
using Xunit;

namespace OmenCoreApp.Tests.Services
{
    [Collection("Config Isolation")]
    public class BloatwareManagerServiceTests : IDisposable
    {
        private readonly string _tempLocalAppData;
        private readonly string? _originalLocalAppData;

        public BloatwareManagerServiceTests()
        {
            _originalLocalAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            _tempLocalAppData = Path.Combine(Path.GetTempPath(), $"omen_bloatware_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempLocalAppData);
            Environment.SetEnvironmentVariable("LOCALAPPDATA", _tempLocalAppData);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("LOCALAPPDATA", _originalLocalAppData);
            try
            {
                if (Directory.Exists(_tempLocalAppData))
                {
                    Directory.Delete(_tempLocalAppData, true);
                }
            }
            catch
            {
                // Best effort cleanup only.
            }
        }

        [Fact]
        public async Task RemoveAppAsync_WhenAppAlreadyRemoved_MarksSkippedAndPreservesDetail()
        {
            var logger = new LoggingService();
            logger.Initialize();

            try
            {
                var service = new BloatwareManagerService(logger);
                var app = new BloatwareApp
                {
                    Name = "Victus Hub",
                    PackageId = "victus-hub",
                    Type = BloatwareType.Win32App,
                    IsRemoved = true,
                    CanRestore = false
                };

                string? statusMessage = null;
                service.StatusChanged += message => statusMessage = message;

                var removed = await service.RemoveAppAsync(app, CancellationToken.None);

                removed.Should().BeTrue();
                app.LastRemovalStatus.Should().Be(RemovalStatus.Skipped);
                app.LastRemovalDetail.Should().Be("Item was already removed in this session.");
                app.LastFailureReason.Should().Be(app.LastRemovalDetail);
                statusMessage.Should().Contain("Skipped");
            }
            finally
            {
                logger.Dispose();
            }
        }

        [Fact]
        public async Task RemoveAppsWithRollbackAsync_WhenAppsAreNoOp_SetsSkippedWithoutFailures()
        {
            var logger = new LoggingService();
            logger.Initialize();

            try
            {
                var service = new BloatwareManagerService(logger);
                var apps = new List<BloatwareApp>
                {
                    new()
                    {
                        Name = "Victus Telemetry",
                        PackageId = "victus-telemetry",
                        Type = BloatwareType.ScheduledTask,
                        IsRemoved = true,
                        CanRestore = true
                    },
                    new()
                    {
                        Name = "HP Promotions",
                        PackageId = "hp-promotions",
                        Type = BloatwareType.AppxPackage,
                        IsRemoved = true,
                        CanRestore = true
                    }
                };

                var result = await service.RemoveAppsWithRollbackAsync(apps, cancellationToken: CancellationToken.None);

                result.Completed.Should().BeTrue();
                result.Skipped.Should().HaveCount(2);
                result.Succeeded.Should().BeEmpty();
                result.Failed.Should().BeEmpty();
                result.RollbackSucceeded.Should().BeEmpty();
                result.RollbackFailed.Should().BeEmpty();

                apps.Should().OnlyContain(app => app.LastRemovalStatus == RemovalStatus.Skipped);
                apps.Should().OnlyContain(app => !string.IsNullOrWhiteSpace(app.LastRemovalDetail));
            }
            finally
            {
                logger.Dispose();
            }
        }

        [Fact]
        public void BloatwareApp_WhenOutcomePropertiesUpdated_RaisesPropertyChanged()
        {
            var app = new BloatwareApp();
            var changedProperties = new List<string>();
            app.PropertyChanged += (_, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.PropertyName))
                {
                    changedProperties.Add(args.PropertyName!);
                }
            };

            app.LastRemovalStatus = RemovalStatus.Skipped;
            app.LastRemovalDetail = "Already absent";
            app.LastFailureReason = "Already absent";

            changedProperties.Should().Contain(nameof(BloatwareApp.LastRemovalStatus));
            changedProperties.Should().Contain(nameof(BloatwareApp.LastRemovalDetail));
            changedProperties.Should().Contain(nameof(BloatwareApp.LastFailureReason));
        }

        [Fact]
        public void BloatwareDependencyMetadataRules_WhenOmenControlComponent_MarksConflictSensitive()
        {
            var app = new BloatwareApp
            {
                Name = "OMEN Light Studio",
                PackageId = "OmenLightStudio",
                Description = "OMEN lighting/control component",
                RemovalRisk = RemovalRisk.Low
            };

            BloatwareDependencyMetadataRules.Apply(app);

            app.ConflictSensitive.Should().BeTrue();
            app.RequiresOmenCoreValidation.Should().BeTrue();
            ((int)app.RemovalRisk).Should().BeGreaterThanOrEqualTo((int)RemovalRisk.Medium);
            app.DependencyTags.Should().Contain("may affect lighting handoff");
            app.DependencyTags.Should().Contain("safe only after OmenCore hardware validation");
            app.DependencyNotes.Should().Contain("Verify OmenCore fan control");
        }

        [Fact]
        public void BloatwareDependencyMetadataRules_WhenHpHotkeyComponent_AddsHotkeyDependency()
        {
            var app = new BloatwareApp
            {
                Name = "HPSystemEventUtility",
                RemovalRisk = RemovalRisk.Low
            };

            BloatwareDependencyMetadataRules.Apply(app);

            ((int)app.RemovalRisk).Should().BeGreaterThanOrEqualTo((int)RemovalRisk.Medium);
            app.DependencyTags.Should().Contain("may affect HP hotkeys");
            app.DependencyNotes.Should().Contain("Fn/media/hotkey");
        }

        [Fact]
        public void BloatwareDependencyMetadataRules_WhenXboxComponent_AddsGamePassDependency()
        {
            var app = new BloatwareApp
            {
                Name = "Microsoft.XboxApp",
                RemovalRisk = RemovalRisk.Low
            };

            BloatwareDependencyMetadataRules.Apply(app);

            ((int)app.RemovalRisk).Should().BeGreaterThanOrEqualTo((int)RemovalRisk.Medium);
            app.DependencyTags.Should().Contain("Game Pass/Xbox dependency");
            app.DependencyNotes.Should().Contain("Game Pass");
        }

        [Fact]
        public void BuildIndependenceReadinessChecklist_WhenOmenComponentSelected_ReturnsHardwareValidationItems()
        {
            var apps = new[]
            {
                new BloatwareApp
                {
                    Name = "OMEN Light Studio",
                    ConflictSensitive = true,
                    RequiresOmenCoreValidation = true
                }
            };

            var checklist = BloatwareManagerService.BuildIndependenceReadinessChecklist(apps);

            checklist.Should().Contain(item => item.Contains("Fan control verified"));
            checklist.Should().Contain(item => item.Contains("Keyboard lighting/RGB"));
            checklist.Should().Contain(item => item.Contains("OMEN key"));
            checklist.Should().Contain(item => item.Contains("restore point"));
        }

        [Fact]
        public void ExportDryRunReport_WritesTargetsDependenciesAndRestorePaths()
        {
            var logger = new LoggingService();
            logger.Initialize();

            try
            {
                var service = new BloatwareManagerService(logger);
                var apps = new List<BloatwareApp>
                {
                    new()
                    {
                        Name = "OMEN Light Studio",
                        PackageId = "OmenLightStudio",
                        Publisher = "HP",
                        Type = BloatwareType.Win32App,
                        Category = BloatwareCategory.OemSoftware,
                        Description = "OMEN lighting/control component",
                        RemovalRisk = RemovalRisk.High,
                        UninstallCommand = "omen-light-studio-uninstall.exe",
                        ConflictSensitive = true,
                        RequiresOmenCoreValidation = true,
                        DependencyTags = new List<string> { "may affect lighting handoff" },
                        DependencyNotes = "Verify OmenCore keyboard lighting before removal."
                    },
                    new()
                    {
                        Name = "HP Telemetry Task",
                        PackageId = @"\HP\Telemetry",
                        Publisher = "HP",
                        Type = BloatwareType.ScheduledTask,
                        Category = BloatwareCategory.Telemetry,
                        Description = "HP telemetry scheduled task",
                        RemovalRisk = RemovalRisk.Low,
                        CanRestore = true
                    }
                };

                var path = service.ExportDryRunReport(apps);

                path.Should().NotBeNull();
                File.Exists(path!).Should().BeTrue();
                var content = File.ReadAllText(path!);

                content.Should().Contain("OmenCore Bloatware Dry Run Report");
                content.Should().Contain("OMEN Light Studio");
                content.Should().Contain("Win32 uninstall command: omen-light-studio-uninstall.exe");
                content.Should().Contain("may affect lighting handoff");
                content.Should().Contain("Scheduled task: \\HP\\Telemetry");
                content.Should().Contain("Expected restore path:");
                content.Should().Contain("INDEPENDENCE READINESS CHECKLIST");
            }
            finally
            {
                logger.Dispose();
            }
        }

        [Fact]
        public void ExtractScheduledTaskNamesFromSchtasksCsv_HandlesQuotedCommas()
        {
            var csv = string.Join(Environment.NewLine,
                "\"HostName\",\"TaskName\",\"Next Run Time\",\"Task To Run\"",
                "\"DESKTOP\",\"\\HP\\Telemetry, Analytics\",\"N/A\",\"C:\\Program Files\\HP\\Telemetry.exe /name \"\"HP, Analytics\"\"\"",
                "\"DESKTOP\",\"\\Microsoft\\Windows\\SafeTask\",\"N/A\",\"C:\\Windows\\System32\\cmd.exe\"");

            var taskNames = BloatwareManagerService.ExtractScheduledTaskNamesFromSchtasksCsv(csv);

            taskNames.Should().Equal(@"\HP\Telemetry, Analytics", @"\Microsoft\Windows\SafeTask");
        }

        [Fact]
        public void ExtractScheduledTaskNamesFromSchtasksCsv_UsesTaskNameHeaderWhenColumnOrderChanges()
        {
            var csv = """
                "HostName","Status","Task To Run","TaskName"
                "DESKTOP","Ready","C:\Program Files\HP\Task.exe","\HP\Task With Reordered Columns"
                """;

            var taskNames = BloatwareManagerService.ExtractScheduledTaskNamesFromSchtasksCsv(csv);

            taskNames.Should().Equal(@"\HP\Task With Reordered Columns");
        }
    }
}
