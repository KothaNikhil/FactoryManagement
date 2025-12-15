using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryManagement.Models
{
    public class AppSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = "Factory Management System";

        [Required]
        [MaxLength(10)]
        public string CurrencySymbol { get; set; } = "₹";

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public DateTime? ModifiedDate { get; set; }
    }
}
