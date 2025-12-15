using CsvHelper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FactoryManagement.Models;

namespace FactoryManagement.Services
{
    public interface IExportService
    {
        Task ExportToExcelAsync<T>(IEnumerable<T> data, string filePath, string sheetName = "Sheet1");
        Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath);
    }

    public class ExportService : IExportService
    {
        public ExportService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task ExportToExcelAsync<T>(IEnumerable<T> data, string filePath, string sheetName = "Sheet1")
        {
            await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                // Load data
                worksheet.Cells["A1"].LoadFromCollection(data, true);

                // Format header
                using (var range = worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Save
                var file = new FileInfo(filePath);
                package.SaveAs(file);
            });
        }

        public async Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath)
        {
            await Task.Run(() =>
            {
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(data);
            });
        }
    }
}
