using System.Text.Json;

namespace BreachAndBastion.Core.Data;

/// <summary>Reads game data from JSON files under <c>data/</c> (10-technical).</summary>
public static class DataLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static T Load<T>(string path)
    {
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<T>(stream, Options)
               ?? throw new InvalidDataException($"'{path}' is empty.");
    }

    /// <summary>Loads every <c>*.json</c> in <paramref name="directory"/>, keyed by id.</summary>
    public static IReadOnlyDictionary<string, T> LoadAll<T>(string directory, Func<T, string> id)
    {
        var result = new Dictionary<string, T>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.json").Order(StringComparer.Ordinal))
        {
            var item = Load<T>(path);
            if (!result.TryAdd(id(item), item))
                throw new InvalidDataException($"Duplicate id '{id(item)}' in '{path}'.");
        }
        return result;
    }
}
