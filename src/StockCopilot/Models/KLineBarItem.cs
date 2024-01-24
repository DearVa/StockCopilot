using System;
using System.Text;
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

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("数值：").Append(Value).Append("\n日期：").Append(DateTime.ToString(dateTimeFormatString));
        if (SecondaryKLine == null)
        {
            sb.Append("\n对比股票当日无数据");
        }
        return sb.ToString();
    }
}