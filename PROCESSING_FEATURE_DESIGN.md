# Processing Transaction Feature - Design Document

## Overview
This document outlines the design for adding a **Processing** transaction type to handle job work scenarios where customers bring raw materials (e.g., paddy) for processing into finished goods (e.g., rice).

**Date:** December 26, 2025  
**Version:** 1.0

---

## Business Use Case

### Scenario: Job Work / Processing Service
1. **Customer brings raw material** (e.g., 100 kg paddy)
2. **Factory processes** the raw material into finished product (e.g., 70 kg rice + 30 kg waste)
3. **Customer receives** the processed product (70 kg rice)
4. **Factory charges** a processing fee (e.g., ₹5 per kg processed)

### Key Characteristics
- **Input Material**: Received from customer (tracked as separate item)
- **Output Material**: Returned to customer (tracked as separate item)
- **Processing Fee**: Revenue earned for the service
- **Stock Impact**: 
  - Raw material: Temporarily increases when received, decreases when processed
  - Finished product: Increases when processed, decreases when returned
- **No Material Ownership**: Factory doesn't own the material; it's a service transaction

---

## Design Approach

### Option 1: Single Transaction with Processing Fee (Recommended)
**Model:** Simple processing transaction that tracks the service fee

**Advantages:**
- ✅ Simpler to implement and understand
- ✅ Aligns with existing transaction model
- ✅ Clear revenue tracking (processing fee)
- ✅ Minimal UI changes

**Disadvantages:**
- ❌ Doesn't track stock movement explicitly
- ❌ May need manual stock adjustments

### Option 2: Compound Transaction with Material Tracking
**Model:** Complex transaction tracking input, output, and conversion

**Advantages:**
- ✅ Complete material tracking
- ✅ Accurate stock management
- ✅ Better for inventory reconciliation

**Disadvantages:**
- ❌ More complex implementation
- ❌ Significant model changes needed
- ❌ Complex UI requirements

### **Selected Approach: Hybrid Model**
Combine both approaches:
- Add "Processing" to the existing TransactionType enum
- Extend Transaction model to support optional input/output items
- Track processing fee as the main transaction amount
- Support optional material conversion tracking

---

## Technical Design

### 1. Data Model Changes

#### 1.1 Transaction Model Extension
```csharp
// Update Transaction.cs
public enum TransactionType
{
    Buy,
    Sell,
    Wastage,
    Processing  // NEW
}

public class Transaction
{
    // Existing properties...
    public int TransactionId { get; set; }
    public int ItemId { get; set; }  // Output/Processed item
    public int? PartyId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }  // Quantity of processed item
    public decimal PricePerUnit { get; set; }  // Processing fee per unit
    public decimal TotalAmount { get; set; }  // Total processing fee
    
    // NEW: Processing-specific fields
    public int? InputItemId { get; set; }  // Raw material item (optional)
    public decimal? InputQuantity { get; set; }  // Quantity of raw material received
    public decimal? ConversionRate { get; set; }  // Output/Input ratio (e.g., 0.7 for 70%)
    
    // Foreign key for input item
    [ForeignKey(nameof(InputItemId))]
    public virtual Item? InputItem { get; set; }
}
```

#### 1.2 Database Migration
- Add nullable columns: `InputItemId`, `InputQuantity`, `ConversionRate`
- Update existing data (set null for non-processing transactions)

### 2. Party Type Extension

Add new party type for processing customers:
```csharp
public enum PartyType
{
    Buyer,
    Seller,
    Both,
    Lender,
    Borrower,
    Financial,
    Processor  // NEW: Customer who brings material for processing
}
```

### 3. Service Layer Changes

#### 3.1 TransactionService
```csharp
public interface ITransactionService
{
    // Existing methods...
    
    // NEW: Processing-specific method
    Task<Transaction> AddProcessingTransactionAsync(
        int partyId,
        int inputItemId,
        decimal inputQuantity,
        int outputItemId,
        decimal outputQuantity,
        decimal processingFeePerUnit,
        int enteredBy,
        string notes = "");
}
```

