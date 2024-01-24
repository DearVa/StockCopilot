using System;
using StockCopilot.Abstractions.Models;

namespace StockCopilot.Models;

public class StockDataRetriever(string name, (Func<KLine, decimal> Getter, Action<KLine, decimal> Setter) data) : 
    SelectableItem<(Func<KLine, decimal> Getter, Action<KLine, decimal> Setter)>(name, data);