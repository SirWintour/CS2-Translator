using System.Text.Json;
namespace CS2.Translator.Core.Helper;

public sealed class TranslationCache
{
    private const int MaxEntries = 3000;

    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly string _filePath;
    private readonly object _lock = new();

    public TranslationCache(string language)
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CS2-Translator"
        );

        Directory.CreateDirectory(baseDir);
        _filePath = Path.Combine(baseDir, $"cache-{language}.json");

        Load();
    }

    public bool TryGet(string sourceText, out string translation)
    {
        lock (_lock)
        {
            var key = Normalize(sourceText);

            if (_cache.TryGetValue(key, out var entry))
            {
                entry.LastUsed = DateTime.UtcNow;
                translation = entry.Translation;
                return true;
            }

            translation = string.Empty;
            return false;
        }
    }

    public void Set(string sourceText, string translation)
    {
        lock (_lock)
        {
            var key = Normalize(sourceText);

            _cache[key] = new CacheEntry
            {
                Translation = translation,
                LastUsed = DateTime.UtcNow
            };

            EnforceLimit();
            Save();
        }
    }

    private void EnforceLimit()
    {
        if (_cache.Count <= MaxEntries)
            return;

        var removeKeys = _cache
            .OrderBy(kv => kv.Value.LastUsed)
            .Take(_cache.Count - MaxEntries)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in removeKeys)
            _cache.Remove(key);
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
            return;

        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(json);
            if (data == null) return;

            _cache.Clear();
            foreach (var kv in data)
                _cache[kv.Key] = kv.Value;
        }
        catch
        {
            //start fresh
            _cache.Clear();
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(
            _cache,
            new JsonSerializerOptions { WriteIndented = true }
        );

        File.WriteAllText(_filePath, json);
    }

    private static string Normalize(string text)
        => text.Trim();

    private sealed class CacheEntry
    {
        public string Translation { get; set; } = "";
        public DateTime LastUsed { get; set; }
    }
}
