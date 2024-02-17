using System;

namespace StockCopilot.Models;

public class StockComparisionMode(string name, Func<float, float, float> data) : 
    SelectableItem<Func<float, float, float>>(name, data);