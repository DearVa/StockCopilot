using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StockCopilot.Core;
using StockCopilot.ViewModels;

namespace StockCopilot;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var type = data.GetType();
        if (ServiceLocator.TryResolve(type, out var view) && view is Control control)
            return control;

        return new TextBlock { Text = type.Name };
    }

    public bool Match(object? data)
    {
        return data is BusyViewModelBase;
    }
}