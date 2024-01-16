using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using DialogHostAvalonia;

namespace StockCopilot.Views;

public partial class MainWindow : Window
{
    public static MainWindow Current => current ?? throw new InvalidOperationException("MainWindow is not initialized");

    private static MainWindow? current;

    public MainWindow()
    {
        current = this;
        InitializeComponent();
    }

    public static Task<object?> ShowDialog(
        object content,
        DialogOpenedEventHandler? dialogOpenedEventHandler = null,
        DialogClosingEventHandler? dialogClosingEventHandler = null)
    {
        return DialogHost.Show(
            content,
            "MainDialogHost",
            dialogOpenedEventHandler,
            dialogClosingEventHandler);
    }
}