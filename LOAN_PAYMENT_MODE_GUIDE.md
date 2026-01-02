# Loan Payment Mode - Feature Guide

## Overview

The **Loan** payment mode allows you to record inventory transactions (purchases/sales) on credit without immediate cash payment. When you use Loan payment mode, the system automatically creates a linked loan entry in the Financial Records module.

---

## How It Works

### Buy Transaction with Loan Payment Mode

**Scenario**: You buy ₹10,000 worth of raw materials from "ABC Supplier" on credit.

**Steps**:
1. Go to **New Transaction**
2. Select **Buy** as transaction type
3. Choose **Item** and **Party** (ABC Supplier)
4. Enter **Quantity** and **Price Per Unit**
5. Select **Payment Mode: Loan**
6. Click **SAVE TRANSACTION**

**What Happens**:
1. **Inventory Transaction Created**:
   - Type: Buy
   - Amount: ₹10,000
   - DebitCredit: "-" (no cash impact)
   - Stock increases by quantity purchased

2. **Financial Loan Auto-Created**:
   - Type: LoanTaken (you owe money)
   - Amount: ₹10,000
   - Interest Rate: 0%
   - Status: Active
   - DebitCredit: Credit (liability)
   - Notes: "Auto-created from Buy transaction #123 for Raw Material"

3. **Transactions Linked**:
   - Both transactions reference each other
   - Full traceability maintained

### Sell Transaction with Loan Payment Mode

**Scenario**: You sell ₹15,000 worth of products to "XYZ Customer" on credit.

**Steps**:
1. Select **Sell** as transaction type
2. Choose **Item** and **Party** (XYZ Customer)
3. Enter **Quantity** and **Price Per Unit**
4. Select **Payment Mode: Loan**
5. Click **SAVE TRANSACTION**

**What Happens**:
1. **Inventory Transaction Created**:
   - Type: Sell
   - Amount: ₹15,000
   - DebitCredit: "-" (no cash impact)
   - Stock decreases by quantity sold

2. **Financial Loan Auto-Created**:
   - Type: LoanGiven (you're owed money)
   - Amount: ₹15,000
   - Interest Rate: 0%
   - Status: Active
   - DebitCredit: Debit (asset)
   - Notes: "Auto-created from Sell transaction #124 for Finished Goods"

---

## In Reports

### All Transactions Report
- **Inventory transactions (Loan mode)**: Show "-" in Debit/Credit columns
- **Financial loans**: Show as Debit (LoanGiven) or Credit (LoanTaken)
- **Total calculations**: Only financial loans affect Debit/Credit totals

### Inventory Report
- Loan-based transactions display with:
  - Payment Mode: Loan
  - Debit Amount: Blank
  - Credit Amount: Blank
  - DebitCredit: "-"
- Not included in cash flow totals

### Financial Report
- Auto-created loans appear with notes referencing original transaction
- Can be managed like any other loan (payments, interest, etc.)

---

## Key Benefits

✅ **No Double Entry**: Inventory shows no cash impact; only loan affects financial totals  
✅ **Automatic Linking**: Transactions connected for audit trail  
✅ **Proper Accounting**: Separates inventory movement from cash flow  
✅ **Convenience**: One action creates both records  
✅ **Traceability**: Easy to find related transactions via notes and links  

---

## Accounting Treatment

### Traditional (Cash/Bank Payment):
```
Buy Item (Cash):
  Debit: Inventory ₹10,000
  Credit: Cash ₹10,000
```

### With Loan Payment Mode:
```
Buy Item (Loan):
  Inventory increases: 10,000 units (tracked, no cash impact)
  
Financial Loan (Auto-created):
  Credit: Accounts Payable ₹10,000 (liability)
```

**Result**: Inventory tracked separately; only the liability affects cash flow reports.

---

## Managing Auto-Created Loans

Once created, auto-created loans can be managed in **Financial Records**:

### Record Payment
1. Go to **Financial Records**
2. Select the loan (look for auto-created note)
3. Enter payment amount
4. Click **RECORD PAYMENT**
5. Loan status updates (PartiallyPaid/Closed)

### Update Interest
- Even though default rate is 0%, you can manually update interest if needed
- Click **UPDATE INTEREST** on the loan

### View Linked Transaction
- Check loan notes for transaction reference
- Use Transaction ID to find original inventory transaction

---

## Best Practices

### When to Use Loan Payment Mode
✅ Credit purchases from suppliers  
✅ Credit sales to customers  
✅ Deferred payment arrangements  
✅ Trade credit transactions  

### When NOT to Use Loan Payment Mode
❌ Immediate cash/bank payments  
❌ When you want to manually track loans separately  
❌ Transactions without parties (Wastage)  

### Tips
- Add detailed notes to help identify transactions later
- Use consistent party names to group related loans
- Regularly reconcile auto-created loans with actual obligations
- Mark loans as paid when settling credit accounts

---

## Example Workflow

**Month Start**:
1. Buy ₹50,000 raw material (Loan mode) → LoanTaken created
2. Sell ₹80,000 products (Loan mode) → LoanGiven created

**Reports Show**:
- Inventory: Both transactions with "-" (no cash impact)
- Financial: ₹50,000 Credit (owed) + ₹80,000 Debit (receivable)
- Net Position: ₹30,000 receivable

**Month End**:
1. Supplier asks for payment → Record ₹50,000 payment on LoanTaken
2. Customer pays you → Record ₹80,000 payment on LoanGiven
3. Both loans closed automatically

**Final Position**:
- Inventory tracked accurately
- Cash flow reflects actual payments
- All transactions linked and traceable

---

## FAQ

**Q: Can I edit a transaction after it's created with Loan mode?**  
A: Yes, but changing payment mode won't retroactively create/delete the loan. Handle loans separately in Financial Records.

**Q: What if I delete an inventory transaction that created a loan?**  
A: The loan remains in Financial Records. Delete it manually if needed.

**Q: Can I add interest to auto-created loans?**  
A: Yes, go to Financial Records and update the loan's interest rate.

**Q: How do I find the inventory transaction from a loan?**  
A: Check the loan's Notes field - it includes the transaction ID.

**Q: Does Loan payment mode work for Processing/Wastage?**  
A: Wastage typically doesn't need a party, so Loan mode isn't applicable. Processing uses Loan mode like Buy/Sell.

---

## Summary

| Payment Mode | Cash Impact | Financial Entry | Use Case |
|--------------|-------------|-----------------|----------|
| **Cash** | Yes - Immediate | None | Direct cash payment |
| **Bank** | Yes - Immediate | None | Bank transfer payment |
| **Loan** | No | Auto-created loan | Credit purchase/sale |

The Loan payment mode bridges inventory management and financial tracking, ensuring accurate accounting without double-counting!
