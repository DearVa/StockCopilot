using System.Runtime.Versioning;
using Microsoft.Win32;

namespace StockCopilot.Core.Internals;

[SupportedOSPlatform("windows")]
internal static class EasyMoneyHelper
{
    public static string InstallPath { get; } = 
        FindEastMoneyInstallPath() ?? throw new DirectoryNotFoundException("未找到东方财富安装目录");

    private static string? FindEastMoneyInstallPath()
    {
        var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Eastmoney");
        var path = reg?.GetValue("UninstallString")?.ToString();
        if (path == null) return null;
        return Path.GetDirectoryName(path);
    }
}