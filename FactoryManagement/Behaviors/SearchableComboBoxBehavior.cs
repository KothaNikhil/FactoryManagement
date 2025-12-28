using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;

namespace FactoryManagement.Behaviors
{
    /// <summary>
    /// Behavior that enables type-to-search functionality in ComboBox.
    /// Filters items based on typed characters and displays matching suggestions.
    /// </summary>
    public class SearchableComboBoxBehavior : Behavior<ComboBox>
    {
        private IEnumerable? _originalItemsSource;
        private ObservableCollection<object>? _filteredItems;
        private string _searchText = string.Empty;
        private TextBox? _editableTextBox;

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                nameof(DisplayMemberPath),
                typeof(string),
                typeof(SearchableComboBoxBehavior),
                new PropertyMetadata(null));

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsEditable = true;
            AssociatedObject.StaysOpenOnEdit = true;
            AssociatedObject.IsTextSearchEnabled = false;
            AssociatedObject.PreviewKeyDown += ComboBox_PreviewKeyDown;
            AssociatedObject.PreviewTextInput += ComboBox_PreviewTextInput;
            AssociatedObject.Loaded += ComboBox_Loaded;
            AssociatedObject.GotFocus += ComboBox_GotFocus;
            AssociatedObject.DropDownOpened += ComboBox_DropDownOpened;
            AssociatedObject.PreviewMouseDown += ComboBox_PreviewMouseDown;
            AssociatedObject.SelectionChanged += ComboBox_SelectionChanged;
            // Capture editable TextBox after template loads
            AssociatedObject.Dispatcher.BeginInvoke((Action)(() => EnsureEditableTextBox()), DispatcherPriority.Loaded);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= ComboBox_PreviewKeyDown;
            AssociatedObject.PreviewTextInput -= ComboBox_PreviewTextInput;
            AssociatedObject.Loaded -= ComboBox_Loaded;
            AssociatedObject.GotFocus -= ComboBox_GotFocus;
            AssociatedObject.DropDownOpened -= ComboBox_DropDownOpened;
            AssociatedObject.PreviewMouseDown -= ComboBox_PreviewMouseDown;
            AssociatedObject.SelectionChanged -= ComboBox_SelectionChanged;
            AssociatedObject.PreviewMouseDown -= ComboBox_PreviewMouseDown;
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            // Store the original items source
            if (_originalItemsSource == null)
            {
                _originalItemsSource = AssociatedObject.ItemsSource;
                _filteredItems = new ObservableCollection<object>();
                _searchText = string.Empty;
                EnsureEditableTextBox();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When selection changes, update the text to show selected item (only if not actively searching)
            if (string.IsNullOrEmpty(_searchText) && AssociatedObject.SelectedItem != null)
            {
                var displayText = GetDisplayText(AssociatedObject.SelectedItem);
                if (AssociatedObject.Text != displayText)
                {
                    AssociatedObject.Text = displayText;
                }
            }
        }

