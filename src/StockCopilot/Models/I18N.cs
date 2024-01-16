using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using StockCopilot.Assets;

namespace StockCopilot.Models;

public static class I18N
{
    public static __Provider Provider { get; } = new();
    
    public sealed class __Provider : INotifyPropertyChanged
    {
        public string StockSearchSuggestBox_Search => Strings.StockSearchSuggestBox_Search;
        
        public CultureInfo Culture
        {
            get => Strings.Culture;
            set
            {
                Strings.Culture = value;
                OnPropertyChanged(nameof(StockSearchSuggestBox_Search));
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}