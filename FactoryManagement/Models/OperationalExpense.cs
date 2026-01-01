using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    /// <summary>
    /// Represents operational expenses like fuel, repairs, utilities, etc.
    /// </summary>
    public class OperationalExpense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperationalExpenseId { get; set; }

        [Required]
        public int ExpenseCategoryId { get; set; }

        [ForeignKey(nameof(ExpenseCategoryId))]
        public virtual ExpenseCategory? ExpenseCategory { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        /// <summary>
        /// User who spent the money (e.g., for fuel, cab charges)
        /// </summary>
        public int? SpentBy { get; set; }

        [ForeignKey(nameof(SpentBy))]
        public virtual User? SpentByUser { get; set; }

        /// <summary>
        /// Payment method used
        /// </summary>
        [Required]
        public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;

        /// <summary>
        /// Optional: Link to Item (for machinery purchase, etc.)
        /// </summary>
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        /// <summary>
        /// Approval status (for future workflow enhancement)
        /// </summary>
        public bool IsApproved { get; set; } = true;

        public int? ApprovedBy { get; set; }

        [ForeignKey(nameof(ApprovedBy))]
        public virtual User? Approver { get; set; }

        [Required]
        public int EnteredBy { get; set; }

        [ForeignKey(nameof(EnteredBy))]
        public virtual User? User { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Path to attached receipt/invoice file (future enhancement)
        /// </summary>
        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        /// <summary>
        /// Computed property for display
        /// </summary>
        [NotMapped]
        public string CategoryDisplay => ExpenseCategory?.CategoryName ?? $"Category ID: {ExpenseCategoryId}";

        /// <summary>
        /// Computed property for spent by user display
        /// </summary>
        [NotMapped]
        public string SpentByDisplay => SpentByUser?.Username ?? "N/A";

        /// <summary>
        /// All expenses are debits (money going out)
        /// </summary>
        [NotMapped]
        public string DebitCredit => "Debit";
    }
}
