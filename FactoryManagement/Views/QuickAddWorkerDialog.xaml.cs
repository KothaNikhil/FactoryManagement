using FactoryManagement.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FactoryManagement.Views
{
    public partial class QuickAddWorkerDialog : Window
    {
        public Worker? NewWorker { get; private set; }
        private Worker? _existingWorker;

        public QuickAddWorkerDialog()
        {
            InitializeComponent();
            this.PreviewKeyDown += QuickAddWorkerDialog_PreviewKeyDown;
        }

        public QuickAddWorkerDialog(Worker worker)
        {
            InitializeComponent();
            _existingWorker = worker;
            LoadWorkerData(worker);
            this.PreviewKeyDown += QuickAddWorkerDialog_PreviewKeyDown;
        }

        private void QuickAddWorkerDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
                e.Handled = true;
            }
        }

        private void LoadWorkerData(Worker worker)
        {
            // Update UI for edit mode
            this.Title = "Edit Worker";
            TitleTextBlock.Text = "✏️ Edit Worker";
            AddWorkerButton.Content = "UPDATE WORKER";
            
            WorkerNameTextBox.Text = worker.Name;
            MobileNumberTextBox.Text = worker.MobileNumber;
        }

        private void AddWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;

            // Validation
            if (string.IsNullOrWhiteSpace(WorkerNameTextBox.Text))
            {
                ErrorTextBlock.Text = "Worker name is required!";
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            // Create or update worker
            NewWorker = new Worker
            {
                WorkerId = _existingWorker?.WorkerId ?? 0,
                Name = WorkerNameTextBox.Text.Trim(),
                MobileNumber = MobileNumberTextBox.Text.Trim(),
                Status = _existingWorker?.Status ?? WorkerStatus.Active,
                Address = _existingWorker?.Address ?? string.Empty,
                Notes = _existingWorker?.Notes ?? string.Empty
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
