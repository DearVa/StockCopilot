using System.Text.Json.Serialization;

namespace StockCopilot.Abstractions.Models;

[Serializable]
public record KLineResponse
{
    [JsonPropertyName("data")]
    public required InternalData? Data { get; init; }
    
    public record InternalData
    {
        [JsonPropertyName("code")]
        public string? Code { get; init; }
        
        [JsonPropertyName("market")]
        public int Market { get; init; }
        
        [JsonPropertyName("name")]
        public string? Name { get; init; }
        
        [JsonPropertyName("klines")]
        public KLine[]? KLines { get; init; }
    }
}