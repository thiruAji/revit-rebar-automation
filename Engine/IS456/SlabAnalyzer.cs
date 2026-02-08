using System;
using RebarAutomation.Models;

namespace RebarAutomation.Engine.IS456
{
    /// <summary>
    /// Analyzes slab and calculates design requirements according to IS 456
    /// </summary>
    public class SlabAnalyzer
    {
        private readonly SlabDesignInput _input;
        private SlabDesignOutput _output;

        public SlabAnalyzer(SlabDesignInput input)
        {
            _input = input;
            _output = new SlabDesignOutput();
        }

        /// <summary>
        /// Performs complete slab analysis
        /// </summary>
        public SlabDesignOutput Analyze()
        {
            DetermineSlabType();
            CalculateDesignMoments();
            CalculateRequiredSteel();
            ValidateDeflection();
            
            return _output;
        }

        /// <summary>
        /// Determines if slab is one-way or two-way based on ly/lx ratio
        /// IS 456: If ly/lx > 2, treat as one-way slab
        /// </summary>
        public void DetermineSlabType()
        {
            double lx = Math.Min(_input.Length, _input.Width);
            double ly = Math.Max(_input.Length, _input.Width);
            
            _output.LyLxRatio = ly / lx;

            if (_output.LyLxRatio > 2.0)
            {
                _output.IsOneWay = true;
                _output.IsTwoWay = false;
            }
            else
            {
                _output.IsOneWay = false;
                _output.IsTwoWay = true;
            }
        }

        /// <summary>
        /// Calculates design moments using IS 456 coefficients
        /// </summary>
        public void CalculateDesignMoments()
        {
            double lx = Math.Min(_input.Length, _input.Width);
            double ly = Math.Max(_input.Length, _input.Width);

            // Calculate total load (factored)
            double deadLoad = _input.DeadLoad + (_input.Thickness / 1000.0) * 25; // 25 kN/m³ for concrete
            double totalLoad = deadLoad + _input.LiveLoad + _input.FloorFinishLoad;
            double factoredLoad = totalLoad * 1.5; // Load factor as per IS 456

            if (_output.IsOneWay)
            {
                // One-way slab: Moments in short span only
                _output.PositiveMomentX = GetMomentCoefficient(true) * factoredLoad * lx * lx;
                _output.NegativeMomentX = GetMomentCoefficient(false) * factoredLoad * lx * lx;
                _output.PositiveMomentY = 0;
                _output.NegativeMomentY = 0;
            }
            else
            {
                // Two-way slab: Use moment coefficients from IS 456 Table 26
                double alphaX = GetTwoWayMomentCoefficient(_output.LyLxRatio, true);
                double alphaY = GetTwoWayMomentCoefficient(_output.LyLxRatio, false);

                _output.PositiveMomentX = alphaX * factoredLoad * lx * lx;
                _output.PositiveMomentY = alphaY * factoredLoad * lx * lx;
                
                // Negative moments (at supports) - typically 1.33 times positive moment
                _output.NegativeMomentX = _output.PositiveMomentX * 1.33;
                _output.NegativeMomentY = _output.PositiveMomentY * 1.33;
            }
        }

        /// <summary>
        /// Calculates required steel area from moments
        /// Using: Mu = 0.87 * fy * Ast * d * (1 - (Ast * fy) / (fck * b * d))
        /// </summary>
        public void CalculateRequiredSteel()
        {
            double fck = GetConcreteStrength(_input.ConcreteGrade);
            double fy = GetSteelStrength(_input.SteelGrade);
            
            double effectiveDepth = _input.Thickness - _input.BottomCover - 10; // Assuming 10mm bar for initial calc
            double b = 1000; // Per meter width

            // Calculate steel for main direction (bottom - positive moment)
            _output.AstMainBottom = CalculateSteelArea(_output.PositiveMomentX, fck, fy, b, effectiveDepth);

            // Calculate steel for main direction (top - negative moment)
            _output.AstMainTop = CalculateSteelArea(_output.NegativeMomentX, fck, fy, b, effectiveDepth);

            // Distribution steel (minimum 0.12% for HYSD bars as per IS 456 Cl. 26.5.2.1)
            double minDistSteel = 0.0012 * b * _input.Thickness;
            
            if (_output.IsTwoWay)
            {
                double astY = CalculateSteelArea(_output.PositiveMomentY, fck, fy, b, effectiveDepth);
                _output.AstDistribution = Math.Max(astY, minDistSteel);
            }
            else
            {
                _output.AstDistribution = minDistSteel;
            }

            // Check minimum steel (0.12% for HYSD)
            double minSteel = 0.0012 * b * _input.Thickness;
            _output.AstMainBottom = Math.Max(_output.AstMainBottom, minSteel);
            _output.AstMainTop = Math.Max(_output.AstMainTop, minSteel);
        }

