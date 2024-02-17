using System.Collections.Immutable;
using System.Runtime.Versioning;

namespace StockCopilot.Core.Tests;

public class KLinesDataSourceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task EastMoneyOnlineKLinesDataSourceTest()
    {
        var mockConfiguration = new ConfigurationRoot([new MemoryConfigurationProvider(new MemoryConfigurationSource())]);
        var dataSource = new EastMoneyOnlineKLinesDataSource(mockConfiguration);
        var kLines = await dataSource.GetKLinesAsync(
            "1", "000001", DateTime.Now.Subtract(TimeSpan.FromDays(30)), 
            DateTime.Now, TimeSpan.FromDays(1));
        Assert.That(kLines, Is.Not.Empty);
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public async Task EastMoneyOfflineKLinesDataSourceTest()
    {
        // var provider = new EastMoneyOfflineContentStockProvider();
        // await provider.LoadAsync();

        // var szIndex = "1.000001";
        // var szStocks = provider[szIndex].ToImmutableHashSet();
        // var szContentIndices = provider.Keys
        //     .Where(k => k.Length == 8 && k[..5] == "1.000" && szStocks.Contains(k)).ToArray();
        
        var dataSource = new EastMoneyOfflineKLinesDataSource();
        var result = await dataSource.GetKLinesAsync(
            "1", "002415", DateTime.Now.Subtract(TimeSpan.FromDays(30)), 
            DateTime.Now, TimeSpan.FromDays(1));
    }
}