namespace CS2.Translator.Core.Models;

public abstract class Log(string rawString)
{
    public string RawString { get; } = rawString;
}