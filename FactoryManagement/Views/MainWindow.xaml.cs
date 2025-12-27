using FactoryManagement.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FactoryManagement.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (s, e) => await viewModel.InitializeAsync();
            StateChanged += MainWindow_StateChanged;
            // Set initial icon state
            UpdateMaximizeRestoreIcon();
        }

        private void MainWindow_StateChanged(object? sender, System.EventArgs e)
        {
            UpdateMaximizeRestoreIcon();
        }

        private void UpdateMaximizeRestoreIcon()
        {
            if (WindowState == WindowState.Maximized)
            {
                MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
            }
            else
            {
                MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
            }
        }

        private void NavigationBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            // Expand menu on hover when unpinned
            if (ViewModel.IsMenuPinned == false)
            {
                ViewModel.IsMenuExpanded = true;
            }
        }

        private void NavigationBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            // Collapse menu on mouse leave when unpinned
            if (ViewModel.IsMenuPinned == false)
            {
                ViewModel.IsMenuExpanded = false;
            }
        }

        

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle menu pin state via pin icon
            ViewModel.IsMenuPinned = !ViewModel.IsMenuPinned;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else if (WindowState != WindowState.Maximized)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return; // Prevent null reference during initialization
            
            if (MenuListBox.SelectedItem is ListBoxItem item && item.Tag is string tag)
            {
                switch (tag)
                {
                    case "Dashboard":
                        await ViewModel.NavigateToDashboardCommand.ExecuteAsync(null);
                        break;
                    case "TransactionEntry":
                        await ViewModel.NavigateToTransactionEntryCommand.ExecuteAsync(null);
                        break;
                    case "Reports":
                        await ViewModel.NavigateToReportsCommand.ExecuteAsync(null);
                        break;
                    case "Items":
                        await ViewModel.NavigateToItemsCommand.ExecuteAsync(null);
                        break;
                    case "Parties":
                        await ViewModel.NavigateToPartiesCommand.ExecuteAsync(null);
                        break;
                    case "FinancialTransactions":
                        await ViewModel.NavigateToFinancialTransactionsCommand.ExecuteAsync(null);
                        break;
                    case "Wages":
                        await ViewModel.NavigateToWagesCommand.ExecuteAsync(null);
                        break;
                    case "Backup":
                        ViewModel.NavigateToBackupCommand.Execute(null);
                        break;
                    case "Users":
                        await ViewModel.NavigateToUsersCommand.ExecuteAsync(null);
                        break;
                    case "Exit":
                        ViewModel.ExitCommand.Execute(null);
                        break;
                }
            }
        }
    }
}
