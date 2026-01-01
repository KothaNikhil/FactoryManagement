using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    /// <summary>
    /// Represents a user-manageable expense category for operational expenses
    /// </summary>
    public class ExpenseCategory
    {
        /// <summary>
        /// Gets or sets the unique identifier for the expense category.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExpenseCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name of the expense category.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional detailed description of the expense category.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who created this expense category.
        /// </summary>
        [Required]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the user who created this expense category.
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual User? Creator { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this expense category was created.
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the date and time when this expense category was last modified, if any changes were made.
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets whether this category has been soft-deleted. Soft-deleted categories are hidden but retained for referential integrity.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the collection of operational expenses associated with this category.
        /// </summary>
        // Navigation property
        public virtual ICollection<OperationalExpense>? OperationalExpenses { get; set; }
    }
}
