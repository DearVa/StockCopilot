using System;

namespace StockCopilot.Models;

public class StockComparisionMode(string name, Func<decimal, decimal, decimal> data) : 
    SelectableItem<Func<decimal, decimal, decimal>>(name, data);