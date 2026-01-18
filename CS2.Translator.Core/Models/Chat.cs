using System.ComponentModel;
using System.Runtime.CompilerServices;
using CS2.Translator.Core.Enums;

namespace CS2.Translator.Core.Models;

public class Chat : Log, INotifyPropertyChanged
{
    public ChatType ChatType { get; }
    public string Name { get; }
    public string Message { get; }

    private Translation _translation;
    public Translation Translation
    {
        get => _translation;
        set
        {
            if (_translation == value)
                return;

            _translation = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TranslationText));
        }
    }

    public string TranslationText => Translation?.Text ?? string.Empty;

    public Chat(
        string rawString,
        ChatType chatType,
        string name,
        string message
    ) : base(rawString)
    {
        ChatType = chatType;
        Name = name;
        Message = message;
        _translation = new Translation("?", "---");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}