using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FactoryManagement.ViewModels
{
    public partial class PaginationViewModel : ViewModelBase
    {
        protected const int DefaultPageSize = 19;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalRecords = 0;

        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => CurrentPage < TotalPages;

        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        private void GoToFirstPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage = 1;
                UpdatePaginatedData();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        private void GoToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdatePaginatedData();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private void GoToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdatePaginatedData();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private void GoToLastPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage = TotalPages;
                UpdatePaginatedData();
            }
        }

        partial void OnCurrentPageChanged(int value)
        {
            GoToFirstPageCommand.NotifyCanExecuteChanged();
            GoToPreviousPageCommand.NotifyCanExecuteChanged();
            GoToNextPageCommand.NotifyCanExecuteChanged();
            GoToLastPageCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanGoToPreviousPage));
            OnPropertyChanged(nameof(CanGoToNextPage));
        }

        partial void OnTotalPagesChanged(int value)
        {
            GoToNextPageCommand.NotifyCanExecuteChanged();
            GoToLastPageCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanGoToNextPage));
        }

        protected virtual void UpdatePaginatedData()
        {
            // Override in derived classes to update the paginated collection
        }

        protected void CalculatePagination<T>(IEnumerable<T> allItems, int pageSize = DefaultPageSize)
        {
            var itemsList = allItems?.ToList() ?? new List<T>();
            TotalRecords = itemsList.Count;
            TotalPages = TotalRecords > 0 ? (int)Math.Ceiling((double)TotalRecords / pageSize) : 1;

            // Ensure current page is valid
            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
            }
            else if (CurrentPage < 1)
            {
                CurrentPage = 1;
            }
        }

        protected IEnumerable<T> GetPagedItems<T>(IEnumerable<T> allItems, int pageSize = DefaultPageSize)
        {
            return allItems?
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                ?? Enumerable.Empty<T>();
        }
    }
}
