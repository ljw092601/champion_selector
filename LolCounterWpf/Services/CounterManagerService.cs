
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LolCounterWpf.Models;

namespace LolCounterWpf.Services;

public class CounterManagerService
{
    private readonly string _path;

    public CounterManagerService(string path)
    {
        _path = path;
    }

    private async Task<Dictionary<string, List<CounterEntry>>> ReadDataAsync()
    {
        if (!File.Exists(_path))
        {
            return new Dictionary<string, List<CounterEntry>>(StringComparer.OrdinalIgnoreCase);
        }

        using var fs = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<Dictionary<string, List<CounterEntry>>>(fs)
               ?? new Dictionary<string, List<CounterEntry>>(StringComparer.OrdinalIgnoreCase);
    }

    private async Task WriteDataAsync(Dictionary<string, List<CounterEntry>> data)
    {
        using var fs = File.Create(_path);
        await JsonSerializer.SerializeAsync(fs, data, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task AddCounterAsync(string championId, CounterEntry newEntry)
    {
        var data = await ReadDataAsync();
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
        var data = await ReadDataAsync();
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
        var data = await ReadDataAsync();
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
