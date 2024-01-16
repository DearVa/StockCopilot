using System.Collections.Generic;
using StockCopilot.Abstractions.Models;

namespace StockCopilot.Models;

public record DetailedData(
    string Name,
    IList<(KLine, KLine)> KLineComparision);