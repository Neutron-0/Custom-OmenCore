using System.CommandLine;
using System.Reflection;
using OmenCore.Linux.Commands;
using OmenCore.Linux.Config;
using OmenCore.Linux.Hardware;

namespace OmenCore.Linux;

/// <summary>
/// OmenCore Linux CLI - Command-line utility for controlling HP OMEN laptops.
/// 
/// Usage:
///   omencore-cli fan --profile auto|silent|gaming|max
///   omencore-cli fan --speed 50%
///   omencore-cli fan --curve "40:20,50:30,60:50,80:80,90:100"
///   omencore-cli perf --mode balanced|performance
///   omencore-cli keyboard --color FF0000
///   omencore-cli keyboard --zone 0 --color 00FF00
///   omencore-cli status [--json]
///   omencore-cli monitor [--interval 1000]
///   omencore-cli config --show|--set key=value
///   omencore-cli daemon --start|--stop|--status
/// 
/// Requirements:
///   - Linux kernel with ec_sys module (write_support=1)
///   - HP WMI module for keyboard lighting
///   - Root privileges for EC access
/// </summary>
class Program
{
    /// <summary>
    /// Version is driven by the single &lt;Version&gt; property in OmenCore.Linux.csproj.
    /// Do NOT hardcode a version string here — edit the .csproj instead.
    /// </summary>
    public static readonly string Version =
        typeof(Program).Assembly.GetName().Version is { } v
            ? $"{v.Major}.{v.Minor}.{v.Build}"
            : "unknown";

    public const string BuildDate = "2026-03";
    
    static async Task<int> Main(string[] args)
    {
        // Handle --version and -V before System.CommandLine parsing
        // This avoids the duplicate key issue with System.CommandLine's auto-generated --version
        if (args.Length > 0 && (args[0] == "--version" || args[0] == "-V"))
        {
            PrintVersion();
            return 0;
        }

        var rootCommand = new RootCommand("OmenCore Linux CLI - HP OMEN laptop control utility")
        {
            // Disable built-in --version to avoid duplicate key conflict
            TreatUnmatchedTokensAsErrors = true
        };
        
        // Add commands
        rootCommand.AddCommand(FanCommand.Create());
        rootCommand.AddCommand(PerformanceCommand.Create());
        rootCommand.AddCommand(KeyboardCommand.Create());
        rootCommand.AddCommand(StatusCommand.Create());
        rootCommand.AddCommand(DiagnoseCommand.Create());
        rootCommand.AddCommand(MonitorCommand.Create());
        rootCommand.AddCommand(ConfigCommand.Create());
        rootCommand.AddCommand(DaemonCommand.Create());
        rootCommand.AddCommand(CreateBatteryCommand());
        
        // Add global options
        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose" },
            description: "Enable verbose output");
        rootCommand.AddGlobalOption(verboseOption);
        
        // Note: Don't add global --json as some commands already have it locally
        // This avoids duplicate option conflicts
        
