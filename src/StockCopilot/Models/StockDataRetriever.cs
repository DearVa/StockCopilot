using System;
using StockCopilot.Abstractions.Models;

namespace StockCopilot.Models;

public class StockDataRetriever(string name, (Func<KLine, float> Getter, Action<KLine, float> Setter) data) : 
    SelectableItem<(Func<KLine, float> Getter, Action<KLine, float> Setter)>(name, data);