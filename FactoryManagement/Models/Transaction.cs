using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    public enum TransactionType
    {
        Buy,
        Sell,
        Wastage
    }

    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        public int? PartyId { get; set; }

        [ForeignKey(nameof(PartyId))]
        public virtual Party? Party { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public decimal PricePerUnit { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        public int EnteredBy { get; set; }

        [ForeignKey(nameof(EnteredBy))]
        public virtual User? User { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