        return await rootCommand.InvokeAsync(args);
    }
    
    private static void PrintVersion()
    {
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine($"║   OmenCore Linux CLI v{Version,-30}  ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine($"║   Build:    {BuildDate,-40}  ║");
        Console.WriteLine($"║   Runtime:  .NET {Environment.Version,-36}  ║");
        Console.WriteLine($"║   OS:       {GetOsInfo(),-40}  ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║   GitHub:   github.com/omencore/omencore              ║");
        Console.WriteLine("║   License:  MIT                                       ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }
    
    private static string GetOsInfo()
    {
        try
        {
            if (File.Exists("/etc/os-release"))
            {
                var lines = File.ReadAllLines("/etc/os-release");
                foreach (var line in lines)
                {
                    if (line.StartsWith("PRETTY_NAME="))
                    {
                        return line.Replace("PRETTY_NAME=", "").Trim('"');
                    }
                }
            }
        }
        catch (Exception ex) when (IsRecoverableSysfsException(ex))
        {
        }
        
        return Environment.OSVersion.ToString();
    }
    
    /// <summary>
    /// Battery command - manages battery-aware fan profiles and power settings.
    /// </summary>
    private static Command CreateBatteryCommand()
    {
        var command = new Command("battery", "Battery status and power management");
        
        var statusSubCommand = new Command("status", "Show battery status");
        statusSubCommand.SetHandler(async () =>
        {
            await ShowBatteryStatusAsync();
        });
        
        var profileSubCommand = new Command("profile", "Set battery profile (affects fans when on battery)");
        var profileNameArg = new Argument<string>("profile", "Profile name: quiet, balanced, performance");
        profileSubCommand.AddArgument(profileNameArg);
        profileSubCommand.SetHandler(async (profileName) =>
        {
            await SetBatteryProfileAsync(profileName);
        }, profileNameArg);
        
        var chargeThresholdCommand = new Command("threshold", "Set battery charge threshold (0 = disabled)");
        var thresholdArg = new Argument<int>("percent", "Stop charging at this percentage (60-100, 0 = disabled)");
        chargeThresholdCommand.AddArgument(thresholdArg);
        chargeThresholdCommand.SetHandler(async (threshold) =>
        {
            await SetChargeThresholdAsync(threshold);
        }, thresholdArg);
        
        command.AddCommand(statusSubCommand);
        command.AddCommand(profileSubCommand);
        command.AddCommand(chargeThresholdCommand);
        
        return command;
    }
    
    private static async Task ShowBatteryStatusAsync()
    {
        var battery = new LinuxBatteryController();
        var batteryPath = DiscoverPowerSupplyPath("battery");
        
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("║               Battery Status                         ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════╣");
        
        int capacity = 0;
        string status = "Unknown";
        int energyNow = 0;
        int energyFull = 0;
        int powerNow = 0;
        bool onAc = !battery.IsOnBattery();
        
        try
        {
            capacity = battery.GetBatteryPercentage() ?? 0;
            status = battery.GetBatteryStatus() ?? "Unknown";

            if (!string.IsNullOrWhiteSpace(batteryPath))
            {
                energyNow = await ReadIntFromFirstExistingAsync(
                    Path.Combine(batteryPath, "energy_now"),
                    Path.Combine(batteryPath, "charge_now"));

                energyFull = await ReadIntFromFirstExistingAsync(
                    Path.Combine(batteryPath, "energy_full"),
                    Path.Combine(batteryPath, "charge_full"));

                powerNow = await ReadIntFromFirstExistingAsync(
                    Path.Combine(batteryPath, "power_now"),
                    Path.Combine(batteryPath, "current_now"));
            }
        }
        catch (Exception ex) when (IsRecoverableSysfsException(ex))
        {
        }
        
        var bar = GetProgressBar(capacity, 100, 30);
        var color = capacity > 60 ? ConsoleColor.Green : capacity > 20 ? ConsoleColor.Yellow : ConsoleColor.Red;
        
        Console.Write($"║  Level:     ");
        Console.ForegroundColor = color;
        Console.Write($"{capacity,3}%");
        Console.ResetColor();
        Console.WriteLine($" [{bar}]    ║");
        
        Console.WriteLine($"║  Status:    {status,-40}  ║");
        Console.WriteLine($"║  Power:     {(onAc ? "AC Adapter" : "Battery"),-40}  ║");
        
        if (powerNow > 0)
        {
            var watts = powerNow / 1000000.0;
            Console.WriteLine($"║  Draw:      {watts:F1} W                                       ║".PadRight(54, ' ') + "  ║");
        }
        
        if (status == "Discharging" && powerNow > 0 && energyNow > 0)
        {
            var hoursRemaining = energyNow / (double)powerNow;
            Console.WriteLine($"║  Remaining: ~{hoursRemaining:F1} hours                                  ║".PadRight(54, ' ') + "  ║");
        }
        
        Console.WriteLine("╚══════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        await Task.CompletedTask;
    }

    private static string? DiscoverPowerSupplyPath(string expectedType)
    {
        const string powerSupplyPath = "/sys/class/power_supply";
        if (!Directory.Exists(powerSupplyPath))
            return null;

        foreach (var dir in Directory.GetDirectories(powerSupplyPath))
        {
            try
            {
                var typePath = Path.Combine(dir, "type");
                if (!File.Exists(typePath))
                    continue;

                var type = File.ReadAllText(typePath).Trim();
                if (type.Equals(expectedType, StringComparison.OrdinalIgnoreCase))
                    return dir;
            }
            catch (Exception ex) when (IsRecoverableSysfsException(ex))
            {
            }
        }

        return null;
    }

    private static async Task<int> ReadIntFromFirstExistingAsync(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (!File.Exists(path))
                continue;

            try
            {
                var text = await File.ReadAllTextAsync(path);
                if (int.TryParse(text.Trim(), out var value))
                    return value;
            }
            catch (Exception ex) when (IsRecoverableSysfsException(ex))
            {
            }
        }

        return 0;
    }

    private static bool IsRecoverableSysfsException(Exception ex) =>
        ex is IOException or UnauthorizedAccessException or FormatException;
    
    private static async Task SetBatteryProfileAsync(string profileName)
    {
        var profile = profileName.ToLower() switch
        {
            "quiet" or "silent" => "quiet",
            "balanced" => "balanced",
            "performance" => "performance",
            _ => null
        };
        
        if (profile == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Invalid profile. Use: quiet, balanced, or performance");
            Console.ResetColor();
            return;
        }
        
            var config = OmenCoreConfig.Load();
            config.Battery.Profile = profile;
            config.Save();
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ Battery profile set to: {profile}");
        Console.ResetColor();
        Console.WriteLine("  The daemon will apply this profile when running on battery.");
        
        await Task.CompletedTask;
    }
    
    private static async Task SetChargeThresholdAsync(int threshold)
    {
        if (threshold != 0 && (threshold < 60 || threshold > 100))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Threshold must be between 60-100, or 0 to disable");
            Console.ResetColor();
            return;
        }
        
        // Try to set via sysfs (HP laptops with omen-wmi)
        const string thresholdPath = "/sys/devices/platform/hp-wmi/charge_threshold";
        
        if (File.Exists(thresholdPath))
        {
            try
            {
                await File.WriteAllTextAsync(thresholdPath, threshold.ToString());
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Charge threshold set to: {(threshold == 0 ? "disabled" : $"{threshold}%")}");
                Console.ResetColor();
                return;
            }
            catch (UnauthorizedAccessException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ Root privileges required. Run with sudo.");
                Console.ResetColor();
                return;
            }
        }
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠ Charge threshold not supported on this device");
        Console.ResetColor();
        
        await Task.CompletedTask;
    }
    
    private static string GetProgressBar(int value, int max, int width)
    {
        var filled = (int)((double)value / max * width);
        var empty = width - filled;
        return new string('█', filled) + new string('░', empty);
    }
}
