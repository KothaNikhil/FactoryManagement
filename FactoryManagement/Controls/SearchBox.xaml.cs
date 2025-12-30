using System.Windows;
using System.Windows.Controls;

namespace FactoryManagement.Controls
{
    public partial class SearchBox : UserControl
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(
                nameof(PlaceholderText),
                typeof(string),
                typeof(SearchBox),
                new PropertyMetadata("Search..."));

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public SearchBox()
        {
            InitializeComponent();
        }
    }
}
