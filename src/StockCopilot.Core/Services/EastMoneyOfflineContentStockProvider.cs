using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Text;
using StockCopilot.Abstractions.Extensions;
using StockCopilot.Abstractions.Interfaces;
using StockCopilot.Core.Internals;

namespace StockCopilot.Core.Services;

[SupportedOSPlatform("windows")]
public class EastMoneyOfflineContentStockProvider : IContentStockProvider
{
    private Dictionary<string, IReadOnlyList<string>>? codes;
    
    private IReadOnlyDictionary<string, IReadOnlyList<string>> Codes => 
        codes ?? throw new InvalidOperationException("未加载数据");

    public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator() => Codes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => Codes.Count;
    
    public bool ContainsKey(string key) => Codes.ContainsKey(key);

    public bool TryGetValue(string key, [NotNullWhen(true)] out IReadOnlyList<string>? value) => 
        Codes.TryGetValue(key, out value);

    public IReadOnlyList<string> this[string key] => Codes[key];

    public IEnumerable<string> Keys => Codes.Keys;
    
    public IEnumerable<IReadOnlyList<string>> Values => Codes.Values;
    
    public async ValueTask LoadAsync()
    {
        if (codes == null)
        {
            codes = new Dictionary<string, IReadOnlyList<string>>();
            
            var datFilePath = Path.Combine(EasyMoneyHelper.InstallPath, "data", "ContentStock.dat");
            if (!File.Exists(datFilePath)) throw new FileNotFoundException("未下载板块数据");
            await using var fs = File.OpenRead(datFilePath);
            using var reader = new StreamReader(fs, Encoding.ASCII);
            await reader.ReadLineAsync();  // skip header
            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (line.IsNullOrWhiteSpace()) break;
                var (marketAndCode, _, _, stocks) = line.Split(',', 4);
                codes[marketAndCode] = stocks[2..].Split(':', StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}