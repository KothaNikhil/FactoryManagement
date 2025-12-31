using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.ViewModels
{
    /// <summary>
    /// Helper class for UI binding of package entries
    /// </summary>
    public partial class StockPackageEntry : ObservableObject
    {
        public int StockPackageId { get; set; }

        [ObservableProperty]
        private decimal _packageSize = 25;

        [ObservableProperty]
        private int _packageCount = 1;

        [ObservableProperty]
        private string? _location;

        /// <summary>
        /// Total quantity calculated from package size and count
        /// </summary>
        [NotMapped]
        public decimal TotalQuantity => PackageSize * PackageCount;

        partial void OnPackageSizeChanged(decimal value)
        {
            OnPropertyChanged(nameof(TotalQuantity));
        }

        partial void OnPackageCountChanged(int value)
        {
            OnPropertyChanged(nameof(TotalQuantity));
        }

        public override string ToString()
        {
            return $"{PackageCount} × {PackageSize}kg";
        }
    }
}
