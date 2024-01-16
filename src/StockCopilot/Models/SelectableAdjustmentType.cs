using StockCopilot.Abstractions.Enums;

namespace StockCopilot.Models;

public class SelectableAdjustmentType(string name, AdjustmentType data) : SelectableItem<AdjustmentType>(name, data);