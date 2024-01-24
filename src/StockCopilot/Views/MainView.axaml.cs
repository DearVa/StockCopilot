using Avalonia.Controls;
using DynamicData;
using StockCopilot.Core;
using StockCopilot.Models;
using StockCopilot.ViewModels;

namespace StockCopilot.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        DataContext = ServiceLocator.Resolve<MainViewModel>();
        InitializeComponent();
    }

    public async void EditStock(object? obj)
    {
        if (DataContext is not MainViewModel { SelectedStockComparision: { } selectedStockComparision }) return;
        switch (obj)
        {
            case StockComparision stockComparision:
            {
                var newStock = await MainViewModel.EditStock("编辑参照股", stockComparision.TopStock.Code);
                if (newStock == null) return;
                stockComparision.TopStock = newStock;
                break;
            }
            case SelectableStock stock:
            {
                var newStock = await MainViewModel.EditStock("编辑对比股", stock.Data.Code);
                if (newStock == null) return;
                selectedStockComparision.SecondaryStocks.Replace(stock, new SelectableStock(newStock.Name, newStock));
                break;
            }
        }
    }

    public void RemoveStock(object? obj)
    {
        if (DataContext is not MainViewModel viewModel) return;
        switch (obj)
        {
            case StockComparision stockComparision:
            {
                viewModel.StockComparisionCollection.Remove(stockComparision);
                break;
            }
            case SelectableStock stock when viewModel.SelectedStockComparision != null:
            {
                viewModel.SelectedStockComparision.SecondaryStocks.Remove(stock);
                break;
            }
        }
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ServiceLocator.Resolve<StockComparisionViewModel>().UpdatePlot();
    }
}