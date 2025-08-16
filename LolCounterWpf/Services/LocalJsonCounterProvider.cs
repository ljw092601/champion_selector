using System.Collections.Generic;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LolCounterWpf.Models;

namespace LolCounterWpf.Services;

public sealed class LocalJsonCounterProvider(string path) : ICounterProvider
{
    private readonly string _path = path;
    private Dictionary<string, List<CounterEntry>>? _cache;

    private async Task EnsureAsync()
    {
        if (_cache is not null) return;
        using var fs = File.OpenRead(_path);
        _cache = await JsonSerializer.DeserializeAsync<Dictionary<string, List<CounterEntry>>>(fs)
                 ?? new();
    }

    public async Task<IReadOnlyList<CounterEntry>> GetCountersAsync(string championIdOrName)
    {
        await EnsureAsync();
        if (_cache!.TryGetValue(championIdOrName, out var list)) return list;

        // 이름(key) 또는 Id로 접근 허용
        var alt = _cache.Keys.FirstOrDefault(k => string.Equals(k, championIdOrName, StringComparison.OrdinalIgnoreCase));
        if (alt is not null && _cache.TryGetValue(alt, out list)) return list;
        return Array.Empty<CounterEntry>();
    }
}