#### 3.2 ItemService - Stock Update Logic
```csharp
public async Task UpdateStockAsync(int itemId, decimal quantityChange, TransactionType transactionType)
{
    var item = await _itemRepository.GetByIdAsync(itemId);
    if (item != null)
    {
        switch (transactionType)
        {
            case TransactionType.Buy:
                item.CurrentStock += quantityChange;
                break;
            case TransactionType.Sell:
            case TransactionType.Wastage:
                item.CurrentStock -= quantityChange;
                break;
            case TransactionType.Processing:
                // For processing: output item increases stock
                item.CurrentStock += quantityChange;
                break;
        }
        item.ModifiedDate = DateTime.Now;
        await _itemRepository.UpdateAsync(item);
    }
}

// NEW: Handle dual stock update for processing
public async Task UpdateStockForProcessingAsync(
    int inputItemId, 
    decimal inputQuantity, 
    int outputItemId, 
    decimal outputQuantity)
{
    // Decrease input material stock
    await UpdateStockAsync(inputItemId, inputQuantity, TransactionType.Sell);
    
    // Increase output product stock
    await UpdateStockAsync(outputItemId, outputQuantity, TransactionType.Buy);
}
```

### 4. ViewModel Changes

#### 4.1 TransactionEntryViewModel
```csharp
// Add to TransactionTypes collection
public ObservableCollection<string> TransactionTypes { get; } = new()
{
    "Buy", 
    "Sell", 
    "Wastage",
    "Processing"  // NEW
};

// NEW: Processing-specific properties
[ObservableProperty]
private Item? _inputItem;

[ObservableProperty]
private decimal _inputQuantity;

[ObservableProperty]
private decimal _conversionRate;

[ObservableProperty]
private bool _isProcessingMode;

// Update visibility based on transaction type
partial void OnSelectedTransactionTypeStringChanged(string value)
{
    IsPartyRequired = SelectedTransactionType != TransactionType.Wastage;
    IsProcessingMode = SelectedTransactionType == TransactionType.Processing;
    OnPropertyChanged(nameof(SelectedTransactionType));
}
```

#### 4.2 DashboardViewModel
```csharp
// NEW: Add processing metrics
[ObservableProperty]
private decimal _totalProcessingFees;

[ObservableProperty]
private int _processingTransactionCount;

// Update LoadDataAsync
private async Task LoadDataAsync()
{
    // ... existing code ...
    
    TotalProcessingFees = transactions
        .Where(t => t.TransactionType == TransactionType.Processing)
        .Sum(t => t.TotalAmount);
    
    ProcessingTransactionCount = transactions
        .Count(t => t.TransactionType == TransactionType.Processing);
}
```

#### 4.3 ReportsViewModel
```csharp
// Add processing filter option
public ObservableCollection<string> TransactionTypeFilters { get; } = new()
{
    "All",
    "Buy",
    "Sell",
    "Wastage",
    "Processing"  // NEW
};

// Update report generation to include processing transactions
// Ensure column visibility handles processing-specific fields
```

### 5. UI Changes

#### 5.1 TransactionEntryView.xaml
```xml
<!-- Add conditional visibility for processing fields -->
<StackPanel Visibility="{Binding IsProcessingMode, Converter={StaticResource BoolToVisibilityConverter}}">
    <Label Content="Input Material:" />
    <ComboBox ItemsSource="{Binding Items}" 
              SelectedItem="{Binding InputItem}"
              DisplayMemberPath="ItemName" />
    
    <Label Content="Input Quantity:" />
    <TextBox Text="{Binding InputQuantity}" />
    
    <Label Content="Conversion Rate (%):" />
    <TextBox Text="{Binding ConversionRate}" />
    
    <TextBlock Text="{Binding ExpectedOutputQuantity, StringFormat='Expected Output: {0:N2}'}" />
</StackPanel>

<!-- Rename "Item" label to "Output Item" when in processing mode -->
<Label Content="{Binding ItemLabelText}" />
```

