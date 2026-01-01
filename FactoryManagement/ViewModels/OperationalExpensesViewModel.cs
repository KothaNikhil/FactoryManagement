using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.ViewModels
{
    /// <summary>
    /// ViewModel for managing operational expenses
    /// </summary>
    public partial class OperationalExpensesViewModel : PaginationViewModel
    {
        private readonly IOperationalExpenseService _expenseService;
        private readonly IExpenseCategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IUserService _userService;

        // Collections
        [ObservableProperty]
        private ObservableCollection<OperationalExpense> _allExpenses = new();

        [ObservableProperty]
        private ObservableCollection<OperationalExpense> _paginatedExpenses = new();

        [ObservableProperty]
        private ObservableCollection<ExpenseCategory> _activeCategories = new();

        [ObservableProperty]
        private ObservableCollection<User> _allUsers = new();

        [ObservableProperty]
        private ObservableCollection<Item> _allItems = new();

        // Form properties
        [ObservableProperty]
        private OperationalExpense? _selectedExpense;

        [ObservableProperty]
        private ExpenseCategory? _selectedCategory;

        [ObservableProperty]
        private User? _selectedSpentByUser;

        [ObservableProperty]
        private Item? _selectedItem;

        [ObservableProperty]
        private decimal _amount;

        [ObservableProperty]
        private DateTime _expenseDate = DateTime.Now;

        [ObservableProperty]
        private string _vendorName = string.Empty;

        [ObservableProperty]
        private PaymentMode _selectedPaymentMode = PaymentMode.Cash;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private int _editingExpenseId;

        // Filters
        [ObservableProperty]
        private DateTime? _filterStartDate;

        [ObservableProperty]
        private DateTime? _filterEndDate;

        [ObservableProperty]
        private ExpenseCategory? _filterCategory;

        [ObservableProperty]
        private PaymentMode? _filterPaymentMode;

        // Summary metrics
        [ObservableProperty]
        private decimal _totalExpenses;

        [ObservableProperty]
        private decimal _monthlyExpenses;

        [ObservableProperty]
        private decimal _todayExpenses;

        [ObservableProperty]
        private int _expenseCount;

        public int CurrentUserId { get; set; } = 1; // Will be set from MainWindow

        public Array PaymentModes => Enum.GetValues(typeof(PaymentMode));

        public OperationalExpensesViewModel(
            IOperationalExpenseService expenseService,
            IExpenseCategoryService categoryService,
            IItemService itemService,
            IUserService userService)
        {
            _expenseService = expenseService;
            _categoryService = categoryService;
            _itemService = itemService;
            _userService = userService;
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Load in parallel
                var tasks = new[]
                {
                    LoadExpensesAsync(),
                    LoadCategoriesAsync(),
                    LoadUsersAsync(),
                    LoadItemsAsync(),
                    LoadSummaryMetricsAsync()
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadExpensesAsync()
        {
            var expenses = await _expenseService.GetAllExpensesAsync();

            // Apply filters
            var filtered = expenses.AsEnumerable();

            if (FilterStartDate.HasValue)
                filtered = filtered.Where(e => e.ExpenseDate >= FilterStartDate.Value);

            if (FilterEndDate.HasValue)
                filtered = filtered.Where(e => e.ExpenseDate <= FilterEndDate.Value);

            if (FilterCategory != null)
                filtered = filtered.Where(e => e.ExpenseCategoryId == FilterCategory.ExpenseCategoryId);

            if (FilterPaymentMode.HasValue)
                filtered = filtered.Where(e => e.PaymentMode == FilterPaymentMode.Value);

            AllExpenses = new ObservableCollection<OperationalExpense>(filtered);
            ExpenseCount = AllExpenses.Count;
            UpdatePaginatedData();
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ActiveCategories = new ObservableCollection<ExpenseCategory>(categories);
        }

        private async Task LoadUsersAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            AllUsers = new ObservableCollection<User>(users.Where(u => u.IsActive));
        }

        private async Task LoadItemsAsync()
        {
            var items = await _itemService.GetAllItemsAsync();
            AllItems = new ObservableCollection<Item>(items);
        }

        private async Task LoadSummaryMetricsAsync()
        {
            TotalExpenses = await _expenseService.GetTotalExpensesAsync();
            
            var now = DateTime.Now;
            MonthlyExpenses = await _expenseService.GetMonthlyExpensesAsync(now.Year, now.Month);
            
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            TodayExpenses = await _expenseService.GetTotalExpensesByDateRangeAsync(today, tomorrow);
        }

        protected override void UpdatePaginatedData()
        {
            CalculatePagination(AllExpenses, DefaultPageSize);
            PaginatedExpenses.Clear();
            foreach (var expense in GetPagedItems(AllExpenses, DefaultPageSize))
            {
                PaginatedExpenses.Add(expense);
            }
        }

        [RelayCommand]
        private async Task SaveExpenseAsync()
        {
            if (IsBusy) return;

            // Validation
            if (SelectedCategory == null)
            {
                ErrorMessage = "Please select an expense category.";
                return;
            }

            if (Amount <= 0)
            {
                ErrorMessage = "Amount must be greater than zero.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (IsEditMode)
                {
                    // Update existing expense
                    var expense = await _expenseService.GetExpenseByIdAsync(EditingExpenseId);
                    if (expense != null)
                    {
                        UpdateExpenseFromForm(expense);
                        await _expenseService.UpdateExpenseAsync(expense);
                    }
                }
                else
                {
                    // Create new expense
                    var expense = new OperationalExpense();
                    UpdateExpenseFromForm(expense);
                    expense.EnteredBy = CurrentUserId;
                    
                    await _expenseService.CreateExpenseAsync(expense);
                }

                // Reset form and reload
                ClearForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving expense: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateExpenseFromForm(OperationalExpense expense)
        {
            expense.ExpenseCategoryId = SelectedCategory!.ExpenseCategoryId;
            expense.Amount = Amount;
            expense.ExpenseDate = ExpenseDate;
            expense.SpentBy = SelectedSpentByUser?.UserId;
            expense.VendorName = string.IsNullOrWhiteSpace(VendorName) ? null : VendorName;
            expense.ItemId = SelectedItem?.ItemId;
            expense.PaymentMode = SelectedPaymentMode;
            expense.Notes = Notes;
        }

        [RelayCommand]
        private void EditExpense(OperationalExpense? expense)
        {
            if (expense == null) return;

            IsEditMode = true;
            EditingExpenseId = expense.OperationalExpenseId;
            SelectedCategory = ActiveCategories.FirstOrDefault(c => c.ExpenseCategoryId == expense.ExpenseCategoryId);
            Amount = expense.Amount;
            ExpenseDate = expense.ExpenseDate;
            SelectedSpentByUser = AllUsers.FirstOrDefault(u => u.UserId == expense.SpentBy);
            VendorName = expense.VendorName ?? string.Empty;
            SelectedItem = AllItems.FirstOrDefault(i => i.ItemId == expense.ItemId);
            SelectedPaymentMode = expense.PaymentMode;
            Notes = expense.Notes;
            SelectedExpense = expense;
        }

        [RelayCommand]
        private async Task DeleteExpenseAsync(OperationalExpense? expense)
        {
            if (expense == null || IsBusy) return;

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete this expense of ₹{expense.Amount:N2}?",
                "Confirm Deletion",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                await _expenseService.DeleteExpenseAsync(expense.OperationalExpenseId);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting expense: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            ClearForm();
        }

        [RelayCommand]
        private void NewExpense()
        {
            ClearForm();
        }

        [RelayCommand]
        private async Task ApplyFiltersAsync()
        {
            await LoadExpensesAsync();
        }

        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            FilterStartDate = null;
            FilterEndDate = null;
            FilterCategory = null;
            FilterPaymentMode = null;
            await LoadExpensesAsync();
        }

        [RelayCommand]
        private void ClearForm()
        {
            IsEditMode = false;
            EditingExpenseId = 0;
            SelectedCategory = null;
            Amount = 0;
            ExpenseDate = DateTime.Now;
            SelectedSpentByUser = null;
            VendorName = string.Empty;
            SelectedItem = null;
            SelectedPaymentMode = PaymentMode.Cash;
            Notes = string.Empty;
            SelectedExpense = null;
        }
    }
}
