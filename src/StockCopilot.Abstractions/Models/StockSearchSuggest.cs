namespace StockCopilot.Abstractions.Models;

[Serializable]
public record StockSearchSuggest(
    string ShortName,
    string Url,
    string ProtocolFollowUrl,
    string OuterCode,
    string HeadCharacter,
    string RelatedCode);