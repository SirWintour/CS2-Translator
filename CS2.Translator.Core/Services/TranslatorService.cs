using System.Text.Json;
using CS2.Translator.Core.Exceptions;
using CS2.Translator.Core.Models;
using CS2.Translator.Core.Helper;

namespace CS2.Translator.Core.Services;

public sealed class TranslatorService
{
    private static readonly HttpClient Http = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "CS2-Translator" }
        }
    };

    private readonly string _targetLanguage;
    private readonly TranslationCache _cache;

    public TranslatorService(string targetLanguage)
    {
        _targetLanguage = targetLanguage;
        _cache = new TranslationCache(targetLanguage);

        DebugLog($"TranslatorService initialized (TargetLanguage='{_targetLanguage}')");
    }
    
    private static void DebugLog(string msg)
    {
        string formatted = $"[TranslatorService] | {msg}";
        Console.WriteLine(formatted);
        DebugLogger.Log(formatted);
    }

    public async Task<Translation> TranslateAsync(string sourceText, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
        {
            DebugLog("[TRANSLATE] Empty source text - returning empty translation");
            return new Translation(targetLang, string.Empty);
        }

        if (_cache.TryGet(sourceText, out var cached))
        {
            DebugLog($"[CACHE] Cache hit for \"{sourceText}\" - '{cached}'");
            return new Translation(targetLang, cached);
        }

        DebugLog($"[TRANSLATE] Requesting translation '{sourceText}' - {targetLang}");

        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var translated = await TranslateRawAsync(sourceText, targetLang);

            sw.Stop();
            DebugLog($"[TRANSLATE] Translation completed in {sw.ElapsedMilliseconds}ms - '{translated}'");

            _cache.Set(sourceText, translated);
            DebugLog($"[CACHE] Stored translation in cache (length={translated.Length})");

            return new Translation(targetLang, translated);
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "TranslateAsync");
            throw;
        }
    }

    private static async Task<string> TranslateRawAsync(string text, string lang)
    {
        var url =
            $"https://translate.googleapis.com/translate_a/single" +
            $"?client=gtx&sl=auto&tl={lang}&dt=t&q={Uri.EscapeDataString(text)}";

        DebugLog($"[HTTP] Sending Google Translate request - {lang}, len={text.Length}");

        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var json = await Http.GetStringAsync(url);
            sw.Stop();

            DebugLog($"[HTTP] Received response in {sw.ElapsedMilliseconds}ms (size={json.Length} chars)");

            var parsed = JsonSerializer.Deserialize<JsonElement>(json);

            var parts = new List<string>();
            foreach (var segment in parsed[0].EnumerateArray())
            {
                var part = segment[0].GetString();
                if (!string.IsNullOrWhiteSpace(part))
                    parts.Add(part);
            }

            var result = string.Concat(parts);
            DebugLog($"[PARSE] Extracted translated text: '{result}'");

            return result;
        }
        catch (HttpRequestException ex)
        {
            DebugLog($"[ERROR] No internet or network failure - {ex.Message}");
            throw new NoInternetException();
        }
        catch (TaskCanceledException)
        {
            DebugLog("[ERROR] Google Translate request timed out");
            throw new GoogleTranslateTimeoutException();
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "TranslateRawAsync");
            throw;
        }
    }
}
