using System.Text.RegularExpressions;
using CS2.Translator.Core.Enums;
using CS2.Translator.Core.Exceptions;
using CS2.Translator.Core.Models;

namespace CS2.Translator.Core.Services;

public sealed class LogsService
{
    public event Action<Chat>? ChatReceived;
    public event Action? ChatsUpdated;

    private readonly LinkedList<Log> _logs = new();
    public List<Chat> Chats { get; } = new();

    private readonly string _logFilePath;
    private readonly TranslatorService _translator;
    private readonly string _targetLanguage;
    private readonly string _playerName;
    private readonly bool _autoTranslate;

    private FileSystemWatcher? _watcher;
    private CancellationTokenSource? _debounceCts;
    private long _lastFilePosition = 0;
    
    // DEBUG
    private static void DebugLog(string msg)
    {
        Console.WriteLine($"[LogsService] {DateTime.Now:HH:mm:ss.fff} | {msg}");
    }
    
    // CTOR
    public LogsService(
        string cs2InstallationPath,
        TranslatorService translator,
        string targetLanguage,
        string playerName,
        bool autoTranslate = true)
    {
        if (string.IsNullOrWhiteSpace(cs2InstallationPath))
            throw new ArgumentException("CS2 path is empty");

        _logFilePath = Path.Combine(
            cs2InstallationPath,
            "game",
            "csgo",
            "console.log"
        );

        _translator = translator;
        _targetLanguage = targetLanguage;
        _playerName = playerName?.Trim() ?? "";
        _autoTranslate = autoTranslate;

        DebugLog("Initialized");
        DebugLog($"Logfile path: {_logFilePath}");
        DebugLog($"PlayerName: '{_playerName}'");
        DebugLog($"AutoTranslate: {_autoTranslate}");
        DebugLog($"TargetLanguage: {_targetLanguage}");
    }
    
    public async Task LoadLogsAsync(int amount)
    {
        DebugLog($"LoadLogsAsync({amount})");

        var lines = await ReadNewLinesAsync();
        DebugLog($"Read lines: {lines.Count}");

        if (lines.Count == 0)
            return;

        var parsed = ParseLines(lines);
        DebugLog($"Parsed chats: {parsed.Count}");

        var newLogs = GetNewLogs(parsed);
        DebugLog($"New logs detected: {newLogs.Count}");

        foreach (var log in newLogs)
            await SaveLogAsync(log);
    }

    public void StartWatching(int loadAmount = 20)
    {
        if (_watcher != null)
        {
            DebugLog("Watcher already running");
            return;
        }

        if (!File.Exists(_logFilePath))
            throw new LogfileNotFoundException();

        var dir = Path.GetDirectoryName(_logFilePath)!;
        var file = Path.GetFileName(_logFilePath);

        DebugLog($"Watching {dir}/{file}");

        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter =
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.FileName
        };

        _watcher.Changed += OnLogFileChanged;
        _watcher.Created += OnLogFileReset;
        _watcher.Renamed += OnLogFileReset;
        
