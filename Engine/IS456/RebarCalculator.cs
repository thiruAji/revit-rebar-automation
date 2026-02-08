using System;
using System.Linq;
using RebarAutomation.Models;

namespace RebarAutomation.Engine.IS456
{
    /// <summary>
    /// Calculates specific rebar requirements (bar sizes, spacing, counts)
    /// </summary>
    public class RebarCalculator
    {
        private readonly SlabDesignInput _input;
        private readonly SlabDesignOutput _designOutput;

        public RebarCalculator(SlabDesignInput input, SlabDesignOutput designOutput)
        {
            _input = input;
            _designOutput = designOutput;
        }

        /// <summary>
        /// Calculates main reinforcement bars (diameter, spacing, count)
        /// </summary>
        public void CalculateMainBars()
        {
            double requiredAst = Math.Max(_designOutput.AstMainBottom, _designOutput.AstMainTop);
            double lx = Math.Min(_input.Length, _input.Width);

            // Try each preferred bar diameter to find optimal spacing
            foreach (int diameter in _input.PreferredBarDiameters.OrderBy(d => d))
            {
                double barArea = Math.PI * diameter * diameter / 4.0;
                double spacing = (barArea * 1000) / requiredAst; // Spacing in mm for 1m width

                // Check if spacing is within limits
                if (spacing >= _input.MinBarSpacing && spacing <= _input.MaxBarSpacing)
                {
                    // Check maximum spacing as per IS 456 Cl. 26.3.3
                    double maxAllowableSpacing = Math.Min(3 * _input.Thickness, 300);
                    
                    if (spacing <= maxAllowableSpacing)
                    {
                        _designOutput.MainBarDiameter = diameter;
                        _designOutput.MainBarSpacing = Math.Floor(spacing / 10) * 10; // Round down to nearest 10mm
                        
                        // Calculate number of bars
                        _designOutput.MainBarCount = (int)Math.Ceiling(lx / _designOutput.MainBarSpacing) + 1;
                        
                        return;
                    }
                }
            }

            // If no suitable spacing found, use largest diameter with minimum spacing
            int largestDia = _input.PreferredBarDiameters.Max();
            _designOutput.MainBarDiameter = largestDia;
            _designOutput.MainBarSpacing = _input.MinBarSpacing;
            _designOutput.MainBarCount = (int)Math.Ceiling(lx / _designOutput.MainBarSpacing) + 1;
            
            _designOutput.Warnings.Add($"Could not find optimal spacing for main bars. Using {largestDia}mm @ {_input.MinBarSpacing}mm c/c");
        }

        /// <summary>
        /// Calculates distribution bars (perpendicular to main bars)
        /// </summary>
        public void CalculateDistributionBars()
        {
            double requiredAst = _designOutput.AstDistribution;
            double ly = Math.Max(_input.Length, _input.Width);

            // Distribution bars are typically smaller than main bars
            int[] distBarOptions = _input.PreferredBarDiameters.Where(d => d <= _designOutput.MainBarDiameter).ToArray();
            
            if (distBarOptions.Length == 0)
            {
                distBarOptions = new int[] { 8, 10 }; // Fallback to smaller bars
            }

            foreach (int diameter in distBarOptions.OrderBy(d => d))
            {
                double barArea = Math.PI * diameter * diameter / 4.0;
                double spacing = (barArea * 1000) / requiredAst;

                // Maximum spacing for distribution bars (IS 456 Cl. 26.5.2.1)
                double maxAllowableSpacing = Math.Min(5 * _input.Thickness, 450);

                if (spacing >= _input.MinBarSpacing && spacing <= Math.Min(_input.MaxBarSpacing, maxAllowableSpacing))
                {
                    _designOutput.DistBarDiameter = diameter;
                    _designOutput.DistBarSpacing = Math.Floor(spacing / 10) * 10;
                    _designOutput.DistBarCount = (int)Math.Ceiling(ly / _designOutput.DistBarSpacing) + 1;
                    
                    return;
                }
            }

            // Fallback
            _designOutput.DistBarDiameter = distBarOptions.Min();
            _designOutput.DistBarSpacing = 200; // Default 200mm spacing
            _designOutput.DistBarCount = (int)Math.Ceiling(ly / _designOutput.DistBarSpacing) + 1;
        }

        /// <summary>
        /// Calculates development length as per IS 456 Cl. 26.2.1
        /// Ld = (φ * σs) / (4 * τbd)
        /// </summary>
        public void CalculateDevelopmentLength()
        {
            double fy = GetSteelStrength(_input.SteelGrade);
            double fck = GetConcreteStrength(_input.ConcreteGrade);
            
            // Design bond stress (IS 456 Table 21)
            double taubd = GetBondStress(fck);
            
            // Stress in bar at design load
            double sigmaS = 0.87 * fy;
            
            // Development length for main bars
            int phi = _designOutput.MainBarDiameter;
            _designOutput.DevelopmentLength = (phi * sigmaS) / (4 * taubd);
            
            // Lap length = Ld (IS 456 Cl. 26.2.5.1)
            _designOutput.LapLength = _designOutput.DevelopmentLength;
            
            // For bars in tension, lap length should not be less than:
            // - 15 times bar diameter
            // - 200 mm
            double minLapLength = Math.Max(15 * phi, 200);
            _designOutput.LapLength = Math.Max(_designOutput.LapLength, minLapLength);
        }

        /// <summary>
        /// Applies crank/bend requirements for bars at supports
        /// </summary>
        public CrankDetails ApplyCranks()
        {
            // Cranks are typically provided at 45 degrees
            // Crank length = effective depth / 2
            double effectiveDepth = _input.Thickness - _input.BottomCover - _designOutput.MainBarDiameter / 2.0;
            
            double crankLength = effectiveDepth / 2.0;
            double crankAngle = 45; // degrees
            
            // Horizontal projection of crank
            double horizontalProjection = crankLength * Math.Cos(crankAngle * Math.PI / 180);
            
            return new CrankDetails
            {
                CrankLength = crankLength,
                CrankAngle = crankAngle,
                HorizontalProjection = horizontalProjection,
                IsRequired = _input.SupportType != SupportType.SimpleSupport
            };
        }

        #region Helper Methods

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

        private double GetConcreteStrength(ConcreteGrade grade)
        {
            switch (grade)
            {
                case ConcreteGrade.M20: return 20;
                case ConcreteGrade.M25: return 25;
                case ConcreteGrade.M30: return 30;
                case ConcreteGrade.M35: return 35;
                case ConcreteGrade.M40: return 40;
                case ConcreteGrade.M45: return 45;
                case ConcreteGrade.M50: return 50;
                default: return 25;
            }
        }

        private double GetBondStress(double fck)
        {
            // Design bond stress for HYSD bars (IS 456 Table 21)
            if (fck <= 20) return 1.2;
            if (fck <= 25) return 1.4;
            if (fck <= 30) return 1.5;
            if (fck <= 35) return 1.7;
            if (fck <= 40) return 1.9;
            return 2.0;
        }

        #endregion
    }

    /// <summary>
    /// Details for crank/bend in reinforcement bars
    /// </summary>
    public class CrankDetails
    {
        public double CrankLength { get; set; }
        public double CrankAngle { get; set; }
        public double HorizontalProjection { get; set; }
        public bool IsRequired { get; set; }
    }
}
