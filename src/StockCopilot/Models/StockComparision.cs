using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using StockCopilot.Abstractions.Models;

namespace StockCopilot.Models;

public class StockComparision(Stock topStock) : ObservableObject
{
    public Stock TopStock
    {
        get => topStock;
        set => SetProperty(ref topStock, value);
    }

    public ObservableCollection<SelectableStock> SecondaryStocks { get; init; } = [];

    public static SelectableKLineInterval[] KLineIntervals { get; } =
    [
        new SelectableKLineInterval("日K", TimeSpan.FromDays(1)),
        new SelectableKLineInterval("周K", TimeSpan.FromDays(7)),
        new SelectableKLineInterval("月K", TimeSpan.FromDays(30)),
        new SelectableKLineInterval("5分钟", TimeSpan.FromMinutes(5)),
        new SelectableKLineInterval("15分钟", TimeSpan.FromMinutes(15)),
        new SelectableKLineInterval("30分钟", TimeSpan.FromMinutes(30)),
        new SelectableKLineInterval("60分钟", TimeSpan.FromMinutes(60))
    ];

    public int SelectedKLineIntervalIndex
    {
        get => selectedKLineIntervalIndex;
        set => SetProperty(ref selectedKLineIntervalIndex, value);
    }

    private int selectedKLineIntervalIndex;

    [JsonIgnore]
    public TimeSpan SelectedKLineInterval =>
        SelectedKLineIntervalIndex >= 0 && SelectedKLineIntervalIndex < SecondaryStocks.Count
            ? KLineIntervals[SelectedKLineIntervalIndex].Data
            : TimeSpan.FromDays(1);

    private static DateTimeOffset ValidateDateTime(DateTimeOffset dateTimeOffset)
    {
        if (dateTimeOffset < new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
        {
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        }
        if (dateTimeOffset > DateTimeOffset.Now.Add(TimeSpan.FromDays(365)))
        {
            return DateTimeOffset.Now.Add(TimeSpan.FromDays(365));
        }
        return dateTimeOffset;
    }

    public DateTimeOffset BeginDateTime
    {
        get => beginDateTime;
        set => SetProperty(ref beginDateTime, ValidateDateTime(value));
    }

    private DateTimeOffset beginDateTime = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
    
    public DateTimeOffset EndDateTime
    {
        get => endDateTime;
        set => SetProperty(ref endDateTime, ValidateDateTime(value));
    }

    private DateTimeOffset endDateTime = DateTimeOffset.Now.Add(TimeSpan.FromDays(365));
}