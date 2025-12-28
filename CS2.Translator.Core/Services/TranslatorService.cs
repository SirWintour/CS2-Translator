using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using CS2.Translator.Core.Exceptions;
using CS2.Translator.Core.Models;
using CS2.Translator.Core.Helper;

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

    private readonly WordTranslationCache _wordCache = new(targetLanguage);

    public async Task<Translation> TranslateAsync(string sourceText, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
            return new Translation(targetLang, string.Empty);

        var words = Tokenize(sourceText);
        var result = new List<string>();

        foreach (var word in words)
        {
            if (!IsTranslatable(word))
            {
                result.Add(word);
                continue;
            }

            if (_wordCache.TryGet(word, out var cached))
            {
                result.Add(cached);
                continue;
            }

            // translate single word
            var translated = await TranslateRawAsync(word, targetLang);

            _wordCache.Set(word, translated);
            result.Add(translated);
        }

        return new Translation(targetLang, string.Join("", result));
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

            return parsed[0][0][0].GetString()
                   ?? throw new TranslatorException("Empty response");
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

    private static List<string> Tokenize(string text)
    {
        return Regex.Matches(text, @"\w+|[^\w]+")
            .Select(m => m.Value)
            .ToList();
    }

    private static bool IsTranslatable(string token)
    {
        return Regex.IsMatch(token, @"^[\p{L}]+$");
    }
}
