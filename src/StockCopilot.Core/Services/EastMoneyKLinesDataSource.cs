using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StockCopilot.Abstractions.Enums;
using StockCopilot.Abstractions.Extensions;
using StockCopilot.Abstractions.Interfaces;
using StockCopilot.Abstractions.Models;
using StockCopilot.Core.JsonSerializerContexts;
using StockCopilot.Core.Models.HttpContrasts.EastMoney;

namespace StockCopilot.Core.Services;

public class EastMoneyKLinesDataSource(IConfiguration configuration) : IKLinesDataSource
{
    private readonly HttpClient httpClient = new();

    private static readonly int[] Fields2 = [51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61];

    private const string DefaultBaseUrl = "https://push2his.eastmoney.com/api/qt/stock/kline/get";

    private const string ErrorString = "Network error or Unsupported market";

    public async ValueTask<IReadOnlyList<KLine>> GetKLinesAsync(
        string code,
        DateTime begin,
        DateTime end,
        TimeSpan interval,
        AdjustmentType adjustmentType = AdjustmentType.None)
    {
        var baseUrl = configuration.GetSection(nameof(EastMoneyKLinesDataSource))["BaseUrl"] ?? DefaultBaseUrl;
        string market;
        if (code.StartsWith('0'))
        {
            market = "0";
        }
        else if (code.StartsWith("hk"))
        {
            market = "106";
            code = code[2..];
        }
        else
        {
            market = "1";
        }
        var urlBuilder = new StringBuilder(baseUrl)
            .Append("?fqt=").Append(adjustmentType switch
            {
                AdjustmentType.Forward => 1,
                AdjustmentType.Backward => 2,
                _ => 0
            })
            .Append("&secid=").Append(market).Append('.').Append(code)
            .Append("&fields1=f1&fields2=f")
            .Append(string.Join(",f", Fields2))
            .Append("&beg=").Append(begin.ToString("yyyyMMdd"))
            .Append("&end=").Append(end.ToString("yyyyMMdd"))
            .Append("&klt=").Append((long)interval.TotalMinutes switch
            {
                5L => "5",
                15L => "15",
                30L => "30",
                60L => "60",
                60L * 24 => "101",
                60L * 24 * 7 => "102",
                60L * 24 * 30 => "103",
                _ => throw new ArgumentOutOfRangeException(nameof(interval))
            });
        var response = await httpClient.GetAsync(urlBuilder.ToString());
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        var kLineResponse = (await JsonSerializer.DeserializeAsync(
            stream,
            typeof(KLineResponse),
            EasyMoneyJsonSerializerContext.Default)).NotNull<KLineResponse>();
        return kLineResponse.Data.NotNull(ErrorString).KLines.NotNull(ErrorString);
    }
}