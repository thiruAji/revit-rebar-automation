using System;
using RebarAutomation.Models;

namespace RebarAutomation.Engine.IS456
{
    /// <summary>
    /// Calculates column reinforcement according to IS 456
    /// </summary>
    public class ColumnCalculator
    {
        private readonly ColumnDesignInput _input;

        public ColumnCalculator(ColumnDesignInput input)
        {
            _input = input;
        }

        /// <summary>
        /// Performs complete column design
        /// </summary>
        public ColumnDesignOutput Design()
        {
            var output = new ColumnDesignOutput();

            // Calculate design loads
            CalculateDesignLoads(output);

            // Design longitudinal reinforcement
            DesignLongitudinalReinforcement(output);

            // Design lateral ties/stirrups
            DesignLateralReinforcement(output);

            // Check slenderness
            CheckSlenderness(output);

            return output;
        }

        private void CalculateDesignLoads(ColumnDesignOutput output)
        {
            output.AxialLoad = _input.AxialLoad * 1.5; // Factored load
            output.MomentX = _input.MomentX * 1.5;
            output.MomentY = _input.MomentY * 1.5;
        }

        private void DesignLongitudinalReinforcement(ColumnDesignOutput output)
        {
            double fck = GetConcreteStrength(_input.ConcreteGrade);
            double fy = GetSteelStrength(_input.SteelGrade);
            
            double Ag = _input.Width * _input.Depth; // Gross area

            // For axially loaded column (simplified)
            // Pu = 0.4 * fck * Ac + 0.67 * fy * Asc
            // Where Ac = Ag - Asc (concrete area)
            
            // Rearranging: Asc = (Pu - 0.4 * fck * Ag) / (0.67 * fy - 0.4 * fck)
            double Pu = output.AxialLoad * 1000; // Convert to kN
            double Asc = (Pu - 0.4 * fck * Ag) / (0.67 * fy - 0.4 * fck);

            // Minimum steel: 0.8% of gross area (IS 456 Cl. 26.5.3.1)
            double minSteel = 0.008 * Ag;
            
            // Maximum steel: 6% of gross area
            double maxSteel = 0.06 * Ag;

            output.LongitudinalSteelArea = Math.Max(Asc, minSteel);
            output.LongitudinalSteelArea = Math.Min(output.LongitudinalSteelArea, maxSteel);

            // Select bars
            SelectLongitudinalBars(output);
        }

        private void DesignLateralReinforcement(ColumnDesignOutput output)
        {
            // Lateral ties diameter (IS 456 Cl. 26.5.3.2c)
            // Should be at least 1/4 of main bar diameter or 6mm, whichever is greater
            output.TieDiameter = Math.Max(output.LongitudinalBarDiameter / 4, 6);
            
            // Round to standard sizes
            if (output.TieDiameter < 8) output.TieDiameter = 8;
            else if (output.TieDiameter < 10) output.TieDiameter = 10;
            else output.TieDiameter = 12;

            // Tie spacing (IS 456 Cl. 26.5.3.2d)
            // Least of:
            // - Least lateral dimension
            // - 16 times diameter of longitudinal bar
            // - 300mm
            
            double spacing1 = Math.Min(_input.Width, _input.Depth);
            double spacing2 = 16 * output.LongitudinalBarDiameter;
            double spacing3 = 300;

            output.TieSpacing = Math.Min(Math.Min(spacing1, spacing2), spacing3);
            output.TieSpacing = Math.Floor(output.TieSpacing / 25) * 25; // Round to 25mm
        }

        private void CheckSlenderness(ColumnDesignOutput output)
        {
            double effectiveLength = _input.UnbracedLength * GetEffectiveLengthFactor();
            double leastDimension = Math.Min(_input.Width, _input.Depth);
            
            output.SlendernessRatio = effectiveLength / leastDimension;

            // Short column if slenderness < 12 (IS 456 Cl. 25.1.2)
            output.IsShortColumn = output.SlendernessRatio < 12;
            
            if (!output.IsShortColumn)
            {
                output.Notes = "Slender column - additional moment due to slenderness must be considered";
            }
        }

        private void SelectLongitudinalBars(ColumnDesignOutput output)
        {
            int[] diameters = new int[] { 12, 16, 20, 25, 32 };
            
            // Minimum 4 bars for rectangular column
            int minBars = 4;

            foreach (int dia in diameters)
            {
                double barArea = Math.PI * dia * dia / 4.0;
                int numBars = (int)Math.Ceiling(output.LongitudinalSteelArea / barArea);
                
                if (numBars >= minBars && numBars <= 12) // Practical limits
                {
                    output.LongitudinalBarDiameter = dia;
                    output.LongitudinalBarCount = numBars;
                    
                    // Ensure even number for rectangular columns
                    if (output.LongitudinalBarCount % 2 != 0)
                    {
                        output.LongitudinalBarCount++;
                    }
                    
                    return;
                }
            }

            // Fallback
            output.LongitudinalBarDiameter = 20;
            output.LongitudinalBarCount = Math.Max(minBars, 
                (int)Math.Ceiling(output.LongitudinalSteelArea / (Math.PI * 20 * 20 / 4.0)));
            
            if (output.LongitudinalBarCount % 2 != 0)
            {
                output.LongitudinalBarCount++;
            }
        }

        private double GetConcreteStrength(ConcreteGrade grade)
        {
            return (int)grade;
        }

        private double GetSteelStrength(SteelGrade grade)
        {
            switch (grade)
            {
                case SteelGrade.Fe415: return 415;
                case SteelGrade.Fe500: return 500;
                case SteelGrade.Fe550: return 550;
                default: return 415;
            }
        }

        private double GetEffectiveLengthFactor()
        {
            // Simplified - assuming both ends hinged
            return 1.0;
        }
    }

    public class ColumnDesignInput
    {
        public double Width { get; set; } // mm
        public double Depth { get; set; } // mm
        public double UnbracedLength { get; set; } // mm
        public double AxialLoad { get; set; } // kN
        public double MomentX { get; set; } // kNm
        public double MomentY { get; set; } // kNm
        public double Cover { get; set; } // mm
        public ConcreteGrade ConcreteGrade { get; set; }
        public SteelGrade SteelGrade { get; set; }
    }

    public class ColumnDesignOutput
    {
        public double AxialLoad { get; set; } // Factored, kN
        public double MomentX { get; set; } // Factored, kNm
        public double MomentY { get; set; } // Factored, kNm
        
        public double LongitudinalSteelArea { get; set; } // mm²
        public int LongitudinalBarDiameter { get; set; }
        public int LongitudinalBarCount { get; set; }
        
        public int TieDiameter { get; set; }
        public double TieSpacing { get; set; }
        
        public double SlendernessRatio { get; set; }
        public bool IsShortColumn { get; set; }
        public string Notes { get; set; }
    }
}
