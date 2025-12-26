using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FactoryManagement.Services;

namespace FactoryManagement.ViewModels
{
    public class BackupViewModel : ViewModelBase
    {
        private readonly BackupService _backupService;
        private ObservableCollection<BackupFileInfo> _backups = new();
        private BackupFileInfo? _selectedBackup;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;

        public BackupViewModel(BackupService backupService)
        {
            _backupService = backupService;
            Backups = new ObservableCollection<BackupFileInfo>();

            CreateBackupCommand = new RelayCommand(async () => await CreateBackupAsync(), () => !IsProcessing);
            RestoreBackupCommand = new RelayCommand(async () => await RestoreBackupAsync(), () => SelectedBackup != null && !IsProcessing);
            DeleteBackupCommand = new RelayCommand(DeleteBackup, () => SelectedBackup != null && !IsProcessing && (SelectedBackup?.IsDefault != true));
            RefreshBackupsCommand = new RelayCommand(LoadBackups);
            OpenBackupFolderCommand = new RelayCommand(OpenBackupFolder);

            LoadBackups();
        }

        public ObservableCollection<BackupFileInfo> Backups
        {
            get => _backups;
            set => SetProperty(ref _backups, value);
        }

        public BackupFileInfo? SelectedBackup
        {
            get => _selectedBackup;
            set
            {
                SetProperty(ref _selectedBackup, value);
                ((RelayCommand)RestoreBackupCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteBackupCommand).RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                SetProperty(ref _isProcessing, value);
                ((RelayCommand)CreateBackupCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RestoreBackupCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteBackupCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand CreateBackupCommand { get; }
        public ICommand RestoreBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand RefreshBackupsCommand { get; }
        public ICommand OpenBackupFolderCommand { get; }

        private async Task CreateBackupAsync()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Creating backup...";

                var filePath = await _backupService.CreateBackupAsync();
                StatusMessage = $"Backup created successfully: {System.IO.Path.GetFileName(filePath)}";

                LoadBackups();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show(ex.Message, "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task RestoreBackupAsync()
        {
            if (SelectedBackup == null) return;
            if (SelectedBackup.IsDefault)
            {
                MessageBox.Show("The default backup cannot be deleted.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to restore the backup from {SelectedBackup.FormattedDate}?\n\n" +
                    "WARNING: This will replace ALL current data with the backup data. This action cannot be undone!",
                    "Confirm Restore",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                IsProcessing = true;
                StatusMessage = "Restoring backup...";

                await _backupService.RestoreBackupAsync(SelectedBackup.FilePath);
                StatusMessage = $"Backup restored successfully from {SelectedBackup.FormattedDate}";

                MessageBox.Show(
                    "Backup restored successfully!\n\nPlease restart the application for changes to take full effect.",
                    "Restore Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show(ex.Message, "Restore Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void DeleteBackup()
        {
            if (SelectedBackup == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the backup from {SelectedBackup.FormattedDate}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                _backupService.DeleteBackup(SelectedBackup.FilePath);
                StatusMessage = $"Backup deleted: {SelectedBackup.FileName}";
                LoadBackups();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show(ex.Message, "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBackups()
        {
            try
            {
                var backups = _backupService.GetAvailableBackups();
                Backups.Clear();
                foreach (var backup in backups)
                {
                    Backups.Add(backup);
                }

                StatusMessage = $"Found {Backups.Count} backup(s)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading backups: {ex.Message}";
            }
        }

        private void OpenBackupFolder()
        {
            try
            {
                var folder = _backupService.GetBackupDirectory();
                System.Diagnostics.Process.Start("explorer.exe", folder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open backup folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
