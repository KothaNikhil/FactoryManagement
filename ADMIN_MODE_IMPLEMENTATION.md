# Admin Mode Implementation

## Overview
This document describes the implementation of admin-only access control for edit and delete operations throughout the Factory Management System.

## Implementation Date
January 2, 2026

## Changes Made

### 1. MainWindowViewModel - Admin Mode Detection
**File:** `FactoryManagement\ViewModels\MainWindowViewModel.cs`

- Added `IsAdminMode` property that checks if the currently selected user has an "Admin" or "Administrator" role
- Added `OnSelectedUserChanged` partial method to notify UI when admin status changes
- Added `System` namespace for `StringComparison`

```csharp
public bool IsAdminMode => SelectedUser?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true || 
                            SelectedUser?.Role?.Equals("Administrator", StringComparison.OrdinalIgnoreCase) == true;
```

### 2. InventoryViewModel - Items Management
**File:** `FactoryManagement\ViewModels\InventoryViewModel.cs`

Protected operations:
- **EditItem**: Prevents non-admin users from editing items
- **SaveItemAsync**: Prevents non-admin users from adding or updating items
- **DeleteItemAsync**: Prevents non-admin users from deleting items

### 3. ContactsViewModel - Contacts/Parties Management
**File:** `FactoryManagement\ViewModels\ContactsViewModel.cs`

Protected operations:
- **EditParty**: Prevents non-admin users from editing contacts
- **SavePartyAsync**: Prevents non-admin users from adding or updating contacts
- **DeletePartyAsync**: Prevents non-admin users from deleting contacts

### 4. WorkersManagementViewModel - Workers Management
**File:** `FactoryManagement\ViewModels\WorkersManagementViewModel.cs`

Protected operations:
- **EditWorkerAsync**: Prevents non-admin users from editing workers
- **SaveWorkerAsync**: Prevents non-admin users from adding or updating workers
- **DeleteWorkerAsync**: Prevents non-admin users from deleting workers

### 5. UsersViewModel - User Management
**File:** `FactoryManagement\ViewModels\UsersViewModel.cs`

Protected operations:
- **EditUser**: Prevents non-admin users from editing users (with MessageBox notification)
- **SaveUserAsync**: Prevents non-admin users from adding or updating users (with MessageBox notification)
- **DeleteUserAsync**: Prevents non-admin users from deleting users (with MessageBox notification)

### 6. NewTransactionViewModel - Transactions
**File:** `FactoryManagement\ViewModels\NewTransactionViewModel.cs`

Protected operations:
- **EditTransactionAsync**: Prevents non-admin users from editing transactions (with MessageBox notification)
- **DeleteTransactionAsync**: Prevents non-admin users from deleting transactions (with MessageBox notification)

### 7. FinancialRecordsViewModel - Financial Records & Loans
**File:** `FactoryManagement\ViewModels\FinancialRecordsViewModel.cs`

Protected operations:
- **DeleteLoanAsync**: Prevents non-admin users from deleting loans (with MessageBox notification)
- **DeleteFinancialTransactionAsync**: Prevents non-admin users from deleting financial transactions (with MessageBox notification)

### 8. PayrollManagementViewModel - Wage Transactions
**File:** `FactoryManagement\ViewModels\PayrollManagementViewModel.cs`

Protected operations:
- **DeleteWageTransactionAsync**: Prevents non-admin users from deleting wage transactions (with MessageBox notification)

### 9. OperationalExpensesViewModel - Operational Expenses
**File:** `FactoryManagement\ViewModels\OperationalExpensesViewModel.cs`

Protected operations:
- **EditExpense**: Prevents non-admin users from editing expenses
- **SaveExpenseAsync**: Prevents non-admin users from adding or updating expenses
- **DeleteExpenseAsync**: Prevents non-admin users from deleting expenses (with MessageBox notification)

### 10. ExpenseCategoryManagementViewModel - Expense Categories
**File:** `FactoryManagement\ViewModels\ExpenseCategoryManagementViewModel.cs`

Protected operations:
- **EditCategory**: Prevents non-admin users from editing expense categories
- **SaveCategoryAsync**: Prevents non-admin users from adding or updating expense categories
- **DeleteCategoryAsync**: Prevents non-admin users from deleting expense categories (with MessageBox notification)

## How It Works

1. **User Selection**: When a user is selected from the dropdown in the main window, the `MainWindowViewModel.SelectedUser` property is updated.

2. **Admin Check**: The `IsAdminMode` property automatically evaluates whether the current user's role is "Admin" or "Administrator" (case-insensitive).

3. **Operation Protection**: Before performing any edit or delete operation, each ViewModel checks `MainWindowViewModel.Instance?.IsAdminMode`:
   - If `false`, the operation is blocked
   - An error message is displayed (either in the ErrorMessage property or via MessageBox)
   - The method returns early without performing the operation

4. **User Feedback**: Users are informed when they attempt restricted operations:
   - Some operations show inline error messages
   - Critical operations (deletes) show MessageBox dialogs
   - Error messages clearly state "Only administrators can [operation]"

## Security Considerations

- **Client-side validation**: This implementation provides UI-level access control
- **Role-based**: Access is determined by the user's role in the database
- **Graceful degradation**: Non-admin users can view all data but cannot modify it
- **Consistent messaging**: All error messages follow a consistent pattern

## Testing Recommendations

1. **Admin User Testing**:
   - Log in as a user with "Admin" or "Administrator" role
   - Verify all edit and delete operations work normally

2. **Non-Admin User Testing**:
   - Log in as a user with any other role (e.g., "Manager", "Operator")
   - Verify all edit and delete operations are blocked
   - Verify appropriate error messages are shown

3. **Edge Cases**:
   - Test with no user selected
   - Test role name variations (case sensitivity)
   - Test switching between admin and non-admin users

## Default Users

The system comes with two default users:
- **Admin** (Role: "Administrator") - Full access
- **Manager** (Role: "Manager") - Read-only for edit/delete operations
- **Guest** (Role: "Guest") - Read-only for edit/delete operations

## Future Enhancements

Potential improvements for future consideration:
1. Add UI visual cues (disabled buttons) for non-admin users
2. Implement server-side validation in services layer
3. Add audit logging for admin operations
4. Create more granular permission levels
5. Add role-based menu visibility
