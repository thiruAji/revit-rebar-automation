using System;
using System.Collections.Generic;
using RebarAutomation.Models;

namespace RebarAutomation.Engine.IS456
{
    /// <summary>
    /// Validates design against IS 456 code requirements
    /// </summary>
    public class ValidationEngine
    {
        private readonly SlabDesignInput _input;
        private readonly SlabDesignOutput _output;
        private List<string> _errors;
        private List<string> _warnings;

        public ValidationEngine(SlabDesignInput input, SlabDesignOutput output)
        {
            _input = input;
            _output = output;
            _errors = new List<string>();
            _warnings = new List<string>();
        }

        /// <summary>
        /// Performs all validation checks
        /// </summary>
        public ValidationResult Validate()
        {
            ValidateReinforcementRatio();
            ValidateSpacing();
            ValidateCover();
            ValidateBarDiameter();
            ValidateMinimumThickness();

            var result = new ValidationResult
            {
                IsValid = _errors.Count == 0,
                Errors = _errors,
                Warnings = _warnings
            };

            return result;
        }

        /// <summary>
        /// Validates reinforcement ratio (IS 456 Cl. 26.5.1.1)
        /// Minimum: 0.12% for HYSD bars, 0.15% for mild steel
        /// Maximum: 4% (IS 456 Cl. 26.5.1.2)
        /// </summary>
        private void ValidateReinforcementRatio()
        {
            double grossArea = 1000 * _input.Thickness; // Per meter width
            
            // Main reinforcement ratio
            double mainBarArea = Math.PI * _output.MainBarDiameter * _output.MainBarDiameter / 4.0;
            double providedMainSteel = (mainBarArea / _output.MainBarSpacing) * 1000;
            double mainReinfRatio = (providedMainSteel / grossArea) * 100;

            // Minimum reinforcement
            double minRatio = 0.12; // For HYSD bars
            if (mainReinfRatio < minRatio)
            {
                _errors.Add($"Main reinforcement ratio {mainReinfRatio:F3}% is less than minimum {minRatio}%");
            }

            // Maximum reinforcement
            double maxRatio = 4.0;
            if (mainReinfRatio > maxRatio)
            {
                _errors.Add($"Main reinforcement ratio {mainReinfRatio:F3}% exceeds maximum {maxRatio}%");
            }

            // Distribution reinforcement
            double distBarArea = Math.PI * _output.DistBarDiameter * _output.DistBarDiameter / 4.0;
            double providedDistSteel = (distBarArea / _output.DistBarSpacing) * 1000;
            double distReinfRatio = (providedDistSteel / grossArea) * 100;

            if (distReinfRatio < minRatio)
            {
                _warnings.Add($"Distribution reinforcement ratio {distReinfRatio:F3}% is less than minimum {minRatio}%");
            }
        }

        /// <summary>
        /// Validates bar spacing (IS 456 Cl. 26.3.3)
        /// Maximum spacing: Lesser of 3d or 300mm for main bars
        /// Maximum spacing: Lesser of 5d or 450mm for distribution bars
        /// </summary>
        private void ValidateSpacing()
        {
            // Main bars
            double maxMainSpacing = Math.Min(3 * _input.Thickness, 300);
            if (_output.MainBarSpacing > maxMainSpacing)
            {
                _errors.Add($"Main bar spacing {_output.MainBarSpacing}mm exceeds maximum {maxMainSpacing}mm");
            }

            // Minimum spacing for concrete placement
            double minSpacing = Math.Max(_output.MainBarDiameter + 5, 25); // Bar dia + 5mm or 25mm
            if (_output.MainBarSpacing < minSpacing)
            {
                _warnings.Add($"Main bar spacing {_output.MainBarSpacing}mm is very tight. Consider using larger bars.");
            }

            // Distribution bars
            double maxDistSpacing = Math.Min(5 * _input.Thickness, 450);
            if (_output.DistBarSpacing > maxDistSpacing)
            {
                _errors.Add($"Distribution bar spacing {_output.DistBarSpacing}mm exceeds maximum {maxDistSpacing}mm");
            }
        }

        /// <summary>
        /// Validates concrete cover (IS 456 Cl. 26.4)
        /// </summary>
        private void ValidateCover()
        {
            // Minimum cover based on exposure condition
            // For moderate exposure (typical): 25mm for slabs
            double minCover = 25;
            
            if (_input.BottomCover < minCover)
            {
                _errors.Add($"Bottom cover {_input.BottomCover}mm is less than minimum {minCover}mm");
            }

            if (_input.TopCover < minCover)
            {
                _errors.Add($"Top cover {_input.TopCover}mm is less than minimum {minCover}mm");
            }

            // Maximum cover (not specified in IS 456, but practical limit)
            double maxCover = 75;
            if (_input.BottomCover > maxCover)
            {
                _warnings.Add($"Bottom cover {_input.BottomCover}mm is unusually large");
            }

            // Cover should not exceed bar diameter (for bond)
            if (_input.BottomCover > _output.MainBarDiameter * 2)
            {
                _warnings.Add("Cover is more than 2 times bar diameter. May affect bond.");
            }
        }

        /// <summary>
        /// Validates bar diameter restrictions
        /// </summary>
        private void ValidateBarDiameter()
        {
            // Bar diameter should not exceed 1/8 of slab thickness
            double maxDiameter = _input.Thickness / 8.0;
            
            if (_output.MainBarDiameter > maxDiameter)
            {
                _warnings.Add($"Main bar diameter {_output.MainBarDiameter}mm exceeds recommended limit of {maxDiameter:F1}mm (D/8)");
            }

            // Minimum bar diameter for slabs: 8mm
            if (_output.MainBarDiameter < 8)
            {
                _errors.Add($"Main bar diameter {_output.MainBarDiameter}mm is less than minimum 8mm");
            }

            if (_output.DistBarDiameter < 8)
            {
                _errors.Add($"Distribution bar diameter {_output.DistBarDiameter}mm is less than minimum 8mm");
            }
        }

        /// <summary>
        /// Validates minimum slab thickness
        /// </summary>
        private void ValidateMinimumThickness()
        {
            // Minimum thickness for slabs (practical): 100mm
            if (_input.Thickness < 100)
            {
                _errors.Add($"Slab thickness {_input.Thickness}mm is less than practical minimum 100mm");
            }

            // Check if thickness is adequate for cover + bars
            double requiredThickness = _input.BottomCover + _output.MainBarDiameter + 
                                       _output.DistBarDiameter + _input.TopCover + 10; // 10mm tolerance

            if (_input.Thickness < requiredThickness)
            {
                _errors.Add($"Slab thickness {_input.Thickness}mm is insufficient for cover and reinforcement. " +
                           $"Minimum required: {requiredThickness:F0}mm");
            }
        }
    }

    /// <summary>
    /// Result of validation checks
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public ValidationResult()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
        }
    }
}
