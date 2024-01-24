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
    public decimal Opening { get; set; }
    
    /// <summary>
    /// 昨收
    /// </summary>
    [KLinesField(53)]
    public decimal Closing { get; set; }
    
    /// <summary>
    /// 最高
    /// </summary>
    [KLinesField(54)]
    public decimal Highest { get; set; }
    
    /// <summary>
    /// 最低
    /// </summary>
    [KLinesField(55)]
    public decimal Lowest { get; set; }
    
    /// <summary>
    /// 成交量
    /// </summary>
    [KLinesField(56)]
    public decimal Volume { get; set; }
    
    /// <summary>
    /// 成交额
    /// </summary>
    [KLinesField(57)]
    public decimal Turnover { get; set; }
    
    /// <summary>
    /// 振幅
    /// </summary>
    [KLinesField(58)]
    public decimal Amplitude { get; set; }
    
    /// <summary>
    /// 涨跌幅
    /// </summary>
    [KLinesField(59)]
    public decimal PriceChangePercentage { get; set; }
    
    /// <summary>
    /// 涨跌额
    /// </summary>
    [KLinesField(60)]
    public decimal PriceChangeAmount { get; set; }
    
    /// <summary>
    /// 换手
    /// </summary>
    [KLinesField(61)]
    public decimal TurnoverRate { get; set; }
}