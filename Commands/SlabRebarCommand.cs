using System;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RebarAutomation.Models;
using RebarAutomation.Engine.IS456;
using RebarAutomation.RevitIntegration;
using RebarAutomation.UI;
using System.Linq;

namespace RebarAutomation.Commands
{
    /// <summary>
    /// Main command for generating slab rebar
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SlabRebarCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // Get selected floor element
                Floor selectedSlab = GetSelectedSlab(uiDoc);
                
                if (selectedSlab == null)
                {
                    TaskDialog.Show("Error", "Please select a floor element first.");
                    return Result.Cancelled;
                }

                // Get slab dimensions
                var slabDimensions = GetSlabDimensions(selectedSlab);

                // Show input dialog
                SlabInputDialog dialog = new SlabInputDialog(slabDimensions);
                bool? dialogResult = dialog.ShowDialog();

                if (dialogResult != true)
                {
                    return Result.Cancelled;
                }

                // Get input from dialog
                SlabDesignInput input = dialog.DesignInput;

                // Perform analysis
                SlabAnalyzer analyzer = new SlabAnalyzer(input);
                SlabDesignOutput output = analyzer.Analyze();

                // Calculate rebar details
                RebarCalculator rebarCalc = new RebarCalculator(input, output);
                rebarCalc.CalculateMainBars();
                rebarCalc.CalculateDistributionBars();
                rebarCalc.CalculateDevelopmentLength();

                // Validate design
                ValidationEngine validator = new ValidationEngine(input, output);
                var validationResult = validator.Validate();

                // Show validation results
                if (!validationResult.IsValid)
                {
                    string errorMsg = "Design validation failed:\n\n";
                    errorMsg += string.Join("\n", validationResult.Errors);
                    
                    if (validationResult.Warnings.Count > 0)
                    {
                        errorMsg += "\n\nWarnings:\n";
                        errorMsg += string.Join("\n", validationResult.Warnings);
                    }

                    TaskDialog.Show("Validation Failed", errorMsg);
                    return Result.Failed;
                }

                // Show warnings if any
                if (validationResult.Warnings.Count > 0)
                {
                    string warningMsg = "Design has warnings:\n\n";
                    warningMsg += string.Join("\n", validationResult.Warnings);
                    warningMsg += "\n\nDo you want to continue?";

                    TaskDialogResult warningResult = TaskDialog.Show("Warnings", warningMsg, 
                        TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

                    if (warningResult != TaskDialogResult.Yes)
                    {
                        return Result.Cancelled;
                    }
                }

                // Show design summary
                string summary = GetDesignSummary(input, output);
                TaskDialogResult summaryResult = TaskDialog.Show("Design Summary", summary,
                    TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel);

                if (summaryResult != TaskDialogResult.Ok)
                {
                    return Result.Cancelled;
                }

                // Create rebar in Revit
                RebarPlacer placer = new RebarPlacer(doc, input, output);
                var rebarList = placer.CreateRebarSet(selectedSlab);

                // Generate BBS
                BBSGenerator bbsGen = new BBSGenerator(doc);
                string bbsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string bbsFile = bbsGen.GenerateBBS(rebarList, bbsPath);

                // Success message
                TaskDialog.Show("Success", 
                    $"Rebar generated successfully!\n\n" +
                    $"Main bars: φ{output.MainBarDiameter}mm @ {output.MainBarSpacing}mm c/c ({output.MainBarCount} bars)\n" +
                    $"Distribution bars: φ{output.DistBarDiameter}mm @ {output.DistBarSpacing}mm c/c ({output.DistBarCount} bars)\n\n" +
                    $"Bar Bending Schedule saved to:\n{bbsFile}");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", $"An error occurred:\n{ex.Message}");
                return Result.Failed;
            }
        }

        private Floor GetSelectedSlab(UIDocument uiDoc)
        {
            var selection = uiDoc.Selection.GetElementIds();
            
            if (selection.Count == 0)
            {
                return null;
            }

            Element elem = uiDoc.Document.GetElement(selection.First());
            return elem as Floor;
        }

        private SlabDimensions GetSlabDimensions(Floor slab)
        {
            var dimensions = new SlabDimensions();

            // Get slab geometry
            Options opt = new Options();
            GeometryElement geomElem = slab.get_Geometry(opt);

            foreach (GeometryObject geomObj in geomElem)
            {
                Solid solid = geomObj as Solid;
                if (solid != null && solid.Faces.Size > 0)
                {
                    BoundingBoxXYZ bbox = solid.GetBoundingBox();
                    
                    double length = (bbox.Max.X - bbox.Min.X) * 304.8; // Convert to mm
                    double width = (bbox.Max.Y - bbox.Min.Y) * 304.8;
                    double thickness = (bbox.Max.Z - bbox.Min.Z) * 304.8;

                    dimensions.Length = Math.Round(length, 0);
                    dimensions.Width = Math.Round(width, 0);
                    dimensions.Thickness = Math.Round(thickness, 0);
                    
                    break;
                }
            }

            return dimensions;
        }

        private string GetDesignSummary(SlabDesignInput input, SlabDesignOutput output)
        {
            string summary = "DESIGN SUMMARY\n";
            summary += "=================\n\n";
            
            summary += "Slab Type: " + (output.IsOneWay ? "One-Way Slab" : "Two-Way Slab") + "\n";
            summary += $"Ly/Lx Ratio: {output.LyLxRatio:F2}\n\n";

            summary += "REINFORCEMENT:\n";
            summary += $"Main Bars: φ{output.MainBarDiameter}mm @ {output.MainBarSpacing}mm c/c\n";
            summary += $"Number of bars: {output.MainBarCount}\n";
            summary += $"Distribution Bars: φ{output.DistBarDiameter}mm @ {output.DistBarSpacing}mm c/c\n";
            summary += $"Number of bars: {output.DistBarCount}\n\n";

            summary += $"Development Length: {output.DevelopmentLength:F0}mm\n";
            summary += $"Lap Length: {output.LapLength:F0}mm\n\n";

            summary += "DEFLECTION CHECK:\n";
            summary += $"Actual L/d: {output.ActualSpanDepthRatio:F2}\n";
            summary += $"Allowable L/d: {output.AllowableSpanDepthRatio:F2}\n";
            summary += $"Status: {(output.DeflectionCheckPassed ? "PASS" : "FAIL")}\n\n";

            summary += "Proceed with rebar generation?";

            return summary;
        }
    }

    public class SlabDimensions
    {
        public double Length { get; set; }
        public double Width { get; set; }
        public double Thickness { get; set; }
    }
}
