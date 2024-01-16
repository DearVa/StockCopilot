using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;

namespace StockCopilot.Desktop;

public static partial class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_OnUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_OnUnobservedTaskException;
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void TaskScheduler_OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        MessageBox(
            IntPtr.Zero, 
            e.Exception.ToString(), 
            "Unhandled TaskScheduler Exception", 
            0x10);
    }

    private static void CurrentDomain_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox(
            IntPtr.Zero, 
            e.ExceptionObject.ToString() ?? "Unknown Exception", 
            "Unhandled Domain Exception", 
            0x10);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions())
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}