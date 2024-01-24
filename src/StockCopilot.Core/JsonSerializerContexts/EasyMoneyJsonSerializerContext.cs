using System.Text.Json.Serialization;
using StockCopilot.Abstractions.JsonConverters;
using StockCopilot.Core.Models.HttpContrasts.EastMoney;

namespace StockCopilot.Core.JsonSerializerContexts;

[JsonSerializable(typeof(SearchSuggestResponse))]
[JsonSerializable(typeof(KLineResponse))]
[JsonSourceGenerationOptions(Converters = [typeof(KLineJsonConverterFactory)])]
internal partial class EasyMoneyJsonSerializerContext : JsonSerializerContext;