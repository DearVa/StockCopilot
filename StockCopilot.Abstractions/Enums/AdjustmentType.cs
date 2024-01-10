namespace StockCopilot.Abstractions.Enums;

/// <summary>
/// 复权方式
/// </summary>
public enum AdjustmentType
{
    /// <summary>
    /// 不复权
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 前复权
    /// </summary>
    Forward = 1,
    
    /// <summary>
    /// 后复权
    /// </summary>
    Backward = 2,
}