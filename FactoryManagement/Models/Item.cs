using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FactoryManagement.Models
{
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public decimal CurrentStock { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;

        public int? CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedBy { get; set; }

        public int? ModifiedByUserId { get; set; }

        [ForeignKey(nameof(ModifiedByUserId))]
        public virtual User? ModifiedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Collection of package distributions for this item
        /// </summary>
        public virtual ICollection<StockPackage> StockPackages { get; set; } = new List<StockPackage>();

        /// <summary>
        /// Indicates if this item has package distribution
        /// </summary>
        [NotMapped]
        public bool IsPackaged => StockPackages?.Any() ?? false;

        /// <summary>
        /// Display text showing total stock and package breakdown
        /// </summary>
        [NotMapped]
        public string StockDisplayText
        {
            get
            {
                var packages = StockPackages?.Where(p => p.PackageCount > 0).ToList();
                if (!packages?.Any() ?? true)
                    return $"{CurrentStock:N2} kg (Loose)";
                
                var breakdown = string.Join(", ", 
                    packages.OrderBy(p => p.PackageSize).Select(p => $"{p.PackageCount}×{p.PackageSize}kg"));
                return $"{CurrentStock:N2} kg ({breakdown})";
            }
        }

        public override string ToString()
        {
            return ItemName ?? "Unnamed Item";
        }
    }
}
