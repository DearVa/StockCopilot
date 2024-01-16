using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Styles.Controls;
using NPOI.OpenXmlFormats;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.Model;
using NPOI.XSSF.UserModel;
using StockCopilot.Abstractions.Interfaces;
using StockCopilot.Models;
using StockCopilot.Views;

namespace StockCopilot.ViewModels;

public partial class StockComparisionViewModel(IKLinesDataSource kLinesDataSource) : BusyViewModelBase
{
    public StockComparision? StockComparision
    {
        get => stockComparision;
        set
        {
            if (stockComparision == value) return;
            if (stockComparision != null)
            {
                stockComparision.PropertyChanged -= StockComparision_OnPropertyChanged;
            }
            if (value != null)
            {
                value.PropertyChanged += StockComparision_OnPropertyChanged;
            }
            OnPropertyChanging();
            stockComparision = value;
            OnPropertyChanged();
            UpdatePlot();
        }
    }

    private StockComparision? stockComparision;

    public ObservableCollection<KLineBarItem> KLineBarItems { get; } = [];

    public static StockDataRetriever[] StockDataRetrievers { get; } =
    [
        new StockDataRetriever("开盘", k => k.Opening),
        new StockDataRetriever("收盘", k => k.Closing),
        new StockDataRetriever("最高", k => k.Highest),
        new StockDataRetriever("最低", k => k.Lowest),
        new StockDataRetriever("成交量", k => k.Volume),
        new StockDataRetriever("成交额", k => k.Turnover),
        new StockDataRetriever("振幅", k => k.Amplitude),
        new StockDataRetriever("涨跌幅", k => k.PriceChangePercentage),
        new StockDataRetriever("涨跌额", k => k.PriceChangeAmount),
        new StockDataRetriever("换手率", k => k.TurnoverRate)
    ];

    [ObservableProperty] private int selectedStockDataRetrieverIndex = 7;

    public static StockComparisionMode[] StockComparisionModes { get; } =
    [
        new StockComparisionMode("绝对值", (x, y) => x - y),
        new StockComparisionMode("不同走势", (x, y) => Math.Sign(x) == Math.Sign(y) ? 0 : x - y),
        new StockComparisionMode("不同走势（红）", (x, y) => x >= 0 && y <= 0 ? x - y : 0),
        new StockComparisionMode("不同走势（绿）", (x, y) => x <= 0 && y >= 0 ? x - y : 0),
    ];

