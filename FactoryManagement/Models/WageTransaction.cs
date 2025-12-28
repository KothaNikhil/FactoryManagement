using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    public enum WageTransactionType
    {
        DailyWage,          // Regular daily wage payment
        HourlyWage,         // Hourly wage payment
        MonthlyWage,        // Monthly salary payment
        OvertimePay,        // Overtime payment
        Bonus,              // Bonus payment
        AdvanceGiven,       // Advance given to worker
        AdvanceAdjustment,  // Advance deducted from wage
        Deduction           // Other deductions (penalty, etc.)
    }

    public class WageTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WageTransactionId { get; set; }

        [Required]
        public int WorkerId { get; set; }

        [ForeignKey(nameof(WorkerId))]
        public virtual Worker? Worker { get; set; }

        [Required]
        public WageTransactionType TransactionType { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // For daily/hourly workers
        public decimal? DaysWorked { get; set; }
        public decimal? HoursWorked { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }

        // Calculated as: (DaysWorked * DailyRate) or (HoursWorked * HourlyRate)
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Overtime hours and rate
        public decimal? OvertimeHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OvertimeRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OvertimeAmount { get; set; }

        // Advance adjustment
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AdvanceAdjusted { get; set; }

        // Other deductions
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Deductions { get; set; }

        // Net amount paid = Amount + OvertimeAmount - AdvanceAdjusted - Deductions
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }
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
        public string DebitCredit => TransactionType == WageTransactionType.AdvanceAdjustment ||
                                     TransactionType == WageTransactionType.Deduction
                                     ? "Credit" : "Debit";
    }
}
