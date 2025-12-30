using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FactoryManagement.Controls
{
    public partial class PaginationControl : UserControl
    {
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationControl), new PropertyMetadata(1));

        public static readonly DependencyProperty TotalPagesProperty =
            DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PaginationControl), new PropertyMetadata(1));

        public static readonly DependencyProperty TotalRecordsProperty =
            DependencyProperty.Register(nameof(TotalRecords), typeof(int), typeof(PaginationControl), new PropertyMetadata(0));

        public static readonly DependencyProperty CanGoToPreviousPageProperty =
            DependencyProperty.Register(nameof(CanGoToPreviousPage), typeof(bool), typeof(PaginationControl), new PropertyMetadata(false));

        public static readonly DependencyProperty CanGoToNextPageProperty =
            DependencyProperty.Register(nameof(CanGoToNextPage), typeof(bool), typeof(PaginationControl), new PropertyMetadata(false));

        public static readonly DependencyProperty FirstPageCommandProperty =
            DependencyProperty.Register(nameof(FirstPageCommand), typeof(ICommand), typeof(PaginationControl));

        public static readonly DependencyProperty PreviousPageCommandProperty =
            DependencyProperty.Register(nameof(PreviousPageCommand), typeof(ICommand), typeof(PaginationControl));

        public static readonly DependencyProperty NextPageCommandProperty =
            DependencyProperty.Register(nameof(NextPageCommand), typeof(ICommand), typeof(PaginationControl));

        public static readonly DependencyProperty LastPageCommandProperty =
            DependencyProperty.Register(nameof(LastPageCommand), typeof(ICommand), typeof(PaginationControl));

        public int CurrentPage
        {
            get => (int)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public int TotalPages
        {
            get => (int)GetValue(TotalPagesProperty);
            set => SetValue(TotalPagesProperty, value);
        }

        public int TotalRecords
        {
            get => (int)GetValue(TotalRecordsProperty);
            set => SetValue(TotalRecordsProperty, value);
        }

        public bool CanGoToPreviousPage
        {
            get => (bool)GetValue(CanGoToPreviousPageProperty);
            set => SetValue(CanGoToPreviousPageProperty, value);
        }

        public bool CanGoToNextPage
        {
            get => (bool)GetValue(CanGoToNextPageProperty);
            set => SetValue(CanGoToNextPageProperty, value);
        }

        public ICommand FirstPageCommand
        {
            get => (ICommand)GetValue(FirstPageCommandProperty);
            set => SetValue(FirstPageCommandProperty, value);
        }

        public ICommand PreviousPageCommand
        {
            get => (ICommand)GetValue(PreviousPageCommandProperty);
            set => SetValue(PreviousPageCommandProperty, value);
        }

        public ICommand NextPageCommand
        {
            get => (ICommand)GetValue(NextPageCommandProperty);
            set => SetValue(NextPageCommandProperty, value);
        }

        public ICommand LastPageCommand
        {
            get => (ICommand)GetValue(LastPageCommandProperty);
            set => SetValue(LastPageCommandProperty, value);
        }

        public PaginationControl()
        {
            InitializeComponent();
        }
    }
}
