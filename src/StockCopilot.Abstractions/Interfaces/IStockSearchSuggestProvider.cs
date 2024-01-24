using StockCopilot.Abstractions.Models;

namespace StockCopilot.Abstractions.Interfaces;

public interface IStockSearchSuggestProvider
{
    ValueTask<IReadOnlyList<Stock>> GetSearchSuggestsAsync(string searchText, int count);
}