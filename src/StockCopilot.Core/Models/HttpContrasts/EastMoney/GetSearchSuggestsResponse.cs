using StockCopilot.Abstractions.Models;

namespace StockCopilot.Core.Models.HttpContrasts.EastMoney;

[Serializable]
internal class GetSearchSuggestsResponse
{
    public GubaCodeTable<StockSearchSuggest>? GubaCodeTable { get; init; }
}