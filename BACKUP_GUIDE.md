# Data Backup Feature

## Overview
A comprehensive backup and restore system has been added to the Factory Management application. This allows you to create backups of all your data (items, parties, and transactions) and restore them when needed.

## Features

### 1. Create Backup
- Creates a complete snapshot of all your data
- Backups are stored in JSON format for easy portability
- Automatically organized with timestamp: `Backup_yyyyMMdd_HHmmss.json`
- Stored in: `Documents\FactoryManagement\Backups\`

### 2. Restore Backup
- Restores all data from a selected backup file
- **Warning**: This replaces ALL current data with the backup data
- Includes confirmation dialog to prevent accidental restoration
- Atomic operation - either all data is restored or none (if error occurs)

### 3. Backup Management
- View all available backups in a clean list
- Shows:
  - File name with timestamp
  - Creation date and time
  - File size
- Delete unwanted backups
- Refresh backup list
- Open backup folder in Windows Explorer

### 4. Data Included in Backup
Each backup contains:
- **All Items**: Item names, units, current stock levels
- **All Parties**: Party names, contact info, addresses, party types
- **All Transactions**: Complete transaction history with details
- **Metadata**: Backup date and version information

## How to Use

### Creating a Backup
1. Navigate to "Data Backup" from the main menu
2. Click "Create Backup" button
3. Backup is created instantly with current timestamp
4. Status message confirms backup creation

### Restoring a Backup
1. Navigate to "Data Backup" from the main menu
2. Select a backup from the list
3. Click "Restore Selected" button
4. Confirm the restoration (warning: this replaces all data!)
5. Wait for restoration to complete
6. Restart the application for changes to take full effect

### Managing Backups
- **Refresh**: Update the backup list if you added files manually
- **Delete**: Remove unwanted backup files
- **Open Folder**: View backup files in Windows Explorer

## Best Practices

1. **Regular Backups**: Create backups regularly, especially before major data operations
2. **Before Updates**: Always create a backup before updating the application
3. **Multiple Backups**: Keep several backups from different dates
4. **External Storage**: Consider copying backups to external drives or cloud storage
5. **Test Restores**: Occasionally test restoring a backup to ensure it works

## Technical Details

### Backup File Location
```
C:\Users\[YourUsername]\Documents\FactoryManagement\Backups\
```

### Backup File Format
- **Format**: JSON (human-readable and portable)
- **Naming**: `Backup_yyyyMMdd_HHmmss.json`
- **Example**: `Backup_20231214_143052.json`

### What Happens During Restore
1. System validates the backup file
2. Begins database transaction
3. Clears existing data (transactions, items, parties)
4. Restores items from backup
5. Restores parties from backup
6. Restores transactions from backup
7. Commits transaction (or rolls back if any error occurs)

## Safety Features

- **Confirmation Dialogs**: Prevents accidental data loss
- **Atomic Operations**: Restore either completes fully or not at all
- **Error Handling**: Clear error messages if something goes wrong
- **Version Tracking**: Backup files include version information

## Troubleshooting

### Backup Creation Fails
- Check disk space
- Ensure Documents folder is accessible
- Check application has write permissions

### Restore Fails
- Verify backup file is not corrupted
- Ensure sufficient database space
- Check backup file format matches current version

### Missing Backups
- Use "Refresh" button to reload the list
- Check backup folder location manually
- Verify files have not been moved or deleted

## Navigation
Access the Data Backup feature from the main menu:
- Menu Item: "Data Backup"
- Icon: Backup/Restore symbol
- Located below "Contacts" in the menu
