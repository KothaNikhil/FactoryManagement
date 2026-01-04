using CommunityToolkit.Mvvm.ComponentModel;
using FactoryManagement.Models;

namespace FactoryManagement.ViewModels
{
    /// <summary>
    /// ViewModel for a single processing output item in the UI
    /// </summary>
    public partial class ProcessingOutputItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private Item? _selectedItem;

        [ObservableProperty]
        private decimal _quantity;

        [ObservableProperty]
        private string _unit = string.Empty;

        partial void OnSelectedItemChanged(Item? value)
        {
            if (value != null)
            {
                Unit = value.Unit;
            }
        }

        partial void OnQuantityChanged(decimal value)
        {
            // Notify parent to recalculate total
        }
    }
}
