using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Windows;

namespace FactoryManagement.Views
{
    public partial class QuickAddPartyDialog : Window
    {
        private readonly IPartyService _partyService;
        public Party? NewParty { get; private set; }

        public QuickAddPartyDialog(IPartyService partyService)
        {
            InitializeComponent();
            _partyService = partyService;
            PartyNameTextBox.Focus();
        }

        private async void AddPartyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(PartyNameTextBox.Text))
                {
                    ShowError("Party name is required");
                    return;
                }

                // Get party type
                PartyType partyType = PartyTypeComboBox.SelectedIndex switch
                {
                    0 => PartyType.Buyer,
                    1 => PartyType.Seller,
                    2 => PartyType.Both,
                    _ => PartyType.Both
                };

                // Create party
                var party = new Party
                {
                    Name = PartyNameTextBox.Text.Trim(),
                    MobileNumber = MobileNumberTextBox.Text.Trim(),
                    Place = PlaceTextBox.Text.Trim(),
                    PartyType = partyType
                };

                // Save to database
                NewParty = await _partyService.AddPartyAsync(party);

                // Close dialog with success
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Error adding party: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}
