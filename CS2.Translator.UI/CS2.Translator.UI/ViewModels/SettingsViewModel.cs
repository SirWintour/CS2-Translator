using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CS2.Translator.Core.Services;
using System;

namespace CS2.Translator.UI.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ConfigService _configService;

    [ObservableProperty]
    private string _installationPath;

    [ObservableProperty]
    private string _language;

    [ObservableProperty]
    private string _playerName;
    
    [ObservableProperty]
    private double _nameFontSize;

    [ObservableProperty]
    private double _translationFontSize;

    public SettingsViewModel(ConfigService configService)
    {
        _configService = configService;

        InstallationPath = _configService.Config.InstallationPath;
        Language = _configService.Config.Language;
        PlayerName = _configService.Config.PlayerName;
        
        NameFontSize = _configService.Config.NameFontSize <= 0
            ? 14
            : _configService.Config.NameFontSize;

        TranslationFontSize = _configService.Config.TranslationFontSize <= 0
            ? 12
            : _configService.Config.TranslationFontSize;
    }

    public event Action? CloseRequested;

    [RelayCommand]
    private void Save()
    {
        _configService.Config.InstallationPath = InstallationPath;
        _configService.Config.Language = Language;
        _configService.Config.PlayerName = PlayerName;

        _configService.Config.NameFontSize = NameFontSize;
        _configService.Config.TranslationFontSize = TranslationFontSize;

        _configService.Save();

        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}