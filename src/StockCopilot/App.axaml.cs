using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
#if DEBUG
using HotAvalonia;
#endif
using Material.Styles.Themes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockCopilot.Abstractions.Interfaces;
using StockCopilot.Core;
using StockCopilot.Core.Services;
using StockCopilot.ViewModels;
using StockCopilot.Views;

namespace StockCopilot;

public class App : Application
{
    public override void Initialize()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

#if DEBUG
        this.EnableHotReload();
#endif

        AvaloniaXamlLoader.Load(this);

        ServiceLocator.ServiceCollection
            // Services
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddJsonFile("appsettings.json", true).Build())
            .AddSingleton<IKeyValueStorage, JsonKeyValueStorage>()
            .AddSingleton<IKLinesDataSource, EastMoneyKLinesDataSource>()
            .AddSingleton<IStockSearchSuggestProvider, EastMoneyStockSearchSuggestProvider>()
            // ViewModels
            .AddSingleton<StockComparisionViewModel>()
            .AddTransient<StockSearchSuggestBoxViewModel>()
            .AddSingleton<MainViewModel>()
            // Theme
            .AddSingleton<MaterialTheme>(_ => this.LocateMaterialTheme<MaterialTheme>());

        ServiceLocator.Build();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                desktop.MainWindow = new MainWindow();
                break;
            }
            case ISingleViewApplicationLifetime singleViewPlatform:
            {
                singleViewPlatform.MainView = new MainView();
                break;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}