#### 5.2 DashboardView.xaml
```xml
<!-- Add processing metrics card -->
<Border Style="{StaticResource DashboardCardStyle}">
    <StackPanel>
        <TextBlock Text="Processing Fees" Style="{StaticResource CardHeaderStyle}" />
        <TextBlock Text="{Binding TotalProcessingFees, StringFormat='₹{0:N2}'}" 
                   Style="{StaticResource CardValueStyle}" />
        <TextBlock Text="{Binding ProcessingTransactionCount, StringFormat='{0} transactions'}" 
                   Style="{StaticResource CardSubtextStyle}" />
    </StackPanel>
</Border>
```

#### 5.3 ReportsView.xaml
```xml
<!-- Add processing columns to DataGrid -->
<DataGridTextColumn Header="Input Material" 
                    Binding="{Binding InputItem.ItemName}"
                    Visibility="{Binding IsProcessingFilterActive, Converter={...}}" />

<DataGridTextColumn Header="Input Qty" 
                    Binding="{Binding InputQuantity, StringFormat='{0:N2}'}"
                    Visibility="{Binding IsProcessingFilterActive, Converter={...}}" />

<DataGridTextColumn Header="Conversion Rate" 
                    Binding="{Binding ConversionRate, StringFormat='{0:P2}'}"
                    Visibility="{Binding IsProcessingFilterActive, Converter={...}}" />
```

---

## Implementation Plan

### Phase 1: Database & Models (Foundation)
1. ✅ Update `TransactionType` enum
2. ✅ Add nullable columns to `Transaction` model
3. ✅ Create database migration
4. ✅ Update `PartyType` enum
5. ✅ Run migration and test

### Phase 2: Services Layer
1. ✅ Update `ItemService.UpdateStockAsync()` for Processing type
2. ✅ Add `UpdateStockForProcessingAsync()` method
3. ✅ Update `TransactionService.AddTransactionAsync()` to handle processing
4. ✅ Add `AddProcessingTransactionAsync()` specialized method
5. ✅ Write unit tests for service methods

### Phase 3: ViewModels
1. ✅ Update `TransactionEntryViewModel`
   - Add processing-specific properties
   - Add validation logic
   - Update save command
2. ✅ Update `DashboardViewModel`
   - Add processing metrics
   - Update LoadDataAsync
3. ✅ Update `ReportsViewModel`
   - Add processing filter
   - Update report generation

### Phase 4: UI Updates
1. ✅ Update `TransactionEntryView.xaml`
   - Add processing input fields
   - Add conditional visibility
2. ✅ Update `DashboardView.xaml`
   - Add processing metrics card
3. ✅ Update `ReportsView.xaml`
   - Add processing columns
   - Update filters

### Phase 5: Testing
1. ✅ Unit tests for models
2. ✅ Unit tests for services
3. ✅ Unit tests for viewmodels
4. ✅ Integration tests
5. ✅ UI tests
6. ✅ End-to-end workflow testing

### Phase 6: Documentation
1. ✅ Update USER_GUIDE.md
2. ✅ Update QUICK_REFERENCE.md
3. ✅ Add processing workflow examples
4. ✅ Update training materials

---

## Validation Rules

### New Transaction Validation
```csharp
private bool ValidateProcessingTransaction()
{
    if (SelectedTransactionType != TransactionType.Processing)
        return true;
    
    // Processing-specific validations
    if (InputItem == null)
    {
        ErrorMessage = "Please select input material";
        return false;
    }
    
    if (InputQuantity <= 0)
    {
        ErrorMessage = "Input quantity must be greater than zero";
        return false;
    }
    
    if (SelectedItem == null)
    {
        ErrorMessage = "Please select output item";
        return false;
    }
    
    if (Quantity <= 0)
    {
        ErrorMessage = "Output quantity must be greater than zero";
        return false;
    }
    
    if (InputItem.ItemId == SelectedItem.ItemId)
    {
        ErrorMessage = "Input and output items must be different";
        return false;
    }
    
    if (SelectedParty == null)
    {
        ErrorMessage = "Please select customer/party";
        return false;
    }
    
    if (PricePerUnit < 0)
    {
        ErrorMessage = "Processing fee cannot be negative";
        return false;
    }
    
    return true;
}
```

