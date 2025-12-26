using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    public enum PartyType
    {
        Buyer,
        Seller,
        Both,
        Lender,         // Party who lends money to you
        Borrower,       // Party who borrows money from you
        Financial,      // Party involved in both lending and borrowing
        Processor       // Customer who brings material for processing (job work)
    }

    public class Party
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PartyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string MobileNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Place { get; set; } = string.Empty;

        [Required]
        public PartyType PartyType { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? ModifiedDate { get; set; }
    }
}
