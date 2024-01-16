using System;
using Avalonia.Media;
using StockCopilot.Abstractions.Models;

namespace StockCopilot.Models;

public class KLineBarItem(KLine topKLine, KLine? secondaryKLine, string dateTimeFormatString)
{
    public KLine TopKLine { get; } = topKLine;

    public KLine? SecondaryKLine { get; } = secondaryKLine;

    public DateTime DateTime => TopKLine.DateTime;

    public double Value
    {
        get => value;
        set
        {
            this.value = value;
            Color = value switch
            {
                < 0 => Colors.Green,
                > 0 => Colors.Red,
                _ => Colors.Gray
            };
        }
    }

    private double value;
        
    public Color Color { get; set; }

    public override string ToString() => $"数值：{Value}\n日期：{DateTime.ToString(dateTimeFormatString)}";
}