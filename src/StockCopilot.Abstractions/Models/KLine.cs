using StockCopilot.Abstractions.Attributes;

namespace StockCopilot.Abstractions.Models;

[Serializable]
public record KLine
{
    /// <summary>
    /// 日期
    /// </summary>
    [KLinesField(51)]
    public DateTime DateTime { get; set; }
    
    /// <summary>
    /// 今开
    /// </summary>
    [KLinesField(52)]
    public float Opening { get; set; } = float.NaN;
    
    /// <summary>
    /// 昨收
    /// </summary>
    [KLinesField(53)]
    public float Closing { get; set; } = float.NaN;
    
    /// <summary>
    /// 最高
    /// </summary>
    [KLinesField(54)]
    public float Highest { get; set; } = float.NaN;
    
    /// <summary>
    /// 最低
    /// </summary>
    [KLinesField(55)]
    public float Lowest { get; set; } = float.NaN;

    /// <summary>
    /// 成交量
    /// </summary>
    [KLinesField(56)]
    public float Volume { get; set; } = float.NaN;
    
    /// <summary>
    /// 成交额
    /// </summary>
    [KLinesField(57)]
    public float Turnover { get; set; } = float.NaN;
    
    /// <summary>
    /// 振幅
    /// </summary>
    [KLinesField(58)]
    public float Amplitude { get; set; } = float.NaN;
    
    /// <summary>
    /// 涨跌幅
    /// </summary>
    [KLinesField(59)]
    public float PriceChangePercentage { get; set; } = float.NaN;
    
    /// <summary>
    /// 涨跌额
    /// </summary>
    [KLinesField(60)]
    public float PriceChangeAmount { get; set; } = float.NaN;
    
    /// <summary>
    /// 换手
    /// </summary>
    [KLinesField(61)]
    public float TurnoverRate { get; set; } = float.NaN;
}