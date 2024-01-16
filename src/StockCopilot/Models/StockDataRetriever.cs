using System;
using StockCopilot.Abstractions.Models;

namespace StockCopilot.Models;

public class StockDataRetriever(string name, Func<KLine, decimal> data) : 
    SelectableItem<Func<KLine, decimal>>(name, data);