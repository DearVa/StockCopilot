using CommunityToolkit.Mvvm.ComponentModel;

namespace StockCopilot.Models;

public abstract partial class SelectableItem<TData>(string name, TData data) : ObservableObject
{
    public string Name { get; init; } = name;
    
    public TData Data { get; init; } = data;

    [ObservableProperty] private bool isSelected;

    public override string ToString() => Name;
}