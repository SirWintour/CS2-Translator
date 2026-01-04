using System;
using Avalonia.Collections;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CS2.Translator.Core.Exceptions;
using CS2.Translator.Core.Models;
using CS2.Translator.Core.Services;

namespace CS2.Translator.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public AvaloniaList<Chat> Chats { get; } = new();

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
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Chats.Clear();
                foreach (var chat in _logsService.Chats)
                    Chats.Add(CloneForUi(chat));
            });

            StatusText = "Watching CS2 console.log";
        }
        catch (LogfileNotFoundException)
        {
            StatusText = "Waiting for CS2 (console.log not found)";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }
    
    private bool _uiUpdateScheduled;
    private readonly object _uiLock = new();
    private void OnChatReceived(Chat chat)
    {
        lock (_uiLock)
        {
            if (_uiUpdateScheduled)
                return;

            _uiUpdateScheduled = true;
        }

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Chats.Clear();

            foreach (var c in _logsService.Chats)
                Chats.Add(c);

            lock (_uiLock)
                _uiUpdateScheduled = false;
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
        StatusText = "Reloading…";

        Dispatcher.UIThread.Post(Chats.Clear);

        await _logsService.LoadLogsAsync(30);

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var chat in _logsService.Chats)
                Chats.Add(CloneForUi(chat));
        });
        StatusText = "Reloaded";
    }
    private static Chat CloneForUi(Chat c)
    {
        return new Chat(
            rawString: c.RawString,
            chatType: c.ChatType,
            name: c.Name,
            message: c.Message
        )
        {
            Translation = c.Translation
        };
    }

    public event Action? SettingsRequested;
}
