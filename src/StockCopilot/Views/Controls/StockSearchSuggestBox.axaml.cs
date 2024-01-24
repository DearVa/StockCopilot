using Avalonia.Controls.Primitives;
using StockCopilot.Abstractions.Models;
using StockCopilot.Core;
using StockCopilot.ViewModels;

namespace StockCopilot.Views.Controls;

public class StockSearchSuggestBox : TemplatedControl
{
    public Stock? SelectedStockSearchSuggest => viewModel.SelectedSearchSuggest;

    private readonly StockSearchSuggestBoxViewModel viewModel;
    
    public StockSearchSuggestBox(string? searchText = null)
    {
        DataContext = viewModel = ServiceLocator.Resolve<StockSearchSuggestBoxViewModel>();
        viewModel.SearchText = searchText;
    }
}