using CommunityToolkit.Mvvm.ComponentModel;

namespace StockCopilot.Models;

public abstract class SelectableItem<TData>(string name, TData data) : ObservableObject
{
    public string Name { get; init; } = name;
    
    public TData Data { get; init; } = data;
    
    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    private bool isSelected;

    public override string ToString() => Name;
}