    [ObservableProperty] private int selectedStockComparisionModeIndex = 2;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(SelectedStockDataRetrieverIndex):
            case nameof(SelectedStockComparisionModeIndex):
            {
                if (KLineBarItems.Count == 0) break;

                var dataRetriever = StockDataRetrievers[Math.Clamp(
                    SelectedStockDataRetrieverIndex, 0, StockDataRetrievers.Length - 1)];
                var comparisionMode = StockComparisionModes[Math.Clamp(
                    SelectedStockComparisionModeIndex, 0, StockComparisionModes.Length - 1)];
                foreach (var kLineBarItem in KLineBarItems.Where(i => i.SecondaryKLine != null))
                {
                    kLineBarItem.Value = (double)comparisionMode.Data(
                        dataRetriever.Data(kLineBarItem.TopKLine),
                        dataRetriever.Data(kLineBarItem.SecondaryKLine!));
                }

                // Raise KLineBarItems Update
                KLineBarItems.Move(KLineBarItems.Count - 1, KLineBarItems.Count - 1);
                break;
            }
        }
    }

    private void StockComparision_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdatePlot();
    }

    private static string GetDateTimeFormatString(TimeSpan interval)
    {
        return (long)interval.TotalMinutes switch
        {
            < 60L * 24 => "yyyy-MM-dd HH:mm",
            >= 60L * 24 * 30 => "yyyy-MM",
            _ => "yyyy-MM-dd"
        };
    }

    private async void UpdatePlot()
    {
        if (StockComparision == null) return;

        var code1 = StockComparision.TopStock.Code;
        var code2 = StockComparision.SelectedSecondaryStock?.Code;

        await ExecuteBusyAction(() => UpdateAllPlotsInternal(
            code1,
            code2,
            StockComparision.BeginDateTime.LocalDateTime,
            StockComparision.EndDateTime.LocalDateTime,
            StockComparision.SelectedKLineInterval));

        async Task UpdateAllPlotsInternal(string topCode, string? secondaryCode, DateTime begin, DateTime end,
            TimeSpan interval)
        {
            try
            {
                KLineBarItems.Clear();

                var topKLines = await kLinesDataSource.GetKLinesAsync(
                    topCode, begin, end, interval);

                var dateTimeFormatString = GetDateTimeFormatString(interval);

                if (secondaryCode != null)
                {
                    var secondaryKLines = await kLinesDataSource.GetKLinesAsync(
                        secondaryCode, begin, end, interval);

                    var dataRetriever = StockDataRetrievers[Math.Clamp(
                        SelectedStockDataRetrieverIndex, 0, StockDataRetrievers.Length - 1)];
                    var comparisionMode = StockComparisionModes[Math.Clamp(
                        SelectedStockComparisionModeIndex, 0, StockComparisionModes.Length - 1)];

                    var i = 0;
                    foreach (var topKLine in topKLines)
                    {
                        while (i < secondaryKLines.Count && secondaryKLines[i].DateTime < topKLine.DateTime)
                        {
                            i++;
                        }

                        if (i >= secondaryKLines.Count || secondaryKLines[i].DateTime > topKLine.DateTime)
                        {
                            KLineBarItems.Add(new KLineBarItem(topKLine, null, dateTimeFormatString));
                        }
                        else
                        {
                            KLineBarItems.Add(new KLineBarItem(topKLine, secondaryKLines[i], dateTimeFormatString)
                            {
                                Value = (double)comparisionMode.Data(
                                    dataRetriever.Data(secondaryKLines[i]),
                                    dataRetriever.Data(topKLine))
                            });
                        }
                    }
                }
                else
                {
                    foreach (var topKLine in topKLines)
                    {
                        KLineBarItems.Add(new KLineBarItem(topKLine, null, dateTimeFormatString));
                    }
                }
            }
            catch (Exception e)
            {
                SnackbarHost.Post(
                    $"获取K线数据失败：{e.Message}",
                    null, DispatcherPriority.Default);
            }
        }
    }

    [RelayCommand]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SharedStringsTable))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(StylesTable))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(List<CT_Property>))]
    private async Task ExportData()
    {
        if (StockComparision?.SelectedSecondaryStock == null) return;

        var wb = new XSSFWorkbook();
        var sheet = wb.CreateSheet();

        var title =
            $"{StockComparision.TopStock} 与{StockComparision.SelectedSecondaryStock} {StockDataRetrievers[SelectedStockDataRetrieverIndex].Name}对比表（{StockComparision.KLineIntervals[StockComparision.SelectedKLineIntervalIndex].Name}）";
        var titleRow = sheet.CreateRow(0);
        titleRow.CreateCell(0).SetCellValue(title);
        var titleStyle = wb.CreateCellStyle();
        var titleFont = wb.CreateFont();
        titleFont.FontHeightInPoints = 16;
        titleStyle.Alignment = HorizontalAlignment.Center;
        titleStyle.SetFont(titleFont);
        titleRow.GetCell(0).CellStyle = titleStyle;
        sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

        var headerRow = sheet.CreateRow(1);
        headerRow.CreateCell(0).SetCellValue("日期");
        headerRow.CreateCell(1).SetCellValue(StockComparision.TopStock.Name);
        headerRow.CreateCell(2).SetCellValue(StockComparision.SelectedSecondaryStock.Name);
        headerRow.CreateCell(3).SetCellValue("差值");
        headerRow.CreateCell(4).SetCellValue("参照股走势");
        headerRow.CreateCell(5).SetCellValue("对比股走势");
        headerRow.CreateCell(6).SetCellValue("走势一致性");
        sheet.SetAutoFilter(new CellRangeAddress(1, 1, 0, 6));
        
        sheet.CreateFreezePane(0, 2);

        var rowNumber = 2;
        var dateTimeFormatString = GetDateTimeFormatString(StockComparision.SelectedKLineInterval);
        var dataRetriever = StockDataRetrievers[Math.Clamp(
            SelectedStockDataRetrieverIndex, 0, StockDataRetrievers.Length - 1)];
        foreach (var kLineBarItem in KLineBarItems)
        {
            var row = sheet.CreateRow(rowNumber++);
            row.CreateCell(0).SetCellValue(kLineBarItem.DateTime.ToString(dateTimeFormatString));

            row.CreateCell(1).SetCellValue((double)dataRetriever.Data(kLineBarItem.TopKLine));
            row.CreateCell(2).SetCellValue(
                kLineBarItem.SecondaryKLine == null
                    ? double.NaN
                    : (double)dataRetriever.Data(kLineBarItem.SecondaryKLine));
            row.CreateCell(3).SetCellFormula($"C{rowNumber}-B{rowNumber}");
            
            row.CreateCell(4).SetCellFormula($"IF(SIGN(B{rowNumber})>0, \"上涨\", IF(SIGN(B{rowNumber})<0, \"下跌\", \"平盘\"))");
            row.CreateCell(5).SetCellFormula($"IF(SIGN(C{rowNumber})>0, \"上涨\", IF(SIGN(C{rowNumber})<0, \"下跌\", \"平盘\"))");
            row.CreateCell(6).SetCellFormula($"IF(SIGN(B{rowNumber})=SIGN(C{rowNumber}), \"同\", \"异\")");
        }

        var columnWidths = new[] { 20, 12, 12, 12, 12, 12, 12 };
        for (var i = 0; i < 7; i++)
        {
            sheet.SetColumnWidth(i, columnWidths[i] * 256);
        }

        try
        {
            var storageFile = await MainWindow.Current.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    DefaultExtension = ".xlsx",
                    FileTypeChoices =
                    [
                        new FilePickerFileType("Excel 工作簿")
                        {
                            Patterns = [".xlsx"]
                        }
                    ],
                    ShowOverwritePrompt = true,
                    SuggestedFileName = title,
                    Title = "选择保存路径"
                });
            if (storageFile == null) return;

            await using var fs = await storageFile.OpenWriteAsync();
            wb.Write(fs);
        }
        catch (Exception e)
        {
            throw;
        }
    }
}