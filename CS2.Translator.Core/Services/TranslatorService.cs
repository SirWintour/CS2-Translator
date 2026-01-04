using System.Text.Json;
using CS2.Translator.Core.Exceptions;
using CS2.Translator.Core.Models;
using CS2.Translator.Core.Helper;
using System.Text;

namespace CS2.Translator.Core.Services;

public sealed class TranslatorService(string targetLanguage)
{
    private static readonly HttpClient Http = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "CS2-Translator" }
        }
    };

    private readonly string _targetLanguage = targetLanguage;
    private readonly TranslationCache _cache = new(targetLanguage);

    public async Task<Translation> TranslateAsync(string sourceText, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
            return new Translation(targetLang, string.Empty);

        if (_cache.TryGet(sourceText, out var cached))
            return new Translation(targetLang, cached);

        var translated = await TranslateRawAsync(sourceText, targetLang);

        _cache.Set(sourceText, translated);

        return new Translation(targetLang, translated);
    }

    private static async Task<string> TranslateRawAsync(string text, string lang)
    {
        var url =
            $"https://translate.googleapis.com/translate_a/single" +
            $"?client=gtx&sl=auto&tl={lang}&dt=t&q={Uri.EscapeDataString(text)}";

        try
        {
            var json = await Http.GetStringAsync(url);
            var parsed = JsonSerializer.Deserialize<JsonElement>(json);

            var parts = new List<string>();

            foreach (var segment in parsed[0].EnumerateArray())
            {
                var part = segment[0].GetString();
                if (!string.IsNullOrWhiteSpace(part))
                    parts.Add(part);
            }

            return string.Concat(parts);
        }
        catch (HttpRequestException)
        {
            throw new NoInternetException();
        }
        catch (TaskCanceledException)
        {
            throw new GoogleTranslateTimeoutException();
        }
    }
}
