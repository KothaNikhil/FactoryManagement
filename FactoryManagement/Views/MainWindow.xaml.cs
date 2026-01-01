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
            Closing += MainWindow_Closing;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            // Set initial icon state
            UpdateMaximizeRestoreIcon();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Ensure app shuts down when main window closes
            Application.Current.Shutdown();
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
                    case "Workers":
                        await ViewModel.NavigateToWorkersCommand.ExecuteAsync(null);
                        break;
                    case "Backup":
                        ViewModel.NavigateToBackupCommand.Execute(null);
                        break;
                    case "Users":
                        await ViewModel.NavigateToUsersCommand.ExecuteAsync(null);
                        break;
                    case "OperationalExpenses":
                        await ViewModel.NavigateToOperationalExpensesCommand.ExecuteAsync(null);
                        break;
                    case "ExpenseCategories":
                        await ViewModel.NavigateToExpenseCategoriesCommand.ExecuteAsync(null);
                        break;
                    case "Exit":
                        ViewModel.ExitCommand.Execute(null);
                        break;
                }
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Handle Ctrl+Tab and Ctrl+Shift+Tab for menu navigation
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Tab)
            {
                // Ctrl+Tab: Navigate to next menu item
                // Ctrl+Shift+Tab: Navigate to previous menu item
                bool goToNext = (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift;
                
                int currentIndex = MenuListBox.SelectedIndex;
                int nextIndex = currentIndex;
                int itemCount = MenuListBox.Items.Count;
                
                // Find next selectable item (skip separators)
                do
                {
                    nextIndex = goToNext ? nextIndex + 1 : nextIndex - 1;
                    
                    // Wrap around: if at end, go to beginning; if at beginning, go to end
                    if (nextIndex >= itemCount)
                    {
                        nextIndex = 0;
                    }
                    else if (nextIndex < 0)
                    {
                        nextIndex = itemCount - 1;
                    }
                    
                    // Check if item is selectable (not a separator)
                    if (MenuListBox.Items[nextIndex] is ListBoxItem item && item.Tag != null)
                    {
                        MenuListBox.SelectedIndex = nextIndex;
                        break;
                    }
                    
                    // Prevent infinite loop if all items are separators
                    if (nextIndex == currentIndex)
                    {
                        break;
                    }
                    
                } while (true);
                
                e.Handled = true;
                return;
            }
            
            // Handle Ctrl+Number keyboard shortcuts for navigation
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                string? tag = null;
                
                // Handle Ctrl+S to save (relay to current view if it supports save)
                if (e.Key == Key.S)
                {
                    // Try to find and trigger save command on current DataContext (MainWindow's DataContext)
                    if (this.DataContext is ViewModels.ViewModelBase viewModel)
                    {
                        // Most ViewModels have a SaveCommand or SaveTransactionCommand
                        var saveProp = viewModel.GetType().GetProperty("SaveCommand") 
                            ?? viewModel.GetType().GetProperty("SaveTransactionCommand");
                        
                        if (saveProp != null)
                        {
                            var saveCommand = saveProp.GetValue(viewModel);
                            if (saveCommand is System.Windows.Input.ICommand command && command.CanExecute(null))
                            {
                                command.Execute(null);
                                e.Handled = true;
                                return;
                            }
                        }
                    }
                }
                
                // Handle Ctrl+N for New
                if (e.Key == Key.N)
                {
                    // This would navigate to a form or clear current form
                    // For now, this is handled by individual views
                    e.Handled = false;
                    return;
                }
                
                switch (e.Key)
                {
                    case Key.D:
                        tag = "Dashboard";
                        break;
                    case Key.T:
                        tag = "TransactionEntry";
                        break;
                    case Key.P:
                        tag = "Wages";
                        break;
                    case Key.F:
                        tag = "FinancialTransactions";
                        break;
                    case Key.R:
                        tag = "Reports";
                        break;
                    case Key.I:
                        tag = "Items";
                        break;
                    case Key.C:
                        tag = "Parties";
                        break;
                    case Key.U:
                        tag = "Users";
                        break;
                    case Key.B:
                        tag = "Backup";
                        break;
                    case Key.W:
                        tag = "Workers";
                        break;
                    case Key.O:
                        tag = "OperationalExpenses";
                        break;
                }
                
                if (tag != null)
                {
                    // Find and select the menu item by tag
                    foreach (var item in MenuListBox.Items)
                    {
                        if (item is ListBoxItem listBoxItem && listBoxItem.Tag?.ToString() == tag)
                        {
                            MenuListBox.SelectedItem = listBoxItem;
                            e.Handled = true;
                            break;
                        }
                    }
                }
            }
        }    }
}