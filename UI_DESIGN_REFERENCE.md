# UI/UX Design Reference Guide

## Color Palette

### Background Colors
- **Primary Background** (`PrimaryBackground`): `#1A1D29` - Deep navy/charcoal
- **Secondary Background** (`SecondaryBackground`): `#252837` - Slightly lighter
- **Card Background** (`CardBackground`): `#2D3142` - Medium dark
- **Elevated Background** (`ElevatedBackground`): `#363B4E` - Lighter elevated

### Accent Colors (Purple Theme)
- **Primary Accent** (`PrimaryAccent`): `#B8B0FF` - Soft lavender
- **Secondary Accent** (`SecondaryAccent`): `#9B8CFF` - Medium purple
- **Dark Accent** (`DarkAccent`): `#6B5DD3` - Deep purple
- **Accent Glow** (`AccentGlow`): `#C4B5FF` - Bright purple
- **Accent Hover** (`AccentHover`): `#A89FFF`
- **Accent Pressed** (`AccentPressed`): `#8A7BFF`

### Status Colors
- **Success** (`SuccessColor`): `#3DAA8F` - Muted teal-green
- **Error/Danger** (`ErrorColor`): `#E86B6B` - Muted coral red
- **Warning** (`WarningColor`): `#E89B47` - Muted amber orange
- **Info** (`InfoColor`): `#5A9EDF` - Soft blue

### Text Colors
- **Primary Text** (`TextPrimary`): `#E8E9F3` - Off-white
- **Secondary Text** (`TextSecondary`): `#A0A3BD` - Muted gray
- **Disabled/Muted** (`TextDisabled`): `#6B7280` - Dark gray
- **Highlight** (`TextHighlight`): `#FFFFFF` - Pure white

### Borders & Dividers
- **Primary Border** (`BorderPrimary`): `#4A4E69` - Gray-blue
- **Subtle Border** (`BorderSubtle`): `#363B4E` - Dark
- **Accent Border** (`BorderAccent`): `#9B8CFF` - Purple
- **Focus Border** (`BorderFocus`): `#B8B0FF` - Light purple

---

## Key UI Patterns & Styles

### Summary Cards (Top Row Cards)
**Usage**: Display key metrics at the top of screens (Loans, Expenses, Balances, etc.)

**Pattern**:
```xaml
<Border Background="{StaticResource SummaryCardSuccess}" CornerRadius="8" Padding="16">
    <Border.Effect>
        <DropShadowEffect Color="Black" Opacity="0.3" ShadowDepth="2" BlurRadius="8"/>
    </Border.Effect>
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        
        <TextBlock Grid.Column="0" Text="💰" FontSize="32" VerticalAlignment="Center" Margin="0,0,12,0"/>
        
        <StackPanel Grid.Column="1" VerticalAlignment="Center">
            <TextBlock Text="Label" Foreground="{StaticResource LabelSuccess}" FontSize="11" Margin="0,0,0,4"/>
            <TextBlock Text="Value" Foreground="White" FontSize="20" FontWeight="Bold"/>
        </StackPanel>
    </Grid>
</Border>
```

**Available Variants**:
- `SummaryCardSuccess` + `LabelSuccess` - For positive/gain metrics
- `SummaryCardDanger` + `LabelDanger` - For negative/loss metrics
- `SummaryCardInfo` + `LabelInfo` - For informational metrics
- `SummaryCardWarning` + `LabelWarning` - For warning/alert metrics

---

### Form Card (Content Panels)
**Usage**: Wrapping forms, lists, and content sections

**Style**: `FormCard`
```xaml
<Border Style="{StaticResource FormCard}">
    <!-- Content here -->
</Border>
```

**Properties**:
- BorderBrush: `BorderPrimary`
- Background: `CardBackground`
- CornerRadius: 8
- Padding: 24
- BorderThickness: 1

---

### Section Header
**Usage**: Title headers with icon and text

