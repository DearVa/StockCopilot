namespace StockCopilot.Core.Tests;

public class KLinesDataSourceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task EastMoneyKLinesDataSourceTest()
    {
        var mockConfiguration = new ConfigurationRoot([new MemoryConfigurationProvider(new MemoryConfigurationSource())]);
        var dataSource = new EastMoneyKLinesDataSource(mockConfiguration);
        var kLines = await dataSource.GetKLinesAsync(
            "002415", DateTime.Now.Subtract(TimeSpan.FromDays(30)), DateTime.Now, TimeSpan.FromDays(1));
        Assert.That(kLines, Is.Not.Empty);
    }
}