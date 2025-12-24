using System;

namespace FactoryManagement.Models
{
    public class RecentActivity
    {
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty; // Transaction, Financial, Wage
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Party { get; set; }
        public decimal Amount { get; set; }
    }
}