**Custom Control**: `<controls:SectionHeader />`
```xaml
<controls:SectionHeader DockPanel.Dock="Top" 
                      Title="User Management" 
                      IconKind="AccountGroup" 
                      IconBackground="{StaticResource Card.Info}" />
```

**Parameters**:
- `Title`: Header text
- `IconKind`: MaterialDesign icon name (e.g., "AccountGroup", "CreditCard", "Settings")
- `IconBackground`: Color for icon background badge

---

### DataGrid
**Style**: `Section.DataGrid`

**Column Headers**: `DataGrid.Header.Standard`

**Pattern**:
```xaml
<DataGrid ItemsSource="{Binding Items}" 
          SelectedItem="{Binding SelectedItem, Mode=TwoWay}" 
          Style="{StaticResource Section.DataGrid}">
    <DataGrid.ColumnHeaderStyle>
        <Style TargetType="DataGridColumnHeader" BasedOn="{StaticResource DataGrid.Header.Standard}"/>
    </DataGrid.ColumnHeaderStyle>
    <DataGrid.Columns>
        <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>
    </DataGrid.Columns>
</DataGrid>
```

---

### Status Badge
**Usage**: Display Active/Inactive, Success/Error status

**Pattern**:
```xaml
<Border CornerRadius="4" Padding="8,4" HorizontalAlignment="Center" BorderThickness="1">
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="Background" Value="#1A3DAA8F"/>
            <Setter Property="BorderBrush" Value="{StaticResource SuccessColor}"/>
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsActive}" Value="False">
                    <Setter Property="Background" Value="#1AE86B6B"/>
                    <Setter Property="BorderBrush" Value="{StaticResource ErrorColor}"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
    <TextBlock FontSize="11" FontWeight="Medium" Text="Active" Foreground="{StaticResource SuccessColor}"/>
</Border>
```

---

### Button Styles

#### Edit Button (DataGrid)
**Style**: `DataGrid.EditButton`
```xaml
<Button Style="{StaticResource DataGrid.EditButton}"
        Command="{Binding EditCommand}"
        CommandParameter="{Binding}"
        ToolTip="Edit">
    <materialDesign:PackIcon Kind="Pencil" Width="16" Height="16"/>
</Button>
```

#### Action Buttons
- **Create/Add**: Use `Button.Create` color = `InfoColor`
- **Save**: Use `Button.Save` color = `SuccessColor`
- **Delete**: Use `ErrorColor`
- **Cancel**: Use `TextSecondary`

---

### Search & Filter Pattern
**Custom Control**: `<controls:SearchBox />`
```xaml
<controls:SearchBox DockPanel.Dock="Top" 
                    PlaceholderText="Search by name or code..." />
```

---

### Pagination Control
**Custom Control**: `<controls:PaginationControl />`
```xaml
<controls:PaginationControl DockPanel.Dock="Bottom"
    CurrentPage="{Binding CurrentPage, Mode=TwoWay}"
    TotalPages="{Binding TotalPages}"
    TotalRecords="{Binding TotalRecords}"
    CanGoToPreviousPage="{Binding CanGoToPreviousPage}"
    CanGoToNextPage="{Binding CanGoToNextPage}"
    FirstPageCommand="{Binding GoToFirstPageCommand}"
    PreviousPageCommand="{Binding GoToPreviousPageCommand}"
    NextPageCommand="{Binding GoToNextPageCommand}"
    LastPageCommand="{Binding GoToLastPageCommand}"
    Margin="0,16,0,0"/>
```

---

## Layout Patterns

### Two-Column Layout (Form + List)
**Usage**: List on left, form/details on right (like UsersView)

```xaml
<Grid Margin="20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>      <!-- List -->
        <ColumnDefinition Width="500"/>    <!-- Form -->
    </Grid.ColumnDefinitions>

    <!-- Left: List Panel -->
    <Border Grid.Column="0" Margin="0,0,12,0" Style="{StaticResource FormCard}">
        <!-- DataGrid and controls -->
    </Border>

    <!-- Right: Form Panel -->
    <Border Grid.Column="1" Style="{StaticResource FormCard}">
        <!-- Form fields -->
    </Border>
</Grid>
```

