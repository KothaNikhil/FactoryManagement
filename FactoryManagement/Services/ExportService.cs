using CsvHelper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                try
                {
                    var dataList = data?.ToList() ?? new List<T>();
                    Debug.WriteLine($"[ExportService] Exporting {dataList.Count} rows to Excel: {filePath}");
                    
                    if (dataList.Count == 0)
                    {
                        Debug.WriteLine($"[ExportService] WARNING: No data to export!");
                    }
                    
                    using var package = new ExcelPackage();
                    var worksheet = package.Workbook.Worksheets.Add(sheetName);

                    // Load data - ensure we pass the materialized list
                    worksheet.Cells["A1"].LoadFromCollection(dataList, true);

                    // Format header and data only if we have data
                    if (worksheet.Dimension != null)
                    {
                        Debug.WriteLine($"[ExportService] Worksheet dimension: {worksheet.Dimension.Address}");
                        using (var range = worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }

                        // Auto-fit columns
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    }
                    else
                    {
                        Debug.WriteLine($"[ExportService] WARNING: Worksheet.Dimension is null after loading data!");
                    }

                    // Save
                    var file = new FileInfo(filePath);
                    package.SaveAs(file);
                    Debug.WriteLine($"[ExportService] Excel file saved successfully: {filePath}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ExportService] ERROR in ExportToExcelAsync: {ex.Message}");
                    Debug.WriteLine($"[ExportService] Stack trace: {ex.StackTrace}");
                    throw;
                }
            });
        }

        public async Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    var dataList = data?.ToList() ?? new List<T>();
                    Debug.WriteLine($"[ExportService] Exporting {dataList.Count} rows to CSV: {filePath}");
                    
                    if (dataList.Count == 0)
                    {
                        Debug.WriteLine($"[ExportService] WARNING: No data to export!");
                    }
                    
                    using var writer = new StreamWriter(filePath);
                    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csv.WriteRecords(dataList);
                    Debug.WriteLine($"[ExportService] CSV file saved successfully: {filePath}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ExportService] ERROR in ExportToCsvAsync: {ex.Message}");
                    Debug.WriteLine($"[ExportService] Stack trace: {ex.StackTrace}");
                    throw;
                }
            });
        }
    }
}
