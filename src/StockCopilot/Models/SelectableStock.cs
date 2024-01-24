using StockCopilot.Abstractions.Models;

namespace StockCopilot.Models;

public class SelectableStock(string name, Stock data) : SelectableItem<Stock>(name, data);