### Summary Cards + Content Layout
**Usage**: Metrics at top, content below (like FinancialRecordsView)

```xaml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>    <!-- Summary Cards -->
        <RowDefinition Height="Auto"/>    <!-- Filters -->
        <RowDefinition Height="Auto"/>    <!-- Tabs -->
        <RowDefinition Height="*"/>       <!-- Content -->
    </Grid.RowDefinitions>

    <!-- Row 0: Summary Cards -->
    <Grid Grid.Row="0" Margin="0,0,0,20">
        <!-- 4 Summary cards side-by-side -->
    </Grid>

    <!-- Row 3: Content -->
    <Border Grid.Row="3" Style="{StaticResource FormCard}">
        <!-- DataGrid or tabbed content -->
    </Border>
</Grid>
```

---

## MaterialDesign Icons Used

Common icons used across the application:
- `AccountGroup` - Users
- `AccountEdit` - User edit
- `Pencil` - Edit action
- `Delete` - Delete action
- `Plus` - Add/Create action
- `CreditCard` - Financial/Payments
- `Settings` - Settings
- `Home` - Dashboard
- `FileDocument` - Reports/Documents
- `ChartLine` - Analytics
- `Package` - Inventory
- `Store` - Store/Parties
- `Worker` - Workers/Employees
- `Money` - Money/Transactions

---

## Spacing & Sizing Standards

### Standard Margins
- **Page Margin**: 20
- **Section Spacing**: 20 (between major sections)
- **Card Padding**: 24 (FormCard), 16 (SummaryCard)
- **Element Spacing**: 12 (horizontal), 8-12 (vertical)

### Font Sizes
- **Page Title**: 24, FontWeight=Bold
- **Section Title**: 14-16, FontWeight=SemiBold
- **Card Label**: 11
- **Card Value**: 20, FontWeight=Bold
- **Normal Text**: 12
- **Small Text**: 11
- **DataGrid**: 12

### Border Radius
- **Cards**: 8
- **Status Badges**: 4
- **Buttons**: 4-8

### Shadows
- **Cards**: `<DropShadowEffect Color="Black" Opacity="0.3" ShadowDepth="2" BlurRadius="8"/>`

---

## Design System Notes

### Dark Mode Only
- Application uses dark theme exclusively
- All colors are optimized for dark backgrounds
- No light mode support needed (pre-release)

### Color Consistency
- Always use theme resources (`{StaticResource ...}`)
- Never hardcode colors except for specific effects
- This ensures consistency and allows for future theme changes

### Accessibility
- Sufficient contrast between text and background
- Clear visual hierarchy through color, size, and weight
- Status indicators use both color AND text/icon

### Responsive Design
- Use Grid with proportional columns (e.g., `Width="*"`)
- Avoid fixed widths where possible
- Use `MinWidth` for DataGrid columns

---

## Examples in Codebase

### Good References
- **UsersView.xaml** - Two-column layout with list and form
- **FinancialRecordsView.xaml** - Summary cards + tabbed content
- **PayrollManagementView.xaml** - Similar pattern with summary cards

### Key Files to Reference
- **Themes/DarkColors.xaml** - All color definitions
- **Themes/DarkTheme.xaml** - All style definitions
- **Controls/SectionHeader.xaml** - Header component
- **Controls/SearchBox.xaml** - Search component
- **Controls/PaginationControl.xaml** - Pagination component

---

## For CashAccountsView Implementation

Based on this guide, the CashAccountsView should:

1. ✅ Use dark theme colors from DarkColors.xaml
2. ✅ Have summary cards showing:
   - Total Cash Balance
   - Total Bank Balance
   - Combined Total Balance
3. ✅ Use FormCard style for main content
4. ✅ Have two sections:
   - Left: List of accounts (DataGrid)
   - Right: Account form (Create/Edit)
5. ✅ Include SectionHeader for title
6. ✅ Use SummaryCard pattern for balance display
7. ✅ Maintain consistent styling with other views

