using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FactoryManagement.Models;

namespace FactoryManagement.Services
{
    public interface IReportExportBuilder
    {
        Task<List<ReportExportRow>> BuildExportRowsAsync(
            ViewModels.ReportType selectedType,
            IEnumerable<Transaction> inventory,
            IEnumerable<FinancialTransaction> financial,
            IEnumerable<WageTransaction> wages,
            IEnumerable<UnifiedTransactionViewModel> unified);
    }

    public class ReportExportBuilder : IReportExportBuilder
    {
        public Task<List<ReportExportRow>> BuildExportRowsAsync(
            ViewModels.ReportType selectedType,
            IEnumerable<Transaction> inventory,
            IEnumerable<FinancialTransaction> financial,
            IEnumerable<WageTransaction> wages,
            IEnumerable<UnifiedTransactionViewModel> unified)
        {
            List<ReportExportRow> rows;

            switch (selectedType)
            {
                case ViewModels.ReportType.All:
                    rows = unified.Select(t => new ReportExportRow
                    {
                        Category = t.Category,
                        TransactionId = t.TransactionId,
                        ItemName = t.ItemName ?? string.Empty,
                        PartyName = t.PartyName ?? string.Empty,
                        WorkerName = t.WorkerName ?? string.Empty,
                        TransactionType = t.TransactionType,
                        Quantity = t.Quantity,
                        Rate = t.Rate,
                        Amount = t.Amount,
                        DebitAmount = t.DebitCredit == "Debit" ? t.Amount : null,
                        CreditAmount = t.DebitCredit == "Credit" ? t.Amount : null,
                        TransactionDate = t.TransactionDate,
                        Notes = t.Notes ?? string.Empty,
                        EnteredBy = t.EnteredBy ?? string.Empty
                    }).OrderByDescending(r => r.TransactionDate).ToList();
                    break;

                case ViewModels.ReportType.Inventory:
                    rows = inventory.Select(t => new ReportExportRow
                    {
                        Category = "Inventory",
                        TransactionId = t.TransactionId.ToString(),
                        ItemName = t.ItemName ?? string.Empty,
                        PartyName = t.PartyName ?? string.Empty,
                        TransactionType = t.TransactionType.ToString(),
                        Quantity = t.Quantity,
                        Rate = t.PricePerUnit,
                        Amount = t.TotalAmount,
                        DebitAmount = t.DebitCredit == "Debit" ? t.TotalAmount : null,
                        CreditAmount = t.DebitCredit == "Credit" ? t.TotalAmount : null,
                        TransactionDate = t.TransactionDate,
                        Notes = t.Notes ?? string.Empty,
                        EnteredBy = t.User?.Username ?? string.Empty
                    }).OrderByDescending(r => r.TransactionDate).ToList();
                    break;

                case ViewModels.ReportType.Financial:
                    rows = financial.Select(t => new ReportExportRow
                    {
                        Category = "Financial",
                        TransactionId = t.FinancialTransactionId.ToString(),
                        PartyName = t.Party?.Name ?? string.Empty,
                        TransactionType = t.TransactionType.ToString(),
                        Amount = t.Amount,
                        DebitAmount = t.DebitCredit == "Debit" ? t.Amount : null,
                        CreditAmount = t.DebitCredit == "Credit" ? t.Amount : null,
                        InterestRate = t.InterestRate,
                        InterestAmount = t.InterestAmount,
                        TransactionDate = t.TransactionDate,
                        DueDate = t.DueDate,
                        Notes = t.Notes ?? string.Empty,
                        EnteredBy = t.User?.Username ?? string.Empty
                    }).OrderByDescending(r => r.TransactionDate).ToList();
                    break;

                case ViewModels.ReportType.Wages:
                default:
                    rows = wages.Select(t => new ReportExportRow
                    {
                        Category = "Wages",
                        TransactionId = t.WageTransactionId.ToString(),
                        WorkerName = t.Worker?.Name ?? string.Empty,
                        TransactionType = t.TransactionType.ToString(),
                        Quantity = t.DaysWorked ?? t.HoursWorked,
                        Rate = t.Rate,
                        Amount = t.NetAmount,
                        DebitAmount = t.DebitCredit == "Debit" ? t.NetAmount : null,
                        CreditAmount = t.DebitCredit == "Credit" ? t.NetAmount : null,
                        TransactionDate = t.TransactionDate,
                        Notes = t.Notes ?? string.Empty,
                        EnteredBy = t.User?.Username ?? string.Empty
                    }).OrderByDescending(r => r.TransactionDate).ToList();
                    break;
            }

            return Task.FromResult(rows);
        }
    }
}
