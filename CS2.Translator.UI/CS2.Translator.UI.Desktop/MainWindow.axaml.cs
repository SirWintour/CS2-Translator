using System;
using Avalonia.Controls;
using CS2.Translator.Core.Services;
using CS2.Translator.UI.ViewModels;
using CS2.Translator.Core.Helper;

namespace CS2.Translator.UI.Views;

public partial class MainWindow : Window
{
    private readonly ConfigService _configService;
    private readonly LogsService _logsService;

    public MainWindow()
    {
        try
        {
            InitializeComponent();
            DebugLog("MainWindow constructor started");

            _configService = new ConfigService();
            DebugLog("Created ConfigService instance");

            _configService.Load();
            DebugLog("Configuration loaded successfully");
            DebugLog($"Config: Language='{_configService.Config.Language}', Player='{_configService.Config.PlayerName}', Path='{_configService.Config.InstallationPath}'");

            var translatorService = new TranslatorService(_configService.Config.Language);
            DebugLog($"Created TranslatorService (TargetLanguage='{_configService.Config.Language}')");

            _logsService = new LogsService(
                _configService.Config.InstallationPath,
                translatorService,
                _configService.Config.Language,
                _configService.Config.PlayerName
            );
            DebugLog("LogsService created successfully");

            var mainVm = new MainViewModel(_logsService, _configService);
            mainVm.SettingsRequested += OpenSettings;
            DebugLog("MainViewModel created and SettingsRequested event bound");
            Content = new MainView(mainVm);
            DebugLog("MainView created and assigned to window content");

            DebugLog("MainWindow fully initialized");
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "MainWindow Constructor");
            Console.WriteLine($"[MainWindow] Initialization failed: {ex.Message}");
        }
    }

    private static void DebugLog(string msg)
    {
        string formatted = $"[MainWindow] | {msg}";
        Console.WriteLine(formatted);
        DebugLogger.Log(formatted);
    }

    private void OpenSettings()
    {
        try
        {
            DebugLog("OpenSettings() invoked → opening SettingsWindow");

            var vm = new SettingsViewModel(_configService);
            var win = new SettingsWindow(vm);

            win.ShowDialog(this);
            DebugLog("SettingsWindow opened successfully");
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "OpenSettings");
        }
    }
}