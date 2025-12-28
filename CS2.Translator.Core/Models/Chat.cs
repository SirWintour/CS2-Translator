using CS2.Translator.Core.Enums;

namespace CS2.Translator.Core.Models;

public class Chat : Log
{
    public ChatType ChatType { get; }
    public string Name { get; }
    public string Message { get; }

    public Translation Translation { get; set; }

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
        
        Translation = new Translation("?", "---");
    }
}