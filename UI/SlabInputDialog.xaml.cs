using System;
using System.Windows;
using RebarAutomation.Models;
using RebarAutomation.Commands;

namespace RebarAutomation.UI
{
    public partial class SlabInputDialog : Window
    {
        public SlabDesignInput DesignInput { get; private set; }

        public SlabInputDialog(SlabDimensions dimensions)
        {
            InitializeComponent();
            
            // Pre-fill dimensions from selected slab
            txtLength.Text = dimensions.Length.ToString("F0");
            txtWidth.Text = dimensions.Width.ToString("F0");
            txtThickness.Text = dimensions.Thickness.ToString("F0");
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate and parse inputs
                DesignInput = new SlabDesignInput
                {
                    Length = double.Parse(txtLength.Text),
                    Width = double.Parse(txtWidth.Text),
                    Thickness = double.Parse(txtThickness.Text),
                    DeadLoad = double.Parse(txtDeadLoad.Text),
                    LiveLoad = double.Parse(txtLiveLoad.Text),
                    FloorFinishLoad = double.Parse(txtFloorFinish.Text),
                    TopCover = double.Parse(txtTopCover.Text),
                    BottomCover = double.Parse(txtBottomCover.Text),
                    SideCover = 25
                };

                // Parse concrete grade
                string concreteGrade = ((System.Windows.Controls.ComboBoxItem)cmbConcreteGrade.SelectedItem).Content.ToString();
                DesignInput.ConcreteGrade = (ConcreteGrade)Enum.Parse(typeof(ConcreteGrade), concreteGrade);

                // Parse steel grade
                string steelGrade = ((System.Windows.Controls.ComboBoxItem)cmbSteelGrade.SelectedItem).Content.ToString();
                DesignInput.SteelGrade = (SteelGrade)Enum.Parse(typeof(SteelGrade), steelGrade);

                // Parse support type
                if (rbSimple.IsChecked == true)
                    DesignInput.SupportType = SupportType.SimpleSupport;
                else if (rbFixed.IsChecked == true)
                    DesignInput.SupportType = SupportType.FixedSupport;
                else
                    DesignInput.SupportType = SupportType.ContinuousSupport;

                // Basic validation
                if (DesignInput.Length <= 0 || DesignInput.Width <= 0 || DesignInput.Thickness <= 0)
                {
                    MessageBox.Show("Dimensions must be positive values.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (DesignInput.Thickness < 100)
                {
                    MessageBox.Show("Slab thickness should be at least 100mm.", "Validation Warning", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                DialogResult = true;
                Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid numeric values for all fields.", "Input Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
