using System;
using Avalonia;
using Avalonia.Controls;

namespace StockCopilot.Attaches;

public static class FocusAttach
{
    public static readonly AttachedProperty<bool> AutoFocusProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("AutoFocus", typeof(FocusAttach), false, true);

    static FocusAttach()
    {
        AutoFocusProperty.Changed.Subscribe(OnAutoFocusChanged);
    }

    public static void SetAutoFocus(AvaloniaObject element, bool value)
    {
        element.SetValue(AutoFocusProperty, value);
    }

    public static bool GetAutoFocus(AvaloniaObject element)
    {
        return element.GetValue(AutoFocusProperty);
    }

    private static void OnAutoFocusChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e is { NewValue: true, Sender: Control control })
        {
            control.AttachedToVisualTree += (_, _) => control.Focus();
        }
    }
}