        /// <summary>
        /// Validates deflection using span/depth ratio (IS 456 Cl. 23.2.1)
        /// </summary>
        public void ValidateDeflection()
        {
            double lx = Math.Min(_input.Length, _input.Width);
            double effectiveDepth = _input.Thickness - _input.BottomCover - 10;

            _output.ActualSpanDepthRatio = lx / effectiveDepth;

            // Basic span/depth ratio for simply supported slab = 20 (IS 456 Table 19)
            double basicRatio = 20;
            
            // Modification factor based on support conditions
            double modificationFactor = GetDeflectionModificationFactor();
            
            _output.AllowableSpanDepthRatio = basicRatio * modificationFactor;
            _output.DeflectionCheckPassed = _output.ActualSpanDepthRatio <= _output.AllowableSpanDepthRatio;

            if (!_output.DeflectionCheckPassed)
            {
                _output.Warnings.Add($"Deflection check failed. Actual L/d = {_output.ActualSpanDepthRatio:F2}, " +
                                    $"Allowable = {_output.AllowableSpanDepthRatio:F2}. Consider increasing slab thickness.");
            }
        }

        #region Helper Methods

        private double GetMomentCoefficient(bool isPositive)
        {
            // Simplified coefficients for one-way slab
            switch (_input.SupportType)
            {
                case SupportType.SimpleSupport:
                    return isPositive ? 0.125 : 0;
                case SupportType.FixedSupport:
                    return isPositive ? 0.0833 : 0.0833;
                case SupportType.ContinuousSupport:
                    return isPositive ? 0.0625 : 0.0833;
                default:
                    return 0.125;
            }
        }

        private double GetTwoWayMomentCoefficient(double lyLxRatio, bool isShortSpan)
        {
            // Simplified IS 456 Table 26 coefficients
            // These should be interpolated from the actual table
            if (isShortSpan)
            {
                if (lyLxRatio <= 1.0) return 0.032;
                if (lyLxRatio <= 1.1) return 0.037;
                if (lyLxRatio <= 1.2) return 0.043;
                if (lyLxRatio <= 1.3) return 0.047;
                if (lyLxRatio <= 1.4) return 0.051;
                if (lyLxRatio <= 1.5) return 0.055;
                if (lyLxRatio <= 1.75) return 0.061;
                return 0.065;
            }
            else
            {
                if (lyLxRatio <= 1.0) return 0.032;
                if (lyLxRatio <= 1.1) return 0.028;
                if (lyLxRatio <= 1.2) return 0.024;
                if (lyLxRatio <= 1.3) return 0.021;
                if (lyLxRatio <= 1.4) return 0.018;
                if (lyLxRatio <= 1.5) return 0.016;
                if (lyLxRatio <= 1.75) return 0.013;
                return 0.011;
            }
        }

        private double CalculateSteelArea(double moment, double fck, double fy, double b, double d)
        {
            // Mu = 0.87 * fy * Ast * d * (1 - (Ast * fy) / (fck * b * d))
            // Solving quadratic equation for Ast
            
            double muLim = 0.138 * fck * b * d * d / 1000000; // Limiting moment in kNm

            if (moment > muLim)
            {
                _output.Warnings.Add($"Moment {moment:F2} kNm exceeds limiting moment {muLim:F2} kNm. " +
                                    "Consider increasing depth or using compression reinforcement.");
            }

            // Simplified formula: Ast = (0.5 * fck * b * d / fy) * (1 - sqrt(1 - (4.6 * Mu) / (fck * b * d²)))
            double term1 = 0.5 * fck * b * d / fy;
            double term2 = 1 - Math.Sqrt(1 - (4.6 * moment * 1000000) / (fck * b * d * d));
            
            return term1 * term2;
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

        private double GetDeflectionModificationFactor()
        {
            switch (_input.SupportType)
            {
                case SupportType.SimpleSupport:
                    return 1.0;
                case SupportType.FixedSupport:
                    return 1.5;
                case SupportType.ContinuousSupport:
                    return 1.3;
                default:
                    return 1.0;
            }
        }

        #endregion
    }
}
