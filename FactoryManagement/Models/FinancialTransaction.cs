using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    public enum FinancialTransactionType
    {
        LoanGiven,      // Money lent to a party
        LoanTaken,      // Money borrowed from a party
        LoanRepayment,  // Repayment received for loan given
        LoanPayment,    // Payment made for loan taken
        InterestReceived, // Interest received on loan given
        InterestPaid    // Interest paid on loan taken
    }

    public enum PaymentStatus
    {
        Pending,
        Partial,
        Completed,
        Overdue
    }

    public class FinancialTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FinancialTransactionId { get; set; }

        public int? PartyId { get; set; }

        [ForeignKey(nameof(PartyId))]
        public virtual Party? Party { get; set; }

        [MaxLength(200)]
        public string PartyName { get; set; } = string.Empty;

        [Required]
        public FinancialTransactionType TransactionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Cash/Bank mode for payment
        [Required]
        public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? InterestRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InterestAmount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }

        // Links to parent loan for repayments and interest transactions
        public int? LinkedLoanAccountId { get; set; }

        [ForeignKey(nameof(LinkedLoanAccountId))]
        public virtual LoanAccount? LinkedLoanAccount { get; set; }

        [Required]
        public int EnteredBy { get; set; }

        [ForeignKey(nameof(EnteredBy))]
        public virtual User? User { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        // Computed property for Debit/Credit indicator
        [NotMapped]
        public string DebitCredit => TransactionType == FinancialTransactionType.LoanGiven ||
                                     TransactionType == FinancialTransactionType.LoanPayment ||
                                     TransactionType == FinancialTransactionType.InterestPaid
                                     ? "Debit" : "Credit";
    }
}
