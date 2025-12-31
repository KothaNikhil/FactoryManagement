using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    /// <summary>
    /// Represents a package/bag distribution of an item.
    /// Example: 25kg bags, 50kg bags, etc.
    /// </summary>
    public class StockPackage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StockPackageId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; } = null!;

        /// <summary>
        /// Size of each package/bag (e.g., 25, 50)
        /// </summary>
        [Required]
        public decimal PackageSize { get; set; }

        /// <summary>
        /// Number of packages/bags
        /// </summary>
        [Required]
        public int PackageCount { get; set; }

        /// <summary>
        /// Calculated total quantity: PackageSize * PackageCount
        /// </summary>
        [NotMapped]
        public decimal TotalQuantity => PackageSize * PackageCount;

        /// <summary>
        /// Optional storage location (e.g., "Warehouse A, Shelf 3")
        /// </summary>
        [MaxLength(100)]
        public string? Location { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        public int? CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        public int? ModifiedByUserId { get; set; }

        [ForeignKey(nameof(ModifiedByUserId))]
        public virtual User? ModifiedBy { get; set; }
    }
}
