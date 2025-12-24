using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FactoryManagement.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EditModeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditMode)
            {
                return isEditMode ? "Edit Item" : "Add New Item";
            }
            return "Add New Item";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TransactionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "Buy";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string options)
            {
                var parts = options.Split('|');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            var enumValue = value.ToString();
            var targetValue = parameter.ToString();

            return enumValue?.Equals(targetValue, StringComparison.OrdinalIgnoreCase) == true
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WageTransactionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            
            var typeStr = value.ToString();
            return typeStr switch
            {
                "DailyWage" => "Wage Payment",
                "HourlyWage" => "Wage Payment",
                "MonthlyWage" => "Wage Payment",
                "AdvanceGiven" => "Advance Given",
                "AdvanceAdjustment" => "Advance Returned",
                _ => typeStr ?? string.Empty
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StockBarWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] != null && values[1] is double containerWidth)
            {
                // Handle both decimal and double types for stock value
                double stock = 0;
                if (values[0] is decimal decimalValue)
                    stock = (double)decimalValue;
                else if (values[0] is double doubleValue)
                    stock = doubleValue;
                else if (values[0] is int intValue)
                    stock = intValue;
                
                // Maximum stock value for reference (adjust based on your needs)
                double maxStock = 200;
                double percentage = Math.Min(stock / maxStock, 1.0);
                return containerWidth * percentage;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StockLevelColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 1 && values[0] != null)
            {
                // Handle both decimal and double types
                decimal stock = 0;
                if (values[0] is decimal decimalValue)
                    stock = decimalValue;
                else if (values[0] is double doubleValue)
                    stock = (decimal)doubleValue;
                else if (values[0] is int intValue)
                    stock = intValue;
                
                // Color coding: Red (< 50), Orange (50-100), Blue (> 100)
                if (stock < 50)
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(184, 84, 80)); // Red
                else if (stock < 100)
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(217, 160, 91)); // Orange
                else
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 144, 200)); // Blue
            }
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 144, 200));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StockToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double width && values[1] is double height)
            {
                return new Rect(0, 0, width, height);
            }
            return new Rect(0, 0, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Transaction Bar Height Converters
    public class TransactionBarHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 3)
            {
                decimal currentValue = values[0] is decimal d1 ? d1 : 0;
                decimal value2 = values[1] is decimal d2 ? d2 : 0;
                decimal value3 = values[2] is decimal d3 ? d3 : 0;
                
                decimal maxValue = Math.Max(Math.Max(currentValue, value2), value3);
                if (maxValue == 0) return 20.0;
                
                double percentage = (double)(currentValue / maxValue);
                return Math.Max(percentage * 280, 20); // Max height 280, min 20
            }
            return 20.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TransactionBarHeightConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 3)
            {
                decimal currentValue = values[0] is decimal d1 ? d1 : 0;
                decimal value2 = values[1] is decimal d2 ? d2 : 0;
                decimal value3 = values[2] is decimal d3 ? d3 : 0;
                
                decimal maxValue = Math.Max(Math.Max(currentValue, value2), value3);
                if (maxValue == 0) return 20.0;
                
                double percentage = (double)(currentValue / maxValue);
                return Math.Max(percentage * 280, 20);
            }
            return 20.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TransactionBarHeightConverter3 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 3)
            {
                decimal currentValue = values[0] is decimal d1 ? d1 : 0;
                decimal value2 = values[1] is decimal d2 ? d2 : 0;
                decimal value3 = values[2] is decimal d3 ? d3 : 0;
                
                decimal maxValue = Math.Max(Math.Max(currentValue, value2), value3);
                if (maxValue == 0) return 20.0;
                
                double percentage = (double)(currentValue / maxValue);
                return Math.Max(percentage * 280, 20);
            }
            return 20.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Loan Bar Height Converters
    public class LoanBarHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                decimal currentValue = values[0] is decimal d1 ? d1 : 0;
                decimal otherValue = values[1] is decimal d2 ? d2 : 0;
                
                decimal maxValue = Math.Max(currentValue, otherValue);
                if (maxValue == 0) return 20.0;
                
                double percentage = (double)(currentValue / maxValue);
                return Math.Max(percentage * 280, 20);
            }
            return 20.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LoanBarHeightConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                decimal currentValue = values[0] is decimal d1 ? d1 : 0;
                decimal otherValue = values[1] is decimal d2 ? d2 : 0;
                
                decimal maxValue = Math.Max(currentValue, otherValue);
                if (maxValue == 0) return 20.0;
                
                double percentage = (double)(currentValue / maxValue);
                return Math.Max(percentage * 280, 20);
            }
            return 20.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Wage Bar Height Converters
    public class WageBarHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                decimal currentValue = values[0] is decimal d1 ? d1 : 0;
                decimal otherValue = values[1] is decimal d2 ? d2 : 0;
                
                decimal maxValue = Math.Max(currentValue, otherValue);
                if (maxValue == 0) return 20.0;
                
                double percentage = (double)(currentValue / maxValue);
                return Math.Max(percentage * 280, 20);
            }
            return 20.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WageBarHeightConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                decimal currentValue = values[0] is decimal d1 ? d1 : 0;
                decimal otherValue = values[1] is decimal d2 ? d2 : 0;
                
                decimal maxValue = Math.Max(currentValue, otherValue);
                if (maxValue == 0) return 20.0;
                
                double percentage = (double)(currentValue / maxValue);
                return Math.Max(percentage * 280, 20);
            }
            return 20.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}