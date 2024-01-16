using System.Text.Json.Serialization;
using StockCopilot.Abstractions.Models;

namespace StockCopilot.Core.Models.HttpContrasts.EastMoney;

[Serializable]
internal record KLineResponse
{
    [JsonPropertyName("data")] public required InternalData? Data { get; init; }

    public record InternalData
    {
        [JsonPropertyName("code")] public string? Code { get; init; }

        [JsonPropertyName("market")] public int Market { get; init; }

        [JsonPropertyName("name")] public string? Name { get; init; }

        [JsonPropertyName("klines")] public KLine[]? KLines { get; init; }
    }
}