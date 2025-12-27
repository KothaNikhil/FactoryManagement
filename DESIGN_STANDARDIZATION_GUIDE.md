# Factory Management System - Design Standardization Guide

## Complete Color Palette (Extracted from Financial Transactions & Wages Management)

### Summary Card Colors
```
Green (Success/Positive)      : #3d8b5f  - Loans Given, Total Wages Paid, Purchases
Green Label Tint              : #90EE90
Red (Danger/Outstanding)      : #b85450  - Loans Taken, Outstanding Issues
Red Label Tint                : #FFB6C1
Blue (Info/Neutral)           : #4a90c8  - Interest Receivable, Worker Wages
Blue Label Tint               : #ADD8E6
Orange (Warning/Advance)      : #d9a05b  - Interest Payable, Advances Given
Orange Label Tint             : #FFEAA7
```

### Card/Panel Backgrounds
```
Main Card Background          : #2a2a2a
Card Header/Section Background: #1a1a1a
Card Border                   : #444
Info Box Background           : #1a1a1a
```

### DataGrid Colors
```
Row Default Background        : #2a2a2a
Row Alternate Background      : #333
Header Background             : #1a1a1a
Header Text                   : #BBB
Grid Borders                  : #444
```

### Text Colors
```
Primary Text                  : White
Secondary/Muted Text          : #888
Form Labels                   : #AAA
Positive Amount               : #3d8b5f
Negative Amount/Danger        : #FF6B6B
```

### Button Colors
```
Create/Primary Button         : #0275d8
Create Hover                  : #025aa5
Save/Success Button           : #1e7e34
Save Hover                    : #176428
Info Button                   : #4a90c8
Warning Button                : #d9a05b
```

### Icon Colors
```
Primary Icon                  : #4FC3F7
Success Icon                  : #1e7e34
```

## Design Patterns

### 1. Summary Cards (Top Row)
**Structure:**
- 4 cards in a row with 10px spacing
- Each card has: emoji icon + label + amount
- Border radius: 8px
- Padding: 16px
- Drop shadow: Black 0.3 opacity, 2px depth, 8px blur

**Layout:**
```xaml
<Grid.ColumnDefinitions>
    <ColumnDefinition Width="*"/>
    <ColumnDefinition Width="10"/>
    <ColumnDefinition Width="*"/>
    <ColumnDefinition Width="10"/>
    <ColumnDefinition Width="*"/>
    <ColumnDefinition Width="10"/>
    <ColumnDefinition Width="*"/>
</Grid.ColumnDefinitions>

<Border Background="#3d8b5f" CornerRadius="8" Padding="16">
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
            <TextBlock Text="Label" Foreground="#90EE90" FontSize="11" Margin="0,0,0,4"/>
            <TextBlock Text="₹12,345.00" Foreground="White" FontSize="20" FontWeight="Bold"/>
        </StackPanel>
    </Grid>
</Border>
```

### 2. Form Cards
**Structure:**
- Border with #444, thickness 1, radius 8
- Background #2a2a2a
- Padding 24px

**Section Headers:**
- Horizontal stack panel with icon badge + text
- Icon badge: colored circle 36x36, radius 8
- Title: 16-18px, Bold, White
- Subtitle: 11px, #888

```xaml
<Border BorderBrush="#444" BorderThickness="1" CornerRadius="8" Background="#2a2a2a" Padding="24">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,20">
            <Border Background="#4a90c8" Width="36" Height="36" CornerRadius="8" Margin="0,0,12,0">
                <TextBlock Text="➕" FontSize="20" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
            </Border>
            <StackPanel VerticalAlignment="Center">
                <TextBlock Text="Section Title" FontSize="16" FontWeight="Bold" Foreground="White"/>
                <TextBlock Text="Subtitle or description" FontSize="11" Foreground="#888" Margin="0,2,0,0"/>
            </StackPanel>
        </StackPanel>

        <!-- Content -->
    </Grid>
</Border>
```

### 3. DataGrid Styling
```xaml
<DataGrid AutoGenerateColumns="False" 
          IsReadOnly="True"
          SelectionMode="Single"
          CanUserAddRows="False"
          Background="Transparent"
          RowBackground="#2a2a2a"
          AlternatingRowBackground="#333"
          BorderThickness="0"
          GridLinesVisibility="None"
          HeadersVisibility="Column"
          FontSize="13">
    <DataGrid.ColumnHeaderStyle>
        <Style TargetType="DataGridColumnHeader">
            <Setter Property="Background" Value="#1a1a1a"/>
            <Setter Property="Foreground" Value="#BBB"/>
            <Setter Property="FontWeight" Value="Bold"/>
            <Setter Property="FontSize" Value="13"/>
            <Setter Property="Padding" Value="10"/>
            <Setter Property="BorderThickness" Value="0,0,1,1"/>
            <Setter Property="BorderBrush" Value="#444"/>
        </Style>
    </DataGrid.ColumnHeaderStyle>
</DataGrid>
```

