# Keyboard Accessibility Checklist - Factory Management Application

## Overview
This document tracks keyboard accessibility features for ensuring the application can be fully navigated and used without a mouse.

---

## ✅ ALREADY IMPLEMENTED

### 1. Keyboard Navigation - Main Window
- ✅ **Ctrl+1**: Dashboard
- ✅ **Ctrl+2**: New Transaction
- ✅ **Ctrl+3**: Wages Management
- ✅ **Ctrl+4**: Financial Records
- ✅ **Ctrl+5**: Reports
- ✅ **Ctrl+6**: Items (Inventory)
- ✅ **Ctrl+7**: Parties (Contacts)
- ✅ **Ctrl+8**: Users Management
- ✅ **Ctrl+9**: Data Backup
- ✅ **Ctrl+0**: Exit Application
- Implementation: [MainWindow.xaml.cs](MainWindow.xaml.cs#L145)

### 2. Searchable Dropdowns - Keyboard Support
- ✅ **Type A-Z, 0-9**: Filter items
- ✅ **Backspace**: Delete last character
- ✅ **↑↓ Arrow Keys**: Navigate filtered items
- ✅ **Enter**: Select item and confirm
- ✅ **Tab**: Select item and move to next field
- ✅ **Escape**: Clear search and show all items
- Implementation: [SearchableComboBoxBehavior.cs](Behaviors/SearchableComboBoxBehavior.cs)

### 3. Button Focus Styles
- ✅ **Primary Buttons**: Cyan (#00f2fe) border when focused via Tab
- ✅ **Secondary Buttons**: Cyan border and text when focused
- ✅ **Danger Buttons**: Cyan border when focused
- ✅ **Icon Buttons**: Cyan border when focused
- Implementation: [DarkTheme.xaml](Themes/DarkTheme.xaml)

### 4. Login Screen
- ✅ **User Selection**: ComboBox keyboard navigable
- ✅ **Exit Button**: Keyboard accessible with focus indicator
- ✅ **Login Button**: Keyboard accessible with focus indicator
- Implementation: [LoginWindow.xaml](Views/LoginWindow.xaml)

---

## 🔄 NEEDS IMPROVEMENT / IMPLEMENTATION

### 1. DataGrid Keyboard Navigation
**Status**: ⚠️ PARTIAL

Currently DataGrids:
- ✅ Allow Tab navigation to DataGrid
- ✅ Allow arrow key navigation within rows/columns
- ❌ No clear visual focus indicator when tabbing between DataGrid rows
- ⚠️ Edit/Delete buttons in DataGrid columns need better keyboard accessibility

**Affected Views**:
- NewTransactionView.xaml (Recent Transactions grid)
- InventoryView.xaml (Items list)
- ContactsView.xaml (Parties list)
- FinancialRecordsView.xaml (Loan accounts grid)
- PayrollManagementView.xaml (Workers grid)
- ReportsView.xaml (Report results grid)

**Required Improvements**:
- Add focus indicators to DataGrid rows (border/background highlight)
- Make Edit/Delete buttons more accessible in keyboard mode
- Set TabIndex properly on DataGrids

### 2. Form Field Tab Order
**Status**: ⚠️ NEEDS REVIEW

**Views to Check**:
- NewTransactionView.xaml: Forms should flow logically
- FinancialRecordsView.xaml: Loan form tab order
- InventoryView.xaml: Item form tab order
- ContactsView.xaml: Party form tab order
- UsersView.xaml: User form tab order

**Action Items**:
- [ ] Review and set TabIndex on all form fields
- [ ] Ensure logical flow (top to bottom, left to right)
- [ ] Set IsTabStop="False" on read-only/display elements

### 3. Dialog Boxes Keyboard Support
**Status**: ⚠️ NEEDS IMPROVEMENT

**Dialogs**:
- QuickAddPartyDialog.xaml
- QuickAddWorkerDialog.xaml

**Required**:
- [ ] Set default button (IsDefault="True")
- [ ] Set cancel button (IsCancel="True")
- [ ] Proper Tab order
- [ ] Focus indicators on buttons

### 4. Menu/Navigation Elements
**Status**: ⚠️ PARTIAL

**Current**:
- ✅ Menu items accessible via Ctrl+Number shortcuts
- ❌ No visible focus indicator on menu items

**Required**:
- [ ] Add focus border/highlight to ListBox menu items
- [ ] Allow keyboard selection of menu without Ctrl shortcut (Alt+Down to open, arrows to navigate)

### 5. Scrolling with Keyboard
**Status**: ⚠️ NEEDS VERIFICATION

**Items to Verify**:
- ScrollViewers in all forms work with arrow keys
- Page Up/Page Down works in ScrollViewers
- Space/Enter works for collapsible sections (if any)

### 6. Search/Filter Fields
**Status**: ✅ IMPLEMENTED (Dropdown filtering)
**Status**: ⚠️ NEEDS VERIFICATION for other fields

### 7. Modal Dialogs
**Status**: ⚠️ NEEDS IMPROVEMENT

**Items to Check**:
- [ ] Escape key closes dialogs
- [ ] Enter key confirms default action
- [ ] Tab wraps correctly within dialog
- [ ] Focus trapped in modal (can't tab out to main window)

---

## 📋 DETAILED IMPROVEMENTS NEEDED

### A. DataGrid Button Accessibility

**Issue**: Edit/Delete buttons in DataGrid columns are hard to access with keyboard

**Solution**:
1. Make buttons more discoverable when tabbing
2. Add keyboard shortcuts for common DataGrid operations:
   - **Delete Row**: Shift+Delete on selected row
   - **Edit Row**: Enter on selected row (opens edit form)
   - **Add New**: Ctrl+N

**Implementation Files**:
- NewTransactionView.xaml.cs
- InventoryView.xaml.cs
- ContactsView.xaml.cs
- (Add similar handlers to all DataGrid views)

---

### B. Form TabIndex Organization

**Action**: Add explicit TabIndex to all form fields

**Pattern**:
```xaml
<StackPanel Grid.Row="0" Grid.Column="0">
    <TextBlock Text="Field Label"/>
    <ComboBox TabIndex="0"/>
</StackPanel>

<StackPanel Grid.Row="0" Grid.Column="2">
    <TextBlock Text="Another Field"/>
    <TextBox TabIndex="1"/>
</StackPanel>

<StackPanel Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="3">
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
        <Button TabIndex="100" Content="CANCEL"/>
        <Button TabIndex="101" Content="SAVE"/>
    </StackPanel>
</StackPanel>
```

---

### C. Visual Focus Indicators for DataGrids

**Add to DarkTheme.xaml**:
```xaml
<Style x:Key="DataGrid.FocusVisual" TargetType="DataGridRow">
    <Style.Triggers>
        <Trigger Property="IsSelected" Value="True">
            <Setter Property="BorderThickness" Value="2"/>
            <Setter Property="BorderBrush" Value="#00f2fe"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

---

### D. Keyboard Shortcuts Reference

**Suggested Application-Wide Shortcuts**:

| Shortcut | Action | Location |
|----------|--------|----------|
| **Ctrl+1-9, 0** | Navigate screens | MainWindow |
| **Ctrl+N** | New record | All master screens |
| **Ctrl+S** | Save | All forms |
| **Ctrl+Delete** | Delete selected | All DataGrids |
| **Escape** | Close dialog / Clear form | Forms & Dialogs |
| **Enter** | Confirm / Select | Dialogs & Forms |
| **Tab** | Navigate fields | All forms |
| **Shift+Tab** | Navigate fields backward | All forms |
| **Alt+F4** | Close window | All windows |

---

## 🎯 IMPLEMENTATION PRIORITY

### Priority 1 (Critical - Do First)
1. [ ] Add TabIndex to all form fields in NewTransactionView
2. [ ] Add visible focus indicator to DataGrid rows
3. [ ] Make Edit/Delete buttons accessible in DataGrids
4. [ ] Add Escape key support to dialogs

### Priority 2 (High - Do Soon)
1. [ ] Add Tab order to all master data forms
2. [ ] Add focus indicators to menu items
3. [ ] Implement Ctrl+N, Ctrl+S shortcuts in forms
4. [ ] Add Shift+Delete for DataGrid row deletion

### Priority 3 (Medium - Nice to Have)
1. [ ] Alt key menu navigation
2. [ ] Additional keyboard shortcuts for common operations
3. [ ] Screen reader accessibility improvements
4. [ ] High contrast mode support

---

## 📝 TESTING CHECKLIST

### Basic Keyboard Navigation Test
- [ ] Tab through entire application without mouse
- [ ] All buttons are reachable and have visible focus indicators
- [ ] All form fields are reachable via Tab
- [ ] No "dead zones" where Tab gets stuck

### DataGrid Keyboard Test
- [ ] Tab to DataGrid - row becomes selected
- [ ] Arrow keys navigate rows/columns
- [ ] Enter activates edit on row (if implemented)
- [ ] Shift+Delete deletes row (if implemented)
- [ ] Can tab to buttons within DataGrid cells

### Form Submission Test
- [ ] Tab through all form fields in logical order
- [ ] Tab reaches Save button
- [ ] Enter on Save button submits form
- [ ] Escape clears form or closes dialog

### Screen Navigation Test
- [ ] Ctrl+1 goes to Dashboard
- [ ] Ctrl+2 goes to New Transaction
- [ ] All Ctrl+N shortcuts work
- [ ] Focus is on logical first field after navigation

### Focus Visibility Test
- [ ] Tab shows cyan border around buttons
- [ ] Tab shows indicator on menu items
- [ ] Tab shows indicator in DataGrid
- [ ] Focus indicators are clearly visible

---

## 📚 REFERENCE DOCUMENTS

- [USER_GUIDE.md](USER_GUIDE.md#keyboard-shortcuts)
- [SEARCHABLE_DROPDOWN_USER_GUIDE.md](SEARCHABLE_DROPDOWN_USER_GUIDE.md)
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md#keyboard-shortcuts)

---

## 🔗 RELATED FILES

**Core Implementation**:
- [MainWindow.xaml.cs](Views/MainWindow.xaml.cs) - Ctrl+Number shortcuts
- [SearchableComboBoxBehavior.cs](Behaviors/SearchableComboBoxBehavior.cs) - Dropdown keyboard support
- [DarkTheme.xaml](Themes/DarkTheme.xaml) - Button focus styles

**Views Needing Updates**:
- [NewTransactionView.xaml](Views/NewTransactionView.xaml)
- [InventoryView.xaml](Views/InventoryView.xaml)
- [ContactsView.xaml](Views/ContactsView.xaml)
- [FinancialRecordsView.xaml](Views/FinancialRecordsView.xaml)
- [PayrollManagementView.xaml](Views/PayrollManagementView.xaml)
- [ReportsView.xaml](Views/ReportsView.xaml)

---

**Last Updated**: December 29, 2025
**Status**: Phase 1 Complete; Phase 2 queued (menus/forms)
