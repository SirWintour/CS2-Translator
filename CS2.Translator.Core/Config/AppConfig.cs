namespace CS2.Translator.Core.Config;

public class AppConfig
{
    public string InstallationPath { get; set; } = "";
    public string Language { get; set; } = "en";
    public string PlayerName { get; set; } = "";
    public double NameFontSize { get; set; } = 14;
    public double TranslationFontSize { get; set; } = 12;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Language))
            Language = "en";

        if (string.IsNullOrWhiteSpace(InstallationPath))
            InstallationPath = GetDefaultCsPath();

        PlayerName ??= "";
    }

    private static string GetDefaultCsPath()
    {
        if (OperatingSystem.IsWindows())
            return @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive";

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".steam/steam/steamapps/common/Counter-Strike Global Offensive"
        );
    }
}