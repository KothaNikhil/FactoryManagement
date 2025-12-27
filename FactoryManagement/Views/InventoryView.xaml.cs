using System.Windows.Controls;

namespace FactoryManagement.Views
{
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
        }

        private void NumericTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Clear if the value is "0" or "0.0" etc
                if (decimal.TryParse(textBox.Text, out decimal value) && value == 0)
                {
                    textBox.Text = string.Empty;
                    textBox.SelectAll();
                }
                else
                {
                    textBox.SelectAll();
                }
            }
        }
    }
}

