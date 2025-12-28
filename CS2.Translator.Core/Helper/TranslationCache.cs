using System.Text.Json;

namespace CS2.Translator.Core.Helper;

public sealed class WordTranslationCache
{
    private const int MaxEntries = 5000;

    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly string _filePath;
    private readonly object _lock = new();

    public WordTranslationCache(string language)
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "cs2-translator"
        );

        Directory.CreateDirectory(baseDir);
        _filePath = Path.Combine(baseDir, $"cache-{language}.json");

        Load();
    }

    public bool TryGet(string word, out string translation)
    {
        lock (_lock)
        {
            var key = Normalize(word);

            if (_cache.TryGetValue(key, out var entry))
            {
                entry.LastUsed = DateTime.UtcNow;
                translation = entry.Value;
                return true;
            }

            translation = "";
            return false;
        }
    }

    public void Set(string word, string translation)
    {
        lock (_lock)
        {
            var key = Normalize(word);

            _cache[key] = new CacheEntry
            {
                Value = translation,
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

        var toRemove = _cache
            .OrderBy(kv => kv.Value.LastUsed)
            .Take(_cache.Count - MaxEntries)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in toRemove)
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

    private static string Normalize(string s)
        => s.Trim().ToLowerInvariant();

    private sealed class CacheEntry
    {
        public string Value { get; set; } = "";
        public DateTime LastUsed { get; set; }
    }
}
