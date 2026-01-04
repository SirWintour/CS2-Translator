using Avalonia.Controls;
using CS2.Translator.Core.Services;
using CS2.Translator.UI.ViewModels;

namespace CS2.Translator.UI.Views;

public partial class MainWindow : Window
{
    private readonly ConfigService _configService;
    private readonly LogsService _logsService;

    public MainWindow()
    {
        InitializeComponent();
        
        _configService = new ConfigService();
        _configService.Load();
        
        var translatorService = new TranslatorService(
            _configService.Config.Language
        );


        _logsService = new LogsService(
            _configService.Config.InstallationPath,
            translatorService,
            _configService.Config.Language,
            _configService.Config.PlayerName
        );
        
        var mainVm = new MainViewModel(
            _logsService,
            _configService
        );

        mainVm.SettingsRequested += OpenSettings;
        
        Content = new MainView(mainVm);
    }

    private void OpenSettings()
    {
        var vm = new SettingsViewModel(_configService);

        var win = new SettingsWindow(vm);

        win.ShowDialog(this);
    }
}