---

## User Workflow

### Processing New Transaction Flow

1. **Select Transaction Type:** "Processing"
2. **Select Customer/Party:** Choose the party bringing material
3. **Input Material Section:**
   - Select input item (e.g., "Paddy")
   - Enter input quantity (e.g., 100 kg)
4. **Output Material Section:**
   - Select output item (e.g., "Rice")
   - Enter output quantity (e.g., 70 kg)
   - System calculates conversion rate: 70%
5. **Processing Fee:**
   - Enter fee per unit (e.g., ₹5 per kg)
   - Total fee calculated: 70 kg × ₹5 = ₹350
6. **Additional Details:**
   - Transaction date
   - Notes
7. **Save Transaction**
   - Stock updated: Paddy -100 kg, Rice +70 kg
   - Revenue recorded: ₹350

### Dashboard Display
- **Processing Fees Card:** Shows total processing revenue
- **Recent Activities:** Lists processing transactions
- **Stock Impact:** Both input and output items reflected

### Reports
- **Filter by Processing Type:** View all processing transactions
- **Processing Summary:** 
  - Total processing fees earned
  - Material conversion rates
  - Customer-wise processing summary
  - Item-wise processing volume

---

## Stock Management Logic

### Processing Transaction Stock Flow

```
Initial State:
- Paddy: 50 kg
- Rice: 200 kg

Transaction: Process 100 kg Paddy → 70 kg Rice (Fee: ₹350)

Stock Updates:
1. Paddy: 50 - 100 = -50 kg (negative stock allowed for job work)
   OR
   Paddy: Receive 100 kg → Process 100 kg (two-step if preferred)
   
2. Rice: 200 + 70 = 270 kg

Final State:
- Paddy: -50 kg (or 0 if two-step)
- Rice: 270 kg
```

### Alternative: Two-Step Processing
For strict stock control, processing can be split:
1. **Receive Material:** Buy transaction (Paddy +100 kg)
2. **Process & Return:** Processing transaction (Paddy -100 kg, Rice +70 kg, Fee ₹350)

---

## Reporting & Analytics

### New Reports to Add

1. **Processing Summary Report**
   - Period: Date range
   - Metrics:
     - Total processing fees
     - Total input quantity
     - Total output quantity
     - Average conversion rate
     - Customer-wise breakdown

2. **Material Conversion Report**
   - Input item → Output item mapping
   - Conversion efficiency
   - Wastage/loss analysis

3. **Customer Processing History**
   - Filter by customer
   - Historical processing volumes
   - Fee trends

---

## Migration Strategy

### Backward Compatibility
- Existing transactions unaffected (nullable fields)
- No data loss
- Old reports continue to work

### Database Migration Script
```sql
-- Add new columns
ALTER TABLE Transactions ADD InputItemId INT NULL;
ALTER TABLE Transactions ADD InputQuantity DECIMAL(18,2) NULL;
ALTER TABLE Transactions ADD ConversionRate DECIMAL(5,4) NULL;

-- Add foreign key
ALTER TABLE Transactions 
ADD CONSTRAINT FK_Transactions_InputItem 
FOREIGN KEY (InputItemId) REFERENCES Items(ItemId);
```

---

## Testing Strategy

### Unit Tests
- ✅ TransactionType enum includes Processing
- ✅ Stock update logic for processing
- ✅ Validation rules
- ✅ Conversion rate calculations

### Integration Tests
- ✅ End-to-end processing transaction flow
- ✅ Stock updates across items
- ✅ Fee calculation
- ✅ Report generation

### UI Tests
- ✅ Processing form visibility
- ✅ Field validation
- ✅ Save functionality
- ✅ Dashboard metrics display

