using StockCopilot.Abstractions.Attributes;

namespace StockCopilot.Abstractions.Models;

[Serializable]
public record KLine
{
    /// <summary>
    /// 日期
    /// </summary>
    [KLinesField(51)]
    public DateTime DateTime { get; init; }
    
    /// <summary>
    /// 开盘
    /// </summary>
    [KLinesField(52)]
    public decimal Opening { get; init; }
    
    /// <summary>
    /// [KLinesField(51)]
    /// </summary>
    [KLinesField(53)]
    public decimal Closing { get; init; }
    
    /// <summary>
    /// 最高
    /// </summary>
    [KLinesField(54)]
    public decimal Highest { get; init; }
    
    /// <summary>
    /// 最低
    /// </summary>
    [KLinesField(55)]
    public decimal Lowest { get; init; }
    
    /// <summary>
    /// 成交量
    /// </summary>
    [KLinesField(56)]
    public ulong Volume { get; init; }
    
    /// <summary>
    /// 成交额
    /// </summary>
    [KLinesField(57)]
    public decimal Turnover { get; init; }
    
    /// <summary>
    /// 振幅
    /// </summary>
    [KLinesField(58)]
    public decimal Amplitude { get; init; }
    
    /// <summary>
    /// 涨跌幅
    /// </summary>
    [KLinesField(59)]
    public decimal PriceChangePercentage { get; init; }
    
    /// <summary>
    /// 涨跌额
    /// </summary>
    [KLinesField(60)]
    public decimal PriceChangeAmount { get; init; }
    
    /// <summary>
    /// 换手率
    /// </summary>
    [KLinesField(61)]
    public decimal TurnoverRate { get; init; }
}