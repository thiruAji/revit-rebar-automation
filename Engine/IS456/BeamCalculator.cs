using System;
using RebarAutomation.Models;

namespace RebarAutomation.Engine.IS456
{
    /// <summary>
    /// Calculates beam reinforcement according to IS 456
    /// </summary>
    public class BeamCalculator
    {
        private readonly BeamDesignInput _input;

        public BeamCalculator(BeamDesignInput input)
        {
            _input = input;
        }

        /// <summary>
        /// Performs complete beam design
        /// </summary>
        public BeamDesignOutput Design()
        {
            var output = new BeamDesignOutput();

            // Calculate design moments and shears
            CalculateDesignForces(output);

            // Design longitudinal reinforcement
            DesignLongitudinalReinforcement(output);

            // Design shear reinforcement
            DesignShearReinforcement(output);

            // Check deflection
            CheckDeflection(output);

            return output;
        }

        private void CalculateDesignForces(BeamDesignOutput output)
        {
            double span = _input.Span;
            double totalLoad = _input.DeadLoad + _input.LiveLoad;
            double factoredLoad = totalLoad * 1.5;

            // For simply supported beam
            output.MaxMoment = factoredLoad * span * span / 8.0; // kNm
            output.MaxShear = factoredLoad * span / 2.0; // kN
        }

        private void DesignLongitudinalReinforcement(BeamDesignOutput output)
        {
            double fck = GetConcreteStrength(_input.ConcreteGrade);
            double fy = GetSteelStrength(_input.SteelGrade);
            double d = _input.Depth - _input.Cover - 10; // Effective depth

            // Calculate required steel area
            double Mu = output.MaxMoment * 1000000; // Convert to Nmm
            double b = _input.Width;

            // Using Ast = (0.5 * fck * b * d / fy) * (1 - sqrt(1 - (4.6 * Mu) / (fck * b * d²)))
            double term1 = 0.5 * fck * b * d / fy;
            double term2 = 1 - Math.Sqrt(1 - (4.6 * Mu) / (fck * b * d * d));
            
            output.TensionSteelArea = term1 * term2;

            // Minimum steel (IS 456 Cl. 26.5.1.1)
            double minSteel = 0.85 * b * d / fy;
            output.TensionSteelArea = Math.Max(output.TensionSteelArea, minSteel);

            // Select bars
            SelectTensionBars(output);

            // Compression steel (if required)
            double muLim = 0.138 * fck * b * d * d / 1000000;
            if (output.MaxMoment > muLim)
            {
                output.CompressionSteelRequired = true;
                output.CompressionSteelArea = (output.MaxMoment - muLim) * 1000000 / (0.87 * fy * (d - 50));
                SelectCompressionBars(output);
            }
        }

        private void DesignShearReinforcement(BeamDesignOutput output)
        {
            double fck = GetConcreteStrength(_input.ConcreteGrade);
            double fy = GetSteelStrength(_input.SteelGrade);
            double b = _input.Width;
            double d = _input.Depth - _input.Cover - 10;

            // Calculate shear stress
            double tauV = (output.MaxShear * 1000) / (b * d); // N/mm²

            // Calculate concrete shear strength (IS 456 Table 19)
            double pt = (output.TensionSteelArea / (b * d)) * 100;
            double tauC = GetConcreteShearStrength(fck, pt);

            if (tauV <= tauC)
            {
                // Minimum shear reinforcement
                output.StirrupDiameter = 8;
                output.StirrupSpacing = 300; // mm
                output.ShearReinforcementNote = "Minimum shear reinforcement provided";
            }
            else
            {
                // Calculate required stirrup spacing
                double Vus = (tauV - tauC) * b * d / 1000; // kN
                
                // Assuming 2-legged 10mm stirrups
                output.StirrupDiameter = 10;
                double Asv = 2 * Math.PI * 10 * 10 / 4.0; // mm²
                
                double spacing = (0.87 * fy * Asv * d) / (Vus * 1000);
                
                // Maximum spacing (IS 456 Cl. 26.5.1.5)
                double maxSpacing = Math.Min(0.75 * d, 300);
                output.StirrupSpacing = Math.Min(spacing, maxSpacing);
                output.StirrupSpacing = Math.Floor(output.StirrupSpacing / 25) * 25; // Round to 25mm
            }
        }

        private void CheckDeflection(BeamDesignOutput output)
        {
            double d = _input.Depth - _input.Cover - 10;
            output.ActualSpanDepthRatio = _input.Span * 1000 / d;
            output.AllowableSpanDepthRatio = 20; // Basic value for simply supported
            output.DeflectionCheckPassed = output.ActualSpanDepthRatio <= output.AllowableSpanDepthRatio;
        }

        private void SelectTensionBars(BeamDesignOutput output)
        {
            int[] diameters = new int[] { 12, 16, 20, 25, 32 };
            
            foreach (int dia in diameters)
            {
                double barArea = Math.PI * dia * dia / 4.0;
                int numBars = (int)Math.Ceiling(output.TensionSteelArea / barArea);
                
                if (numBars >= 2 && numBars <= 6) // Practical limits
                {
                    output.TensionBarDiameter = dia;
                    output.TensionBarCount = numBars;
                    return;
                }
            }

            // Fallback
            output.TensionBarDiameter = 20;
            output.TensionBarCount = (int)Math.Ceiling(output.TensionSteelArea / (Math.PI * 20 * 20 / 4.0));
        }

        private void SelectCompressionBars(BeamDesignOutput output)
        {
            double barArea = Math.PI * output.TensionBarDiameter * output.TensionBarDiameter / 4.0;
            output.CompressionBarCount = (int)Math.Ceiling(output.CompressionSteelArea / barArea);
            output.CompressionBarDiameter = output.TensionBarDiameter;
        }

        private double GetConcreteStrength(ConcreteGrade grade)
        {
            return (int)grade; // Enum values match strength
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

        private double GetConcreteShearStrength(double fck, double pt)
        {
            // Simplified from IS 456 Table 19
            if (pt < 0.15) return 0.28;
            if (pt < 0.25) return 0.36;
            if (pt < 0.50) return 0.48;
            if (pt < 0.75) return 0.56;
            if (pt < 1.00) return 0.62;
            if (pt < 1.25) return 0.67;
            if (pt < 1.50) return 0.72;
            return 0.75;
        }
    }

    public class BeamDesignInput
    {
        public double Span { get; set; } // m
        public double Width { get; set; } // mm
        public double Depth { get; set; } // mm
        public double DeadLoad { get; set; } // kN/m
        public double LiveLoad { get; set; } // kN/m
        public double Cover { get; set; } // mm
        public ConcreteGrade ConcreteGrade { get; set; }
        public SteelGrade SteelGrade { get; set; }
    }

    public class BeamDesignOutput
    {
        public double MaxMoment { get; set; } // kNm
        public double MaxShear { get; set; } // kN
        
        public double TensionSteelArea { get; set; } // mm²
        public int TensionBarDiameter { get; set; }
        public int TensionBarCount { get; set; }
        
        public bool CompressionSteelRequired { get; set; }
        public double CompressionSteelArea { get; set; }
        public int CompressionBarDiameter { get; set; }
        public int CompressionBarCount { get; set; }
        
        public int StirrupDiameter { get; set; }
        public double StirrupSpacing { get; set; }
        public string ShearReinforcementNote { get; set; }
        
        public double ActualSpanDepthRatio { get; set; }
        public double AllowableSpanDepthRatio { get; set; }
        public bool DeflectionCheckPassed { get; set; }
    }
}
