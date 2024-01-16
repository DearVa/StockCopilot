using System;

namespace StockCopilot.Models;

public class SelectableKLineInterval(string name, TimeSpan data) : SelectableItem<TimeSpan>(name, data);