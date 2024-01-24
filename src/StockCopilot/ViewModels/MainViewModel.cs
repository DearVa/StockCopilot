using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Material.Dialog;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using StockCopilot.Abstractions.Interfaces;
using StockCopilot.Abstractions.Models;
using StockCopilot.Models;
using StockCopilot.Views;
using StockCopilot.Views.Controls;

namespace StockCopilot.ViewModels;

public partial class MainViewModel : BusyViewModelBase
{
    private readonly IKeyValueStorage keyValueStorage;
    private readonly StockComparisionViewModel stockComparisionViewModel;
    private readonly MaterialTheme materialTheme;

    public MainViewModel(IKeyValueStorage keyValueStorage,
        StockComparisionViewModel stockComparisionViewModel,
        MaterialTheme materialTheme)
    {
        this.keyValueStorage = keyValueStorage;
        this.stockComparisionViewModel = stockComparisionViewModel;
        this.materialTheme = materialTheme;
        
        materialTheme.BaseTheme = keyValueStorage.Read(nameof(UseDarkMode)) is "1" ? BaseThemeMode.Dark : BaseThemeMode.Light;

        var stockComparisionCollectionJson = keyValueStorage.Read(nameof(StockComparisionCollection));
        if (stockComparisionCollectionJson != null)
        {
            try
            {
                var savedItems = (ObservableCollection<StockComparision>?)JsonSerializer.Deserialize(
                    stockComparisionCollectionJson,
                    typeof(ObservableCollection<StockComparision>),
                    StockComparisionCollectionJsonSerializerContext.Default);
                if (savedItems != null)
                {
                    StockComparisionCollection.AddRange(savedItems);
                    foreach (var stockComparision in StockComparisionCollection)
                    {
                        stockComparision.PropertyChanged += StockComparisionOnPropertyChanged;
                        stockComparision.SecondaryStocks.CollectionChanged += SecondaryStocksOnCollectionChanged;
                    }
                }
            }
            catch (JsonException) { }
            catch (InvalidOperationException) { }
        }

        StockComparisionCollection.CollectionChanged += StockComparisionCollectionOnCollectionChanged;

        void StockComparisionCollectionOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SaveStockComparisionCollection();

            if (e.NewItems != null)
            {
                foreach (StockComparision stockComparision in e.NewItems)
                {
                    stockComparision.PropertyChanged += StockComparisionOnPropertyChanged;
                    stockComparision.SecondaryStocks.CollectionChanged += SecondaryStocksOnCollectionChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (StockComparision stockComparision in e.OldItems)
                {
                    stockComparision.PropertyChanged -= StockComparisionOnPropertyChanged;
                    stockComparision.SecondaryStocks.CollectionChanged -= SecondaryStocksOnCollectionChanged;
                }
            }
        }

        void StockComparisionOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SaveStockComparisionCollection();
        }
        
        void SecondaryStocksOnCollectionChanged(object? o, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            SaveStockComparisionCollection();
        }

        void SaveStockComparisionCollection()
        {
            keyValueStorage.Save(nameof(StockComparisionCollection),
                JsonSerializer.Serialize(
                    StockComparisionCollection,
                    typeof(ObservableCollection<StockComparision>),
                    StockComparisionCollectionJsonSerializerContext.Default));
        }
    }

    public bool UseDarkMode
    {
        get => materialTheme.BaseTheme == BaseThemeMode.Dark;
        set
        {
            materialTheme.BaseTheme = value ? BaseThemeMode.Dark : BaseThemeMode.Light;
            keyValueStorage.Save(nameof(UseDarkMode), value ? "1" : "0");
        }
    }

    public ObservableCollection<StockComparision> StockComparisionCollection { get; } = [];

    public StockComparision? SelectedStockComparision
    {
        get => stockComparisionViewModel.StockComparision;
        set
        {
            if (stockComparisionViewModel.StockComparision == value) return;
            OnPropertyChanging();
            stockComparisionViewModel.StockComparision = value;
            OnPropertyChanged();
        }
    }

    [RelayCommand]
    private async Task AddTopStock()
    {
        var stock = await EditStock("添加参照股", null);
        if (stock == null) return;
        StockComparisionCollection.Add(new StockComparision(stock));
    }

    [RelayCommand]
    private async Task AddSecondaryStock()
    {
        if (SelectedStockComparision == null) return;
        var stock = await EditStock("添加对比股", null);
        if (stock == null) return;
        SelectedStockComparision.SecondaryStocks.Add(new SelectableStock(stock.Name, stock));
    }

    public static async Task<Stock?> EditStock(string header, string? code)
    {
        var stockSearchSuggestBox = new StockSearchSuggestBox(code);
        var result = await DialogHelper.CreateCustomDialog(new CustomDialogBuilderParams
        {
            ContentHeader = header,
            Content = stockSearchSuggestBox,
            Width = 400,
            Borderless = true,
            DialogButtons =
            [
                new DialogButton
                {
                    Content = "取消",
                    Result = "cancel",
                    IsNegative = true
                },
                new DialogButton
                {
                    Content = "确定",
                    Result = "ok",
                    IsPositive = true
                }
            ]
        }).ShowDialog(MainWindow.Current);

        if (result.GetResult != "ok") return null;
        if (stockSearchSuggestBox.SelectedStockSearchSuggest is not { } selectedStockSearchSuggest) return null;
        return selectedStockSearchSuggest;
    }

    [JsonSerializable(typeof(ObservableCollection<StockComparision>))]
    private partial class StockComparisionCollectionJsonSerializerContext : JsonSerializerContext;
}