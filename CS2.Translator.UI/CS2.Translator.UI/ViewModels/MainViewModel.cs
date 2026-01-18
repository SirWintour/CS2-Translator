using System;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CS2.Translator.Core.Exceptions;
using CS2.Translator.Core.Models;
using CS2.Translator.Core.Services;

namespace CS2.Translator.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const int MaxChats = 150;
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

            await Dispatcher.UIThread.InvokeAsync(FullRefresh);

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
    private void OnChatReceived(Chat chat)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Chats.Insert(0, CloneForUi(chat));
            EnforceLimit();
        });
    }

    private void FullRefresh()
    {
        Chats.Clear();

        foreach (var chat in _logsService.Chats)
        {
            Chats.Add(CloneForUi(chat));
            EnforceLimit();
        }
    }

    private void EnforceLimit()
    {
        while (Chats.Count > MaxChats)
            Chats.RemoveAt(0);
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

        await _logsService.LoadLogsAsync(30);

        Dispatcher.UIThread.Post(FullRefresh);

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