        private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // When ComboBox gets focus, show all items if no active search
            if (string.IsNullOrEmpty(_searchText))
            {
                RestoreOriginalItems();
                // Open dropdown to show all options
                AssociatedObject.IsDropDownOpen = true;
            }
        }

        private void ComboBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // When clicking the ComboBox, ensure all items are shown if no search is active
            if (string.IsNullOrEmpty(_searchText))
            {
                RestoreOriginalItems();
            }
        }

        private void ComboBox_DropDownOpened(object? sender, EventArgs e)
        {
            // When dropdown opens, always ensure items are shown
            if (string.IsNullOrEmpty(_searchText))
            {
                RestoreOriginalItems();
            }
        }

        private void ComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _searchText += e.Text;
            AssociatedObject.Text = _searchText;
            SetCaretToEnd();
            FilterItems(_searchText);
            
            // Open dropdown to show filtered results
            AssociatedObject.IsDropDownOpen = true;
            e.Handled = true;
        }

        private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Handle Backspace to remove last character
            if (e.Key == Key.Back)
            {
                if (_searchText.Length > 0)
                {
                    _searchText = _searchText.Substring(0, _searchText.Length - 1);
                }
                
                AssociatedObject.Text = _searchText;
                SetCaretToEnd();
                
                if (_searchText.Length == 0)
                {
                    RestoreOriginalItems();
                }
                else
                {
                    FilterItems(_searchText);
                }
                
                AssociatedObject.IsDropDownOpen = true;
                e.Handled = true;
            }
            // Handle Escape to cancel search and restore original items
            else if (e.Key == Key.Escape)
            {
                _searchText = string.Empty;
                AssociatedObject.Text = string.Empty;
                SetCaretToEnd();
                RestoreOriginalItems();
                AssociatedObject.IsDropDownOpen = false;
                e.Handled = true;
            }
            // Handle Enter or Tab to confirm selection and close dropdown
            else if (e.Key == Key.Return || e.Key == Key.Tab)
            {
                _searchText = string.Empty;
                AssociatedObject.Text = string.Empty;
                SetCaretToEnd();
                RestoreOriginalItems();
                AssociatedObject.IsDropDownOpen = false;
                e.Handled = e.Key == Key.Return;
            }
            // Handle Down arrow to navigate filtered results
            else if (e.Key == Key.Down || e.Key == Key.Up)
            {
                AssociatedObject.IsDropDownOpen = true;
                // Let default behavior handle arrow key navigation
                e.Handled = false;
            }
        }

        private void FilterItems(string searchText)
        {
            if (_originalItemsSource == null)
                return;

            if (_filteredItems == null)
                _filteredItems = new ObservableCollection<object>();

            _filteredItems.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                RestoreOriginalItems();
                return;
            }

            var searchLower = searchText.ToLower();
            var filtered = new ObservableCollection<object>();

            foreach (var item in _originalItemsSource)
            {
                string displayText = GetDisplayText(item);
                if (displayText.ToLower().StartsWith(searchLower))
                {
                    filtered.Add(item);
                }
            }

            AssociatedObject.ItemsSource = filtered;
        }

        private void RestoreOriginalItems()
        {
            if (_originalItemsSource != null)
            {
                AssociatedObject.ItemsSource = _originalItemsSource;
            }
        }

        private void EnsureEditableTextBox()
        {
            _editableTextBox = AssociatedObject.Template?.FindName("PART_EditableTextBox", AssociatedObject) as TextBox;
        }

        private void SetCaretToEnd()
        {
            if (_editableTextBox == null)
                EnsureEditableTextBox();

            if (_editableTextBox != null)
            {
                // Defer caret update to ensure TextBox reflects latest text
                _editableTextBox.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _editableTextBox.CaretIndex = _searchText.Length;
                    _editableTextBox.Select(_searchText.Length, 0);
                }), DispatcherPriority.Background);
            }
        }

        private string GetDisplayText(object item)
        {
            if (item == null)
                return string.Empty;

            // If DisplayMemberPath is specified, use reflection to get the property value
            if (!string.IsNullOrEmpty(DisplayMemberPath))
            {
                try
                {
                    var property = item.GetType().GetProperty(DisplayMemberPath);
                    if (property != null)
                    {
                        var value = property.GetValue(item);
                        return value?.ToString() ?? string.Empty;
                    }
                }
                catch
                {
                    // If reflection fails, fall back to ToString
                }
            }

            // If ItemTemplate is used, we need to check the ComboBox's ItemTemplate
            // For now, try the common display properties
            var nameProperty = item.GetType().GetProperty("Name") 
                ?? item.GetType().GetProperty("ItemName")
                ?? item.GetType().GetProperty("Title");

            if (nameProperty != null)
            {
                var value = nameProperty.GetValue(item);
                return value?.ToString() ?? string.Empty;
            }

            return item.ToString() ?? string.Empty;
        }
    }
}
