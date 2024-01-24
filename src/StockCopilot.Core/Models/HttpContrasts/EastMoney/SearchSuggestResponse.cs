using System.Text.Json.Serialization;

namespace StockCopilot.Core.Models.HttpContrasts.EastMoney;

[Serializable]
internal record SearchSuggestResponse
{
    [JsonPropertyName("result")] public required InternalResult[] Result { get; init; }
    
    public record InternalResult
    {
        [JsonPropertyName("shortName")] public required string ShortName { get; init; }
        
        [JsonPropertyName("code")] public required string Code { get; init; }
        
        [JsonPropertyName("securityTypeName")] public required string SecurityTypeName { get; init; }
        
        [JsonPropertyName("market")] public required int Market { get; init; }
        
        [JsonPropertyName("pinyin")] public required string Pinyin { get; init; }
    }
}