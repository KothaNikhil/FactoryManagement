using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    public enum TransactionType
    {
        Buy,
        Sell,
        Wastage,
        Processing
    }

    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        public int? PartyId { get; set; }

        [ForeignKey(nameof(PartyId))]
        public virtual Party? Party { get; set; }

        [MaxLength(200)]
        public string PartyName { get; set; } = string.Empty;

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public decimal PricePerUnit { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        // Cash/Bank mode for payment
        [Required]
        public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        public int EnteredBy { get; set; }

        [ForeignKey(nameof(EnteredBy))]
        public virtual User? User { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Processing-specific fields
        public int? InputItemId { get; set; }

        [ForeignKey(nameof(InputItemId))]
        public virtual Item? InputItem { get; set; }

        public decimal? InputQuantity { get; set; }

        public decimal? ConversionRate { get; set; }

        // Computed property for Debit/Credit indicator
        [NotMapped]
        public string DebitCredit
        {
            get
            {
                // Loan transactions don't create cash debit/credit in the transaction ledger
                // The financial loan entry handles the accounting
                if (PaymentMode == PaymentMode.Loan)
                {
                    return "-"; // No cash impact for any loan-based transaction
                }

                // Normal cash/bank transactions
                return (TransactionType == TransactionType.Sell || TransactionType == TransactionType.Processing)
                    ? "Credit" : "Debit";
            }
        }
    }
}
