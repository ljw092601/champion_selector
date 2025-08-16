namespace LolCounterWpf.Models;

public sealed class Champion
{
    public required string Id { get; init; }       // e.g., "Garen"
    public required string Name { get; init; }     // e.g., "가렌"
    public required string ImageUrl { get; init; } // ddragon full url
}

public sealed class CounterEntry
{
    public required string OpponentId { get; init; }    // e.g., "Darius"
    public required string OpponentName { get; init; }  // e.g., "다리우스"
    public string? Detail { get; set; }

    // Data binding 편의를 위한 이미지 URL (ddragon 버전과 매핑 시 채움)
    public string? ImageUrl { get; set; }
}