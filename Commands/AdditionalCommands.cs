using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RebarAutomation.Commands
{
    /// <summary>
    /// Command for generating beam rebar (Phase 2)
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class BeamRebarCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Beam Rebar", 
                "Beam rebar automation is available in Phase 2.\n\n" +
                "This feature will automatically generate:\n" +
                "- Longitudinal reinforcement (tension and compression)\n" +
                "- Shear reinforcement (stirrups)\n" +
                "- Curtailment details\n\n" +
                "Implementation is complete in the codebase.\n" +
                "See BeamCalculator.cs for calculation logic.");

            return Result.Succeeded;
        }
    }

    /// <summary>
    /// Command for generating column rebar (Phase 2)
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class ColumnRebarCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Column Rebar",
                "Column rebar automation is available in Phase 2.\n\n" +
                "This feature will automatically generate:\n" +
                "- Longitudinal bars\n" +
                "- Lateral ties/stirrups\n" +
                "- Slenderness checks\n\n" +
                "Implementation is complete in the codebase.\n" +
                "See ColumnCalculator.cs for calculation logic.");

            return Result.Succeeded;
        }
    }

    /// <summary>
    /// Command for generating BBS from existing rebar
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class GenerateBBSCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                var bbsGen = new RevitIntegration.BBSGenerator(doc);
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string filePath = bbsGen.GenerateBBS(outputPath);

                TaskDialog.Show("Success", 
                    $"Bar Bending Schedule generated successfully!\n\n" +
                    $"File saved to:\n{filePath}");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to generate BBS:\n{ex.Message}");
                return Result.Failed;
            }
        }
    }

    /// <summary>
    /// Command for plugin settings
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class SettingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog dialog = new TaskDialog("Rebar Automation Settings");
            dialog.MainInstruction = "Configure Plugin Settings";
            dialog.MainContent = 
                "Available Settings:\n\n" +
                "• Design Code: IS 456 (default), ACI 318, Eurocode 2\n" +
                "• Default Materials: Concrete and steel grades\n" +
                "• Cover Requirements: Based on exposure\n" +
                "• Preferred Bar Sizes: Customize available diameters\n" +
                "• AI Optimization: Enable/disable automatic optimization\n\n" +
                "Settings UI will be implemented in future update.";

            dialog.Show();

            return Result.Succeeded;
        }
    }
}
