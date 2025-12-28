using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CS2.Translator.Core.Models;
using CS2.Translator.Core.Services;
using CS2.Translator.Core.Exceptions;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace CS2.Translator.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<Chat> Chats { get; } = new();

    private readonly LogsService _logsService;
    private readonly ConfigService _configService;

    [ObservableProperty]
    private string _statusText = "Idle";

    public MainViewModel(
        LogsService logsService,
        ConfigService configService)
    {
        _logsService = logsService;
        _configService = configService;
        _logsService.ChatReceived += OnChatReceived;
        
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            StatusText = "Loading logs…";

            await _logsService.LoadLogsAsync(30);
            _logsService.StartWatching();

            Dispatcher.UIThread.Post(() =>
            {
                Chats.Clear();
                foreach (var chat in _logsService.Chats)
                    Chats.Add(chat);
            });

            StatusText = "Watching CS2 console.log";
        }
        catch (LogfileNotFoundException)
        {
            StatusText = "Waiting for CS2 (console.log not found)";
        }
    }



    private void OnChatReceived(Chat chat)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Chats.Insert(0, chat);
        });
    }


    [RelayCommand]
    private void OpenSettings()
    {
        SettingsRequested?.Invoke();
    }

    [RelayCommand]
    private async Task Reload()
    {
        Dispatcher.UIThread.Post(() => Chats.Clear());

        await _logsService.LoadLogsAsync(30);

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var chat in _logsService.Chats)
                Chats.Add(chat);
        });
    }


    public event Action? SettingsRequested;
}
