using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    /// <summary>
    /// Represents daily cash balance tracking and reconciliation
    /// </summary>
    public class CashBalance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CashBalanceId { get; set; }

        /// <summary>
        /// Date for this cash balance record (one record per day)
        /// </summary>
        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        /// <summary>
        /// Opening balance at start of day (carried forward from previous day's closing)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Expected closing balance based on opening + credits - debits
        /// Calculated automatically from transactions
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedClosingBalance { get; set; }

        /// <summary>
        /// Actual cash counted at end of day
        /// Null if reconciliation not yet performed
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualCashCounted { get; set; }

        /// <summary>
        /// Difference between expected and actual cash (Actual - Expected)
        /// Positive = Cash surplus, Negative = Cash shortage
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Discrepancy { get; set; }

        /// <summary>
        /// Whether reconciliation has been performed for this day
        /// </summary>
        public bool IsReconciled { get; set; } = false;

        /// <summary>
        /// User who performed the reconciliation
        /// </summary>
        public int? ReconciledBy { get; set; }

        [ForeignKey(nameof(ReconciledBy))]
        public virtual User? ReconciledByUser { get; set; }

        /// <summary>
        /// When reconciliation was performed
        /// </summary>
        public DateTime? ReconciledDate { get; set; }

        /// <summary>
        /// Notes/remarks for this day's cash handling
        /// Could include reasons for discrepancies, special transactions, etc.
        /// </summary>
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Reason for discrepancy if any
        /// </summary>
        [MaxLength(500)]
        public string? DiscrepancyReason { get; set; }

        /// <summary>
        /// Total cash inflow for the day (from transactions)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCashIn { get; set; }

        /// <summary>
        /// Total cash outflow for the day (from transactions)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCashOut { get; set; }

        /// <summary>
        /// Record creation timestamp
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Last modified timestamp
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Computed property for status display
        /// </summary>
        [NotMapped]
        public string Status
        {
            get
            {
                if (!IsReconciled)
                    return "Pending";
                if (Discrepancy == 0)
                    return "Balanced";
                if (Discrepancy > 0)
                    return "Surplus";
                return "Shortage";
            }
        }

        /// <summary>
        /// Computed property for discrepancy display text
        /// </summary>
        [NotMapped]
        public string DiscrepancyText
        {
            get
            {
                if (!Discrepancy.HasValue)
                    return "-";
                if (Discrepancy == 0)
                    return "Balanced";
                if (Discrepancy > 0)
                    return $"Surplus: ₹{Math.Abs(Discrepancy.Value):N2}";
                return $"Shortage: ₹{Math.Abs(Discrepancy.Value):N2}";
            }
        }
    }

    /// <summary>
    /// DTO for cash flow summary
    /// </summary>
    public class CashFlowSummary
    {
        public decimal OpeningBalance { get; set; }
        public decimal TotalCashIn { get; set; }
        public decimal TotalCashOut { get; set; }
        public decimal ExpectedClosingBalance { get; set; }
        public decimal NetFlow => TotalCashIn - TotalCashOut;
    }

    /// <summary>
    /// DTO for cash adjustment entries (opening balance, corrections)
    /// </summary>
    public class CashAdjustment
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string AdjustmentType { get; set; } = "Opening Balance"; // Opening Balance, Correction, Other
        public int EnteredBy { get; set; }
    }
}
