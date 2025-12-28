using System;

namespace FactoryManagement.Models
{
    public class ReportExportRow
    {
        public string Category { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string WorkerName { get; set; } = string.Empty;

        public decimal? Quantity { get; set; }
        public decimal? Rate { get; set; }

        public decimal Amount { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }

        public decimal? InterestRate { get; set; }
        public decimal? InterestAmount { get; set; }
        public DateTime? DueDate { get; set; }

        public string EnteredBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}