### Test Scenarios
1. **Simple Processing:** Paddy → Rice with fee
2. **Zero Fee Processing:** Free service
3. **Multiple Processing:** Same customer, different items
4. **Edit Processing Transaction:** Verify stock reversal
5. **Delete Processing Transaction:** Verify stock restoration
6. **Report Filtering:** Processing transactions only

---

## Performance Considerations

- **Stock Updates:** Two items affected per processing transaction
- **Report Queries:** Additional joins for input item
- **Indexing:** Add index on `InputItemId` if needed
- **Caching:** Cache conversion rates for common material pairs

---

## Security & Permissions

- Same user permissions as existing transactions
- Processing transactions require party selection (no anonymous processing)
- Audit trail maintained (CreatedDate, EnteredBy)

---

## Future Enhancements

1. **Conversion Templates:** Save common material conversions
2. **Batch Processing:** Process multiple materials in one transaction
3. **Quality Grading:** Track input/output quality grades
4. **Processing Time Tracking:** Record processing duration
5. **Cost Tracking:** Track processing costs separately from fees
6. **Wastage Recording:** Automatically calculate and record wastage

---

## Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Negative stock allowed | Medium | Add validation option to prevent negative stock |
| Complex UI | Low | Provide clear labels and help text |
| Data migration issues | Medium | Thorough testing before deployment |
| User confusion | Medium | Comprehensive training and documentation |
| Report performance | Low | Add indexes, optimize queries |

---

## Success Criteria

✅ Processing transaction type successfully added  
✅ Stock updates correctly for both input and output items  
✅ Processing fees tracked separately in dashboard  
✅ Reports show processing transactions with full details  
✅ All existing functionality remains intact  
✅ User can complete processing workflow intuitively  
✅ All tests passing (unit, integration, UI)  
✅ Documentation updated

---

## Timeline Estimate

- **Phase 1 (Database & Models):** 2-3 hours
- **Phase 2 (Services):** 3-4 hours
- **Phase 3 (ViewModels):** 4-5 hours
- **Phase 4 (UI):** 3-4 hours
- **Phase 5 (Testing):** 4-6 hours
- **Phase 6 (Documentation):** 2-3 hours

**Total Estimated Time:** 18-25 hours

---

## Approval & Next Steps

**Status:** ✅ Design Complete - Awaiting Approval

**Next Step:** Once approved, begin Phase 1 implementation (Database & Models)

**Questions/Clarifications:**
1. Should we allow negative stock for input materials?
2. Is two-step processing (receive then process) preferred?
3. What should be the default conversion rate display format?
4. Should processing fees be shown separately in profit calculations?

---

## Appendix

### Example Data

**Sample Processing Transaction:**
```json
{
    "TransactionId": 101,
    "TransactionType": "Processing",
    "PartyId": 5,
    "PartyName": "ABC Rice Mill Customer",
    "InputItemId": 10,
    "InputItem": "Paddy",
    "InputQuantity": 100.00,
    "ItemId": 20,
    "Item": "Rice",
    "Quantity": 70.00,
    "ConversionRate": 0.70,
    "PricePerUnit": 5.00,
    "TotalAmount": 350.00,
    "TransactionDate": "2025-12-26",
    "EnteredBy": 1,
    "Notes": "Premium quality paddy processing"
}
```

### SQL Queries for Reports

**Processing Summary:**
```sql
SELECT 
    t.TransactionDate,
    p.Name AS Customer,
    i_input.ItemName AS InputMaterial,
    t.InputQuantity,
    i_output.ItemName AS OutputMaterial,
    t.Quantity AS OutputQuantity,
    t.ConversionRate,
    t.PricePerUnit AS FeePerUnit,
    t.TotalAmount AS TotalFee
FROM Transactions t
INNER JOIN Parties p ON t.PartyId = p.PartyId
INNER JOIN Items i_input ON t.InputItemId = i_input.ItemId
INNER JOIN Items i_output ON t.ItemId = i_output.ItemId
WHERE t.TransactionType = 'Processing'
    AND t.TransactionDate BETWEEN @StartDate AND @EndDate
ORDER BY t.TransactionDate DESC;
```

---

**Document End**
