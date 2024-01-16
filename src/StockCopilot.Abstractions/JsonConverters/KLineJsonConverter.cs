using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using StockCopilot.Abstractions.Attributes;
using StockCopilot.Abstractions.Extensions;
using StockCopilot.Abstractions.Models;

namespace StockCopilot.Abstractions.JsonConverters;

public class KLineJsonConverter : JsonConverter<KLine>
{
    private readonly PropertyInfo[] properties;

    public KLineJsonConverter(IReadOnlyList<int> fields2)
    {
        var fields = typeof(KLine)
            .GetProperties()
            .Select(p => (p, p.GetCustomAttribute<KLinesFieldAttribute>()))
            .Where(p => p.Item2 != null)
            .ToList();
        
        properties = new PropertyInfo[fields2.Count];
        for (var i = 0; i < fields2.Count; i++)
        {
            var field = fields.First(p => p.Item2!.Index == fields2[i]);
            properties[i] = field.p;
        }
    }

    public override KLine? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var fields = reader.GetString().NotNull().Split(',');
        if (fields.Length != properties.Length) return null;
        
        var kLine = new KLine();
        for (var i = 0; i < fields.Length; i++)
        {
            var propertyType = properties[i].PropertyType;
            properties[i].SetValue(kLine, Convert.ChangeType(fields[i], propertyType));
        }

        return kLine;
    }

    public override void Write(Utf8JsonWriter writer, KLine value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(string.Join(',', properties.Select(p => p.GetValue(value))));
    }
}

public class KLineJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(KLine);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new KLineJsonConverter([51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61]);
    }
}