namespace StockCopilot.Abstractions.Models;

public record Stock(string Code, string Name, string Market, string Pinyin, string SecurityTypeName)
{
    public override string ToString() => Name;
}