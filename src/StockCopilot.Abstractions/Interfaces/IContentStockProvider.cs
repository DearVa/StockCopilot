namespace StockCopilot.Abstractions.Interfaces;

public interface IContentStockProvider : IReadOnlyDictionary<string, IReadOnlyList<string>>
{
    ValueTask LoadAsync();
}