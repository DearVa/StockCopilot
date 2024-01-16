using Avalonia.Controls.Primitives;
using StockCopilot.Core;
using StockCopilot.ViewModels;

namespace StockCopilot.Views;

public class StockComparisionView : TemplatedControl
{
    public StockComparisionView()
    {
        DataContext = ServiceLocator.Resolve<StockComparisionViewModel>();
    }
}