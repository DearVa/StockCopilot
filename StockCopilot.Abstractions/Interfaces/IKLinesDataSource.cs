using StockCopilot.Abstractions.Enums;
using StockCopilot.Abstractions.Models;

namespace StockCopilot.Abstractions.Interfaces;

public interface IKLinesDataSource
{
    ValueTask<IReadOnlyList<KLine>> GetKLinesAsync(
        string code,
        DateTime begin,
        DateTime end,
        TimeSpan interval,
        AdjustmentType adjustmentType = AdjustmentType.None);
}