using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace FactoryManagement.ViewModels
{
    public partial class ViewModelBase : ObservableObject, INotifyPropertyChanged
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;
    }
}
