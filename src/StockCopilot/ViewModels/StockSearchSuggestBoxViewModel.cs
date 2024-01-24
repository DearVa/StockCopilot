using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Material.Styles.Controls;
using StockCopilot.Abstractions.Interfaces;
using StockCopilot.Abstractions.Models;
using StockCopilot.Assets;

namespace StockCopilot.ViewModels;

public partial class StockSearchSuggestBoxViewModel(IStockSearchSuggestProvider stockSearchSuggestProvider)
    : ObservableObject
{
    public string? SearchText
    {
        get => searchText;
        set
        {
            if (!SetProperty(ref searchText, value)) return;
            _ = DelayUpdateSearchSuggests(TimeSpan.FromMilliseconds(500));
        }
    }

    private string? searchText;

    public ObservableCollection<Stock> SearchSuggests { get; } = [];

    [ObservableProperty] private Stock? selectedSearchSuggest;

    private CancellationTokenSource? cancellationTokenSource;

    private async Task DelayUpdateSearchSuggests(TimeSpan delay)
    {
        try
        {
            if (cancellationTokenSource != null)
            {
                await cancellationTokenSource.CancelAsync();
            }

            cancellationTokenSource = new CancellationTokenSource();
            await Task.Delay(delay, cancellationTokenSource.Token);

            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    SearchSuggests.Clear();
                    return;
                }

                var searchSuggests = await stockSearchSuggestProvider.GetSearchSuggestsAsync(SearchText, 4);
                SearchSuggests.Clear();
                SearchSuggests.AddRange(searchSuggests);
            }
            finally
            {
                cancellationTokenSource = null;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception e)
        {
            SnackbarHost.Post(
                string.Format(Strings.StockSearchSuggestBoxViewModel_UpdateStockSearchSuggest_Fail, e.Message),
                null, DispatcherPriority.Default);
        }
    }
}