### 4. Form Inputs
```xaml
<!-- Label Style -->
<TextBlock Text="Field Name *" 
          Style="{StaticResource MaterialDesignBody2TextBlock}"
          Margin="0,0,0,8"/>

<!-- TextBox -->
<TextBox Text="{Binding Value}" 
        Style="{StaticResource MaterialDesignOutlinedTextBox}"/>

<!-- ComboBox -->
<ComboBox ItemsSource="{Binding Items}"
         Style="{StaticResource MaterialDesignOutlinedComboBox}"/>

<!-- DatePicker -->
<DatePicker SelectedDate="{Binding Date}"
           Style="{StaticResource MaterialDesignOutlinedDatePicker}"/>
```

### 5. Buttons
```xaml
<!-- Primary/Create Button -->
<Button Content="Create Item"
        Background="#0275d8" 
        Foreground="White" 
        Height="38"
        FontSize="13"
        FontWeight="SemiBold"
        BorderThickness="0"
        Cursor="Hand">
    <Button.Style>
        <Style TargetType="Button">
            <Setter Property="Background" Value="#0275d8"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}" 
                                CornerRadius="6" 
                                BorderThickness="0">
                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#025aa5"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Button.Style>
</Button>

<!-- Success/Save Button -->
<Button Content="Save Changes"
        Background="#1e7e34" 
        Foreground="White" 
        Height="38"
        FontSize="13"
        FontWeight="SemiBold"
        BorderThickness="0"
        Cursor="Hand">
    <Button.Style>
        <Style TargetType="Button">
            <Setter Property="Background" Value="#1e7e34"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}" 
                                CornerRadius="6" 
                                BorderThickness="0">
                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#176428"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Button.Style>
</Button>
```

### 6. Info Boxes (Inside Forms)
```xaml
<Border Background="#1a1a1a" CornerRadius="6" Padding="14" Margin="0,0,0,16">
    <StackPanel>
        <Grid Margin="0,0,0,8">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            <TextBlock Grid.Column="0" Text="Label:" Foreground="#AAA" FontSize="12" VerticalAlignment="Center"/>
            <TextBlock Grid.Column="1" Text="Value" 
                      FontWeight="SemiBold" Foreground="White" FontSize="15" 
                      HorizontalAlignment="Right" VerticalAlignment="Center"/>
        </Grid>
    </StackPanel>
</Border>
```

## Screen-Specific Updates

### Dashboard Screen
1. Add 4 summary cards at top:
   - Total Purchases (Green)
   - Total Sales (Blue)
   - Total Wastage (Red)
   - Low Stock Alert (Orange)

2. Update "Recent Entries" table:
   - Use standard DataGrid styling
   - Dark card background #2a2a2a
   - Proper header with icon

3. Update "Stock Levels" panel:
   - Match form card styling
   - Use consistent colors

### New Transaction Screen
1. Left panel (form):
   - Use FormCard style
   - Icon badge header with #4a90c8
   - All inputs with MaterialDesignOutlined styles
   - Save button with success green #1e7e34

2. Right panel (recent transactions):
   - Standard DataGrid styling
   - Card background #2a2a2a

### Reports & Analytics Screen
1. Add summary cards at top:
   - Total Amount (Green)
   - Total Transactions (Blue)
   - Average Transaction (Orange)
   - Date Range Info (Red/Info)

2. Filter section:
   - Use FormCard style
   - Consistent input styling

3. Results table:
   - Standard DataGrid styling

### Items Management Screen
1. Update "All Items" table:
   - Standard DataGrid styling
   - Add proper card header

2. "Add New Item" form:
   - FormCard style
   - Icon badge header
   - SAVE ITEM button with success green

### Contacts Screen
1. "All Parties" table:
   - Standard DataGrid styling
   - Proper card wrapper

2. "Add New Party" form:
   - FormCard style
   - Icon badge header
   - SAVE PARTY button with success green

## Typography Standards

```
Section Titles        : 18px, Bold, White
Section Subtitles     : 11px, #888
Card Labels (small)   : 11px, Tinted color (#90EE90, #FFB6C1, etc.)
Card Values (large)   : 20px, Bold, White
Form Labels           : Body2 style, #AAA
Table Headers         : 13px, Bold, #BBB
Table Data            : 13px, White
Button Text           : 13px, SemiBold, White
```

## Spacing Standards

```
Card Margin           : 0,0,0,20 (bottom spacing between sections)
Card Padding          : 24px
Section Header Margin : 0,0,0,20
Icon Badge Margin     : 0,0,12,0
Column Spacing        : 10px
Form Field Margin     : 0,0,0,16
```

## Implementation Checklist

- [ ] Update DarkTheme.xaml with all color resources
- [ ] Update Dashboard with summary cards and styling
- [ ] Update New Transaction forms and tables
- [ ] Update Reports & Analytics screen
- [ ] Update Inventory screen
- [ ] Update Contacts screen
- [ ] Verify consistency across all screens
- [ ] Test hover states and interactions
- [ ] Verify WCAG AA contrast ratios
- [ ] Test with real data
