using System.Text.Json;
using StockCopilot.Abstractions.Extensions;
using StockCopilot.Abstractions.Interfaces;
using StockCopilot.Abstractions.Models;
using StockCopilot.Core.JsonSerializerContexts;

namespace StockCopilot.Core.Services;

public class EastMoneyStockSearchSuggestProvider : IStockSearchSuggestProvider
{
    private readonly HttpClient httpClient = new();

    private const string DefaultBaseUrl = "https://search-codetable.eastmoney.com/codetable/search/web";

    public async ValueTask<IReadOnlyList<Stock>> GetSearchSuggestsAsync(string searchText, int count)
    {
        var url = $"{DefaultBaseUrl}?keyword={searchText}&pageIndex=1&pageSize={count}";
        var responseMessage = await httpClient.GetAsync(url);
        responseMessage.EnsureSuccessStatusCode();
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        var response = await JsonSerializer.DeserializeAsync(
            stream,
            EasyMoneyJsonSerializerContext.Default.SearchSuggestResponse);
        return response.NotNull().Result.Select(r => new Stock(
            r.Code, r.ShortName, r.Market.ToString(), r.Pinyin, r.SecurityTypeName)).ToArray();
    }
}