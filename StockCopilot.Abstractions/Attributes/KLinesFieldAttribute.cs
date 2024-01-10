namespace StockCopilot.Abstractions.Attributes;

public class KLinesFieldAttribute(int index) : Attribute
{
    public int Index { get; } = index;
}