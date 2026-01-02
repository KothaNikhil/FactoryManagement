using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    public enum BalanceChangeType
    {
        OpeningBalance = 0,
        Transaction = 1,
        ManualAdjustment = 2,
        Transfer = 3
    }

    public class BalanceHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BalanceHistoryId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public virtual CashAccount? Account { get; set; }

        [Required]
        public BalanceChangeType ChangeType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ChangeAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NewBalance { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Link to related transaction
        public int? TransactionId { get; set; }
        public int? FinancialTransactionId { get; set; }
        public int? WageTransactionId { get; set; }
        public int? OperationalExpenseId { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public int EnteredBy { get; set; }

        [ForeignKey(nameof(EnteredBy))]
        public virtual User? User { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
