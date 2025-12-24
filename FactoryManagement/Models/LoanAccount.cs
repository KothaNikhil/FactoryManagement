using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    public enum LoanType
    {
        Given,  // Money you lent out
        Taken   // Money you borrowed
    }

    public enum LoanStatus
    {
        Active,
        Closed,
        Overdue,
        PartiallyPaid
    }

    public class LoanAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoanAccountId { get; set; }

        [Required]
        public int PartyId { get; set; }

        [ForeignKey(nameof(PartyId))]
        public virtual Party? Party { get; set; }

        [Required]
        public LoanType LoanType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingPrincipal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingInterest { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalOutstanding { get; set; }

        [Required]
        public LoanStatus Status { get; set; } = LoanStatus.Active;

        [Required]
        public int CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? User { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        // Navigation property for related transactions
        public virtual ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    }
}
