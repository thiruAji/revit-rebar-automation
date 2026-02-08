using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace RebarAutomation.RevitIntegration
{
    /// <summary>
    /// Generates Bar Bending Schedule from rebar elements
    /// </summary>
    public class BBSGenerator
    {
        private readonly Document _doc;

        public BBSGenerator(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// Generates BBS for all rebar in the model
        /// </summary>
        public string GenerateBBS(string outputPath)
        {
            // Extract rebar data
            var rebarData = ExtractRebarData();

            if (rebarData.Count == 0)
            {
                throw new Exception("No rebar found in the model");
            }

            // Export to Excel
            string filePath = ExportToExcel(rebarData, outputPath);

            return filePath;
        }

        /// <summary>
        /// Generates BBS for specific rebar elements
        /// </summary>
        public string GenerateBBS(List<Rebar> rebarList, string outputPath)
        {
            var rebarData = new List<BBSEntry>();

            foreach (var rebar in rebarList)
            {
                var entry = CreateBBSEntry(rebar);
                if (entry != null)
                {
                    rebarData.Add(entry);
                }
            }

            string filePath = ExportToExcel(rebarData, outputPath);
            return filePath;
        }

        /// <summary>
        /// Extracts rebar data from all rebar elements in the model
        /// </summary>
        private List<BBSEntry> ExtractRebarData()
        {
            var rebarData = new List<BBSEntry>();

            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            var rebarElements = collector.OfClass(typeof(Rebar)).Cast<Rebar>();

            foreach (var rebar in rebarElements)
            {
                var entry = CreateBBSEntry(rebar);
                if (entry != null)
                {
                    rebarData.Add(entry);
                }
            }

            return rebarData;
        }

        /// <summary>
        /// Creates a BBS entry from a rebar element
        /// </summary>
        private BBSEntry CreateBBSEntry(Rebar rebar)
        {
            var entry = new BBSEntry();

            // Get bar mark
            Parameter markParam = rebar.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
            entry.Mark = markParam?.AsString() ?? "?";

            // Get bar diameter
            RebarBarType barType = _doc.GetElement(rebar.GetTypeId()) as RebarBarType;
            if (barType != null)
            {
                Parameter diamParam = barType.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
                entry.Diameter = (int)Math.Round(diamParam.AsDouble() * 304.8); // Convert feet to mm
            }

            // Get bar shape
            RebarShape shape = _doc.GetElement(rebar.RebarShapeId) as RebarShape;
            entry.Shape = shape?.Name ?? "Straight";

            // Calculate bar length
            entry.Length = CalculateBarLength(rebar);

            // Get number of bars
            entry.Number = rebar.NumberOfBarPositions;

            // Calculate total length
            entry.TotalLength = entry.Length * entry.Number;

            // Calculate weight (density of steel = 7850 kg/m³)
            double area = Math.PI * entry.Diameter * entry.Diameter / 4.0; // mm²
            entry.Weight = (area / 1000000.0) * (entry.TotalLength / 1000.0) * 7850; // kg

            // Get spacing
            Parameter commentParam = rebar.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            entry.Spacing = commentParam?.AsString() ?? "";

            return entry;
        }

        /// <summary>
        /// Calculates cutting length of a single bar including bends
        /// </summary>
        private double CalculateBarLength(Rebar rebar)
        {
            double totalLength = 0;

            // Get all curves in the rebar
            var curves = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, 0);

            foreach (Curve curve in curves)
            {
                totalLength += curve.Length;
            }

            // Convert from feet to mm
            totalLength *= 304.8;

            // Add hook lengths if present
            RebarHookType startHook = _doc.GetElement(rebar.GetHookTypeId(0)) as RebarHookType;
            RebarHookType endHook = _doc.GetElement(rebar.GetHookTypeId(1)) as RebarHookType;

            if (startHook != null)
            {
                Parameter hookLength = startHook.get_Parameter(BuiltInParameter.REBAR_HOOK_LENGTH);
                if (hookLength != null)
                {
                    totalLength += hookLength.AsDouble() * 304.8;
                }
            }

            if (endHook != null)
            {
                Parameter hookLength = endHook.get_Parameter(BuiltInParameter.REBAR_HOOK_LENGTH);
                if (hookLength != null)
                {
                    totalLength += hookLength.AsDouble() * 304.8;
                }
            }

            return Math.Round(totalLength, 0);
        }

        /// <summary>
        /// Exports BBS data to Excel file
        /// </summary>
        private string ExportToExcel(List<BBSEntry> data, string outputPath)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Create output directory if it doesn't exist
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Generate filename with timestamp
            string fileName = $"BBS_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            string filePath = Path.Combine(directory, fileName);

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Bar Bending Schedule");

                // Add title
                worksheet.Cells["A1:H1"].Merge = true;
                worksheet.Cells["A1"].Value = "BAR BENDING SCHEDULE";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Add project info
                worksheet.Cells["A2"].Value = $"Project: {_doc.ProjectInformation.Name}";
                worksheet.Cells["A3"].Value = $"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}";

                // Add headers
                int headerRow = 5;
                worksheet.Cells[headerRow, 1].Value = "Mark";
                worksheet.Cells[headerRow, 2].Value = "Diameter (mm)";
                worksheet.Cells[headerRow, 3].Value = "Shape";
                worksheet.Cells[headerRow, 4].Value = "Length (mm)";
                worksheet.Cells[headerRow, 5].Value = "Number";
                worksheet.Cells[headerRow, 6].Value = "Total Length (mm)";
                worksheet.Cells[headerRow, 7].Value = "Weight (kg)";
                worksheet.Cells[headerRow, 8].Value = "Spacing";

                // Style headers
                using (var range = worksheet.Cells[headerRow, 1, headerRow, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Group data by mark and diameter
                var groupedData = data.GroupBy(d => new { d.Mark, d.Diameter })
                                     .Select(g => new BBSEntry
                                     {
                                         Mark = g.Key.Mark,
                                         Diameter = g.Key.Diameter,
                                         Shape = g.First().Shape,
                                         Length = g.First().Length,
                                         Number = g.Sum(x => x.Number),
                                         TotalLength = g.Sum(x => x.TotalLength),
                                         Weight = g.Sum(x => x.Weight),
                                         Spacing = g.First().Spacing
                                     })
                                     .OrderBy(d => d.Mark)
                                     .ThenBy(d => d.Diameter)
                                     .ToList();

                // Add data rows
                int currentRow = headerRow + 1;
                foreach (var entry in groupedData)
                {
                    worksheet.Cells[currentRow, 1].Value = entry.Mark;
                    worksheet.Cells[currentRow, 2].Value = entry.Diameter;
                    worksheet.Cells[currentRow, 3].Value = entry.Shape;
                    worksheet.Cells[currentRow, 4].Value = entry.Length;
                    worksheet.Cells[currentRow, 5].Value = entry.Number;
                    worksheet.Cells[currentRow, 6].Value = entry.TotalLength;
                    worksheet.Cells[currentRow, 7].Value = Math.Round(entry.Weight, 2);
                    worksheet.Cells[currentRow, 8].Value = entry.Spacing;

                    currentRow++;
                }

                // Add summary
                int summaryRow = currentRow + 1;
                worksheet.Cells[summaryRow, 1].Value = "SUMMARY";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;

                // Group by diameter for summary
                var diameterSummary = groupedData.GroupBy(d => d.Diameter)
                                                 .Select(g => new
                                                 {
                                                     Diameter = g.Key,
                                                     TotalWeight = g.Sum(x => x.Weight)
                                                 })
                                                 .OrderBy(s => s.Diameter);

                summaryRow++;
                foreach (var summary in diameterSummary)
                {
                    worksheet.Cells[summaryRow, 1].Value = $"φ{summary.Diameter}mm";
                    worksheet.Cells[summaryRow, 2].Value = $"{summary.TotalWeight:F2} kg";
                    summaryRow++;
                }

                // Total weight
                double totalWeight = groupedData.Sum(d => d.Weight);
                summaryRow++;
                worksheet.Cells[summaryRow, 1].Value = "TOTAL WEIGHT";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 2].Value = $"{totalWeight:F2} kg";
                worksheet.Cells[summaryRow, 2].Style.Font.Bold = true;

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Save file
                FileInfo fileInfo = new FileInfo(filePath);
                package.SaveAs(fileInfo);
            }

            return filePath;
        }
    }

    /// <summary>
    /// Represents a single entry in the Bar Bending Schedule
    /// </summary>
    public class BBSEntry
    {
        public string Mark { get; set; }
        public int Diameter { get; set; }
        public string Shape { get; set; }
        public double Length { get; set; } // mm
        public int Number { get; set; }
        public double TotalLength { get; set; } // mm
        public double Weight { get; set; } // kg
        public string Spacing { get; set; }
    }
}
