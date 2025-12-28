namespace CS2.Translator.Core.Models;

public class Translation(string language, string text)
{
    public string Language { get; } = language;
    public string Text { get; } = text;
}