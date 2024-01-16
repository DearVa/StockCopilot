namespace StockCopilot.Core.Models.HttpContrasts.EastMoney;

[Serializable]
internal record GubaCodeTable<TData>(
    int Status, 
    string Message,
    int TotalCount,
    string BizCode,
    string BizMsg,
    TData[] Data)
    where TData : class;