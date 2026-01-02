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
    public partial class ContactsViewModel : PaginationViewModel
    {
        private readonly IPartyService _partyService;

        [ObservableProperty]
        private ObservableCollection<Party> _parties = new();

        [ObservableProperty]
        private ObservableCollection<Party> _paginatedParties = new();

        [ObservableProperty]
        private Party? _selectedParty;

        public int TotalParties => Parties.Count;
        
        public int ActiveParties => Parties.Count;
        
        public decimal OutstandingBalance => 0m; // Placeholder - implement when financial tracking is added
        
        public int RecentActivityCount => 0; // Placeholder - implement when activity tracking is added

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _mobileNumber = string.Empty;

        [ObservableProperty]
        private string _place = string.Empty;

        [ObservableProperty]
        private PartyType _partyType;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _searchText = string.Empty;

        private ObservableCollection<Party> _allParties = new();

        public ObservableCollection<string> PartyTypes { get; } = new()
        {
            "Buyer", "Seller", "Both"
        };

        public ContactsViewModel(IPartyService partyService)
        {
            _partyService = partyService;
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterParties();
            UpdatePaginatedData();
        }

        protected override void UpdatePaginatedData()
        {
            const int PageSizeParties = 15;
            CalculatePagination(Parties, PageSizeParties);
            PaginatedParties.Clear();
            foreach (var party in GetPagedItems(Parties, PageSizeParties))
            {
                PaginatedParties.Add(party);
            }
        }

        [RelayCommand]
        private async Task LoadPartiesAsync()
        {
            try
            {
                IsBusy = true;
                var parties = await _partyService.GetAllPartiesAsync();
                _allParties.Clear();
                Parties.Clear();
                foreach (var party in parties)
                {
                    _allParties.Add(party);
                    Parties.Add(party);
                }
                UpdateSummaryProperties();
                UpdatePaginatedData();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading parties: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void NewParty()
        {
            IsEditMode = false;
            SelectedParty = null;
            Name = string.Empty;
            MobileNumber = string.Empty;
            Place = string.Empty;
            PartyType = PartyType.Both;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private void EditParty(Party? party)
        {
            if (party == null) return;

            if (!MainWindowViewModel.Instance?.IsAdminMode ?? false)
            {
                ErrorMessage = "Only administrators can edit contacts.";
                return;
            }

            IsEditMode = true;
            SelectedParty = party;
            Name = party.Name;
            MobileNumber = party.MobileNumber;
            Place = party.Place;
            PartyType = party.PartyType;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task SavePartyAsync()
        {
            try
            {
                if (!MainWindowViewModel.Instance?.IsAdminMode ?? false)
                {
                    ErrorMessage = "Only administrators can add or edit contacts.";
                    return;
                }

                if (!ValidateParty())
                    return;

                IsBusy = true;

                if (IsEditMode && SelectedParty != null)
                {
                    SelectedParty.Name = Name;
                    SelectedParty.MobileNumber = MobileNumber;
                    SelectedParty.Place = Place;
                    SelectedParty.PartyType = PartyType;
                    await _partyService.UpdatePartyAsync(SelectedParty);
                    ErrorMessage = "Party updated successfully!";
                }
                else
                {
                    var newParty = new Party
                    {
                        Name = Name,
                        MobileNumber = MobileNumber,
                        Place = Place,
                        PartyType = PartyType
                    };
                    await _partyService.AddPartyAsync(newParty);
                    ErrorMessage = "Party added successfully!";
                }

                await LoadPartiesAsync();
                NewParty();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving party: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeletePartyAsync(Party? party)
        {
            if (party == null) return;

            if (!MainWindowViewModel.Instance?.IsAdminMode ?? false)
            {
                ErrorMessage = "Only administrators can delete contacts.";
                return;
            }

            try
            {
                IsBusy = true;
                await _partyService.DeletePartyAsync(party.PartyId);
                await LoadPartiesAsync();
                ErrorMessage = "Party deleted successfully!";
                NewParty();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting party: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateParty()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Party name is required";
                return false;
            }

            return true;
        }

        private void FilterParties()
        {
            Parties.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var party in _allParties)
                    Parties.Add(party);
            }
            else
            {
                var filtered = _allParties.Where(p =>
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Place.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.MobileNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                );
                foreach (var party in filtered)
                    Parties.Add(party);
            }
            UpdateSummaryProperties();
            UpdatePaginatedData();
        }

        private void UpdateSummaryProperties()
        {
            OnPropertyChanged(nameof(TotalParties));
            OnPropertyChanged(nameof(ActiveParties));
            OnPropertyChanged(nameof(OutstandingBalance));
            OnPropertyChanged(nameof(RecentActivityCount));
        }

        public async Task InitializeAsync()
        {
            await LoadPartiesAsync();
        }
    }
}

