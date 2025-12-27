using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    public enum WorkerStatus
    {
        Active,
        Inactive,
        OnLeave,
        Terminated
    }

    public class Worker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string MobileNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public WorkerStatus Status { get; set; } = WorkerStatus.Active;

        [Column(TypeName = "decimal(18,2)")]
        // Primary rate field used by new code. For existing logic, the effective rate is typically
        // resolved using fallback logic in the ViewModel, e.g.: Rate > 0 ? Rate : DailyRate.
        // When creating or updating workers, prefer setting Rate and avoid writing to the
        // legacy fields below unless explicitly dealing with migrated/legacy data.
        public decimal Rate { get; set; }

        // Legacy fields for backward compatibility (deprecated). These were used before the
        // unified Rate property was introduced and are kept only to support existing data and
        // older parts of the application. New code should read/write the Rate property and
        // treat DailyRate/HourlyRate/MonthlyRate as read-only legacy values.
        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRate { get; set; }

        // Track total advances given
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAdvance { get; set; }

        // Track total wages paid
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalWagesPaid { get; set; }

        public int? CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        public int? ModifiedByUserId { get; set; }

        [ForeignKey(nameof(ModifiedByUserId))]
        public virtual User? ModifiedBy { get; set; }

        public DateTime JoiningDate { get; set; } = DateTime.Now;

        public DateTime? LeavingDate { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        public override string ToString()
        {
            return Name ?? "Unnamed Worker";
        }

        // Navigation property
        public virtual ICollection<WageTransaction> WageTransactions { get; set; } = new List<WageTransaction>();
    }
}
