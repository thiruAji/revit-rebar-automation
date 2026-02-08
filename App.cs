using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RebarAutomation
{
    /// <summary>
    /// Main application class that implements IExternalApplication
    /// Registers the plugin with Revit and creates the ribbon interface
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Create ribbon tab
                string tabName = "Rebar Automation";
                application.CreateRibbonTab(tabName);

                // Create ribbon panel
                RibbonPanel panel = application.CreateRibbonPanel(tabName, "Tools");

                // Add Slab Rebar button
                AddSlabRebarButton(panel);

                // Add Beam Rebar button (Phase 2)
                AddBeamRebarButton(panel);

                // Add Column Rebar button (Phase 2)
                AddColumnRebarButton(panel);

                // Add separator
                panel.AddSeparator();

                // Add Generate BBS button
                AddGenerateBBSButton(panel);

                // Add Settings button
                AddSettingsButton(panel);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to initialize Rebar Automation:\n{ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Cleanup if needed
            return Result.Succeeded;
        }

        private void AddSlabRebarButton(RibbonPanel panel)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            
            PushButtonData buttonData = new PushButtonData(
                "SlabRebarButton",
                "Slab\nRebar",
                assemblyPath,
                "RebarAutomation.Commands.SlabRebarCommand"
            );

            buttonData.ToolTip = "Generate rebar for slabs automatically";
            buttonData.LongDescription = "Analyzes selected slab and generates reinforcement according to IS 456 standards. " +
                                         "Calculates main bars, distribution bars, and creates Bar Bending Schedule.";

            PushButton button = panel.AddItem(buttonData) as PushButton;
            // Note: Add icon image if available
            // button.LargeImage = new BitmapImage(new Uri("path/to/icon.png"));
        }

        private void AddBeamRebarButton(RibbonPanel panel)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            
            PushButtonData buttonData = new PushButtonData(
                "BeamRebarButton",
                "Beam\nRebar",
                assemblyPath,
                "RebarAutomation.Commands.BeamRebarCommand"
            );

            buttonData.ToolTip = "Generate rebar for beams (Phase 2)";
            buttonData.LongDescription = "Generates longitudinal and shear reinforcement for beams.";

            PushButton button = panel.AddItem(buttonData) as PushButton;
        }

        private void AddColumnRebarButton(RibbonPanel panel)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            
            PushButtonData buttonData = new PushButtonData(
                "ColumnRebarButton",
                "Column\nRebar",
                assemblyPath,
                "RebarAutomation.Commands.ColumnRebarCommand"
            );

            buttonData.ToolTip = "Generate rebar for columns (Phase 2)";
            buttonData.LongDescription = "Generates longitudinal bars and ties for columns.";

            PushButton button = panel.AddItem(buttonData) as PushButton;
        }

        private void AddGenerateBBSButton(RibbonPanel panel)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            
            PushButtonData buttonData = new PushButtonData(
                "GenerateBBSButton",
                "Generate\nBBS",
                assemblyPath,
                "RebarAutomation.Commands.GenerateBBSCommand"
            );

            buttonData.ToolTip = "Generate Bar Bending Schedule";
            buttonData.LongDescription = "Extracts all rebar from the model and generates a detailed Bar Bending Schedule in Excel format.";

            PushButton button = panel.AddItem(buttonData) as PushButton;
        }

        private void AddSettingsButton(RibbonPanel panel)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            
            PushButtonData buttonData = new PushButtonData(
                "SettingsButton",
                "Settings",
                assemblyPath,
                "RebarAutomation.Commands.SettingsCommand"
            );

            buttonData.ToolTip = "Configure plugin settings";
            buttonData.LongDescription = "Configure design code (IS 456, ACI 318, Eurocode 2), default materials, and preferences.";

            PushButton button = panel.AddItem(buttonData) as PushButton;
        }
    }
}
