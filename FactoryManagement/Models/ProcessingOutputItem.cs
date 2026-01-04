using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    /// <summary>
    /// Represents an output item from a processing transaction
    /// </summary>
    public class ProcessingOutputItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProcessingOutputId { get; set; }

        [Required]
        public int TransactionId { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public virtual Transaction? Transaction { get; set; }

        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public decimal Quantity { get; set; }

        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;
    }
}
