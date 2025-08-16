using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LolCounterWpf.Models;

namespace LolCounterWpf.Services;

public sealed class DataDragonService
{
    private readonly HttpClient _http = new();
    private string? _version;
    private List<Champion>? _cache;

    public async Task<string> GetLatestVersionAsync()
    {
        if (_version is not null) return _version;
        var versions = await _http.GetFromJsonAsync<List<string>>(
            "https://ddragon.leagueoflegends.com/api/versions.json");
        _version = versions?.FirstOrDefault() ?? "15.16.1"; // 안전 기본값
        return _version!;
    }

    public async Task<IReadOnlyList<Champion>> GetChampionsAsync(string locale = "ko_KR")
    {
        if (_cache is not null) return _cache;
        var v = await GetLatestVersionAsync();
        var url = $"https://ddragon.leagueoflegends.com/cdn/{v}/data/{locale}/champion.json";
        using var stream = await _http.GetStreamAsync(url);

        using var doc = await JsonDocument.ParseAsync(stream);
        var data = doc.RootElement.GetProperty("data");

        var list = new List<Champion>();
        foreach (var prop in data.EnumerateObject())
        {
            var id = prop.Name; // e.g., "Garen"
            var name = prop.Value.GetProperty("name").GetString()!; // e.g., "가렌"
            var imageUrl = $"https://ddragon.leagueoflegends.com/cdn/{v}/img/champion/{id}.png";
            list.Add(new Champion { Id = id, Name = name, ImageUrl = imageUrl });
        }
        _cache = list.OrderBy(c => c.Name).ToList();
        return _cache;
    }
}