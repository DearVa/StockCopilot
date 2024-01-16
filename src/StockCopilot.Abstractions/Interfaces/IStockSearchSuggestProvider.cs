using StockCopilot.Abstractions.Models;

namespace StockCopilot.Abstractions.Interfaces;

public interface IStockSearchSuggestProvider
{
    ValueTask<IReadOnlyList<StockSearchSuggest>> GetSearchSuggestsAsync(string searchText, int count);
}