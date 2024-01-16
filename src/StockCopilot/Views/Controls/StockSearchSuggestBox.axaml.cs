using Avalonia.Controls.Primitives;
using StockCopilot.Abstractions.Models;
using StockCopilot.Core;
using StockCopilot.ViewModels;

namespace StockCopilot.Views.Controls;

public class StockSearchSuggestBox : TemplatedControl
{
    public StockSearchSuggest? SelectedStockSearchSuggest =>
        ((StockSearchSuggestBoxViewModel?)DataContext)?.SelectedSearchSuggest;
    
    public StockSearchSuggestBox(string? searchText = null)
    {
        var viewModel = ServiceLocator.Resolve<StockSearchSuggestBoxViewModel>();
        DataContext = viewModel;
        viewModel.SearchText = searchText;
    }
}