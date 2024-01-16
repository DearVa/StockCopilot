using System.Text.Json;
using System.Text.Json.Serialization;
using StockCopilot.Abstractions.Extensions;
using StockCopilot.Abstractions.Interfaces;
using StockCopilot.Abstractions.Models;
using StockCopilot.Core.JsonSerializerContexts;
using StockCopilot.Core.Models.HttpContrasts.EastMoney;

namespace StockCopilot.Core.Services;

public class EastMoneyStockSearchSuggestProvider : IStockSearchSuggestProvider
{
    private readonly HttpClient httpClient = new();

    private const string DefaultBaseUrl = "https://searchadapter.eastmoney.com/api/suggest/get";

    public async ValueTask<IReadOnlyList<StockSearchSuggest>> GetSearchSuggestsAsync(string searchText, int count)
    {
        var url = $"{DefaultBaseUrl}?input={searchText}&type=8&count={count}";
        var responseMessage = await httpClient.GetAsync(url);
        responseMessage.EnsureSuccessStatusCode();
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        var response = (await JsonSerializer.DeserializeAsync(
            stream,
            typeof(GetSearchSuggestsResponse),
            EasyMoneyJsonSerializerContext.Default)).NotNull<GetSearchSuggestsResponse>();
        return response.GubaCodeTable.NotNull().Data;
    }
}