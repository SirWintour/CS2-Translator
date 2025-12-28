using System.Text.Json;
using CS2.Translator.Core.Config;

namespace CS2.Translator.Core.Services;

public class ConfigService
{
    public AppConfig Config { get; private set; } = new();

    private readonly string _configPath = GetConfigPath();

    public void Load()
    {
        if (!File.Exists(_configPath))
        {
            Config.Validate();
            Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            Config = JsonSerializer.Deserialize<AppConfig>(json)
                     ?? new AppConfig();
        }
        catch
        {
            Config = new AppConfig();
        }

        Config.Validate();
    }

    public void Save()
    {
        Config.Validate();

        var dir = Path.GetDirectoryName(_configPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir!);

        var json = JsonSerializer.Serialize(
            Config,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_configPath, json);
    }

    private static string GetConfigPath()
    {
        string baseDir;

        if (OperatingSystem.IsWindows())
        {
            baseDir = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData
            );
        }
        else
        {
            baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config"
            );
        }

        return Path.Combine(
            baseDir,
            "CS2-Translator",
            "config.json"
        );
    }
}