        _watcher.EnableRaisingEvents = true;
        DebugLog("Watcher started");
    }
    
    private async void OnLogFileChanged(object? sender, FileSystemEventArgs e)
    {
        DebugLog("File changed");
        await DebouncedReload(20);
    }

    private async void OnLogFileReset(object? sender, FileSystemEventArgs e)
    {
        DebugLog("Logfile recreated - RESET");

        _lastFilePosition = 0;
        _logs.Clear();
        Chats.Clear();

        await DebouncedReload(20);
    }


    public void StopWatching()
    {
        DebugLog("Stopping watcher");

        _watcher?.Dispose();
        _watcher = null;

        _debounceCts?.Cancel();
        _debounceCts = null;
    }
    // Core
    private async Task SaveLogAsync(Log log)
    {
        DebugLog($"SaveLog: {log.RawString}");

        _logs.AddLast(log);

        if (log is not Chat chat)
            return;

        Chats.Insert(0, chat);
        DebugLog($"Chat added: {chat.Name} -> {chat.Message}");

        if (_autoTranslate && !string.IsNullOrWhiteSpace(chat.Message))
        {
            if (!string.IsNullOrEmpty(_playerName) &&
                chat.Name.Equals(_playerName, StringComparison.OrdinalIgnoreCase))
            {
                DebugLog("Own message detected → skipping translation");
                chat.Translation = new Translation(_targetLanguage, chat.Message);
            }
            else
            {
                try
                {
                    DebugLog("Translating message...");
                    chat.Translation =
                        await _translator.TranslateAsync(
                            chat.Message,
                            _targetLanguage
                        );
                    DebugLog("Translation finished");
                }
                catch (TranslatorException ex)
                {
                    DebugLog($"Translation failed: {ex.Message}");
                    chat.Translation =
                        new Translation(_targetLanguage, $"[error] {ex.Message}");
                }
            }
        }

        DebugLog("Raising ChatReceived event");
        ChatReceived?.Invoke(chat);
        ChatsUpdated?.Invoke();
    }

    private async Task DebouncedReload(int loadAmount)
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(250, _debounceCts.Token);
            DebugLog("Debounce passed");
            await LoadLogsAsync(loadAmount);
        }
        catch (TaskCanceledException)
        {
            DebugLog("Debounce canceled");
        }
    }
    
    // PARSING
    private static List<Chat> ParseLines(IEnumerable<string> lines)
    {
        var chats = new List<Chat>();

        foreach (var line in lines)
        {
            if (!Regex.IsMatch(line, @"\s\s\[\w+\]") && !line.Contains("﹫"))
                continue;

            var split = line.Split([": "], 2, StringSplitOptions.None);
            if (split.Length < 2)
                continue;

            var namePart = split[0].Trim();
            var messagePart = split[1].Trim();

            namePart = Regex.Replace(
                namePart,
                @"\d{1,2}/\d{1,2} \d{1,2}:\d{1,2}:\d{1,2}",
                ""
            );

            namePart = Regex.Replace(namePart, @"\[\w+\]", "");
            namePart = Regex.Replace(namePart, @"﹫\w+", "").Trim();

            chats.Add(new Chat(
                rawString: line,
                chatType: ChatType.All,
                name: namePart,
                message: messagePart
            ));
        }

        return chats;
    }

    private List<Log> GetNewLogs(List<Chat> parsed)
    {
        var result = new List<Log>();

        for (var i = parsed.Count - 1; i >= 0; i--)
        {
            if (_logs.Last == null)
            {
                result.Insert(0, parsed[i]);
                continue;
            }

            if (!Compare(_logs.Last, parsed[i]))
                result.Insert(0, parsed[i]);
            else
                break;
        }

        return result;
    }

    private static bool Compare(LinkedListNode<Log> last, Log current)
    {
        var node = last;

        if (node.Value.RawString != current.RawString)
            return false;

        for (var i = 0; i < 3; i++)
        {
            node = node.Previous;
            if (node == null)
                return true;

            if (node.Value.RawString != current.RawString)
                return false;
        }

        return true;
    }
    
    // FILE IO
    private async Task<List<string>> ReadNewLinesAsync()
    {
        var result = new List<string>();

        if (!File.Exists(_logFilePath))
            throw new LogfileNotFoundException();

        await Task.Run(() =>
        {
            using var fs = new FileStream(
                _logFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );

            if (fs.Length < _lastFilePosition)
            {
                DebugLog("Logfile truncated - full reset");

                _lastFilePosition = 0;
                _logs.Clear();
                Chats.Clear();
            }

            fs.Seek(_lastFilePosition, SeekOrigin.Begin);

            using var sr = new StreamReader(fs);

            while (sr.ReadLine() is { } line)
                result.Add(line);

            _lastFilePosition = fs.Position;
        });

        return result;
    }
}
