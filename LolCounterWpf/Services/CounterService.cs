using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LolCounterWpf.Models;

namespace LolCounterWpf.Services;

public class CounterService : ICounterProvider
{
    private readonly string _path;
    private Dictionary<string, List<CounterEntry>>? _cache;

    public CounterService(string path)
    {
        _path = path;
    }

    private async Task EnsureCacheAsync()
    {
        if (_cache is not null) return;

        if (!File.Exists(_path))
        {
            _cache = new Dictionary<string, List<CounterEntry>>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        using var fs = File.OpenRead(_path);
        _cache = await JsonSerializer.DeserializeAsync<Dictionary<string, List<CounterEntry>>>(fs)
                 ?? new Dictionary<string, List<CounterEntry>>(StringComparer.OrdinalIgnoreCase);
    }

    private async Task WriteDataAsync(Dictionary<string, List<CounterEntry>> data)
    {
        using var fs = File.Create(_path);
        await JsonSerializer.SerializeAsync(fs, data, new JsonSerializerOptions { WriteIndented = true });
        _cache = null; // Invalidate cache
    }

    public async Task<IReadOnlyList<CounterEntry>> GetCountersAsync(string championIdOrName)
    {
        await EnsureCacheAsync();
        if (_cache!.TryGetValue(championIdOrName, out var list)) return list;

        var alt = _cache.Keys.FirstOrDefault(k => string.Equals(k, championIdOrName, StringComparison.OrdinalIgnoreCase));
        if (alt is not null && _cache.TryGetValue(alt, out list)) return list;
        return Array.Empty<CounterEntry>();
    }

    public async Task AddCounterAsync(string championId, CounterEntry newEntry)
    {
        await EnsureCacheAsync();
        var data = new Dictionary<string, List<CounterEntry>>(_cache!, StringComparer.OrdinalIgnoreCase);
        if (!data.TryGetValue(championId, out var entries))
        {
            entries = new List<CounterEntry>();
            data[championId] = entries;
        }
        entries.Add(newEntry);
        await WriteDataAsync(data);
    }

    public async Task UpdateCounterAsync(string championId, string originalOpponentName, CounterEntry updatedEntry)
    {
        await EnsureCacheAsync();
        var data = new Dictionary<string, List<CounterEntry>>(_cache!, StringComparer.OrdinalIgnoreCase);
        if (data.TryGetValue(championId, out var entries))
        {
            var index = entries.FindIndex(e => e.OpponentName == originalOpponentName);
            if (index != -1)
            {
                entries[index] = updatedEntry;
                await WriteDataAsync(data);
            }
        }
    }

    public async Task DeleteCounterAsync(string championId, string opponentName)
    {
        await EnsureCacheAsync();
        var data = new Dictionary<string, List<CounterEntry>>(_cache!, StringComparer.OrdinalIgnoreCase);
        if (data.TryGetValue(championId, out var entries))
        {
            var entryToRemove = entries.FirstOrDefault(e => e.OpponentName == opponentName);
            if (entryToRemove != null)
            {
                entries.Remove(entryToRemove);
                await WriteDataAsync(data);
            }
        }
    }
}
