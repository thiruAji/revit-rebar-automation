using System;
using System.Collections.Generic;

namespace RebarAutomation.AI
{
    /// <summary>
    /// Pattern recognition for complex geometries using rule-based AI
    /// </summary>
    public class PatternRecognition
    {
        /// <summary>
        /// Detects irregular slab shapes and suggests appropriate rebar layout
        /// </summary>
        public IrregularShapeAnalysis DetectIrregularShapes(List<Point2D> slabBoundary)
        {
            var analysis = new IrregularShapeAnalysis();

            // Check if shape is rectangular
            if (IsRectangular(slabBoundary))
            {
                analysis.ShapeType = ShapeType.Rectangular;
                analysis.Complexity = ComplexityLevel.Simple;
                return analysis;
            }

            // Check for L-shape
            if (IsLShape(slabBoundary))
            {
                analysis.ShapeType = ShapeType.LShape;
                analysis.Complexity = ComplexityLevel.Moderate;
                analysis.SuggestedApproach = "Divide into two rectangular panels and design separately";
                return analysis;
            }

            // Check for T-shape
            if (IsTShape(slabBoundary))
            {
                analysis.ShapeType = ShapeType.TShape;
                analysis.Complexity = ComplexityLevel.Moderate;
                analysis.SuggestedApproach = "Divide into rectangular panels at the junction";
                return analysis;
            }

            // Complex irregular shape
            analysis.ShapeType = ShapeType.Irregular;
            analysis.Complexity = ComplexityLevel.Complex;
            analysis.SuggestedApproach = "Use finite element analysis or divide into simpler shapes";

            return analysis;
        }

        /// <summary>
        /// Suggests rebar layout for complex patterns
        /// </summary>
        public RebarLayoutSuggestion SuggestRebarLayout(IrregularShapeAnalysis shapeAnalysis)
        {
            var suggestion = new RebarLayoutSuggestion();

            switch (shapeAnalysis.ShapeType)
            {
                case ShapeType.Rectangular:
                    suggestion.MainDirection = "Along shorter span";
                    suggestion.DistributionDirection = "Along longer span";
                    suggestion.SpecialRequirements = new List<string>();
                    break;

                case ShapeType.LShape:
                    suggestion.MainDirection = "Divide into two panels";
                    suggestion.DistributionDirection = "Perpendicular in each panel";
                    suggestion.SpecialRequirements = new List<string>
                    {
                        "Add extra bars at re-entrant corner",
                        "Provide diagonal bars at corner (45°)",
                        "Ensure proper anchorage at panel junction"
                    };
                    break;

                case ShapeType.TShape:
                    suggestion.MainDirection = "Along each wing";
                    suggestion.DistributionDirection = "Perpendicular to main bars";
                    suggestion.SpecialRequirements = new List<string>
                    {
                        "Add extra reinforcement at T-junction",
                        "Ensure continuity of bars across junction",
                        "Consider torsion at junction"
                    };
                    break;

                case ShapeType.Irregular:
                    suggestion.MainDirection = "Radial from center or along principal axes";
                    suggestion.DistributionDirection = "Circumferential or perpendicular";
                    suggestion.SpecialRequirements = new List<string>
                    {
                        "Use mesh reinforcement for very irregular shapes",
                        "Provide additional bars at acute corners",
                        "Consider using FEA for accurate design"
                    };
                    break;
            }

            return suggestion;
        }

        /// <summary>
        /// Handles openings and penetrations in slabs
        /// </summary>
        public OpeningReinforcementPlan HandleOpenings(List<Opening> openings, double slabThickness)
        {
            var plan = new OpeningReinforcementPlan();

            foreach (var opening in openings)
            {
                var requirement = new OpeningReinforcement
                {
                    OpeningId = opening.Id,
                    Size = opening.Size
                };

                // Small openings (< 500mm)
                if (opening.Size < 500)
                {
                    requirement.TrimmerBarsRequired = true;
                    requirement.TrimmerBarDiameter = 12;
                    requirement.TrimmerBarCount = 2; // One on each side
                    requirement.AdditionalLength = opening.Size + 600; // Extend 300mm beyond opening
                    requirement.Notes = "Provide trimmer bars on both sides of opening";
                }
                // Medium openings (500-1500mm)
                else if (opening.Size < 1500)
                {
                    requirement.TrimmerBarsRequired = true;
                    requirement.TrimmerBarDiameter = 16;
                    requirement.TrimmerBarCount = 4; // Two on each side
                    requirement.AdditionalLength = opening.Size + 1000;
                    requirement.Notes = "Provide double trimmer bars. Check if additional analysis needed.";
                }
                // Large openings (> 1500mm)
                else
                {
                    requirement.TrimmerBarsRequired = true;
                    requirement.TrimmerBarDiameter = 20;
                    requirement.TrimmerBarCount = 6;
                    requirement.AdditionalLength = opening.Size + 1500;
                    requirement.Notes = "Large opening - requires detailed analysis. Consider edge beams.";
                    requirement.RequiresDetailedAnalysis = true;
                }

                plan.Reinforcements.Add(requirement);
            }

            return plan;
        }

        #region Helper Methods

        private bool IsRectangular(List<Point2D> boundary)
        {
            if (boundary.Count != 4) return false;

            // Check if all angles are 90 degrees
            for (int i = 0; i < 4; i++)
            {
                var p1 = boundary[i];
                var p2 = boundary[(i + 1) % 4];
                var p3 = boundary[(i + 2) % 4];

                double angle = CalculateAngle(p1, p2, p3);
                if (Math.Abs(angle - 90) > 1) // 1 degree tolerance
                    return false;
            }

            return true;
        }

        private bool IsLShape(List<Point2D> boundary)
        {
            // L-shape typically has 6 vertices with specific angle pattern
            if (boundary.Count != 6) return false;

            int rightAngles = 0;
            int reflex Angles = 0;

            for (int i = 0; i < boundary.Count; i++)
            {
                var p1 = boundary[i];
                var p2 = boundary[(i + 1) % boundary.Count];
                var p3 = boundary[(i + 2) % boundary.Count];

                double angle = CalculateAngle(p1, p2, p3);
                
                if (Math.Abs(angle - 90) < 5) rightAngles++;
                else if (Math.Abs(angle - 270) < 5) reflexAngles++;
            }

            // L-shape has 5 right angles and 1 reflex angle (270°)
            return rightAngles == 5 && reflexAngles == 1;
        }

        private bool IsTShape(List<Point2D> boundary)
        {
            // T-shape typically has 8 vertices
            if (boundary.Count != 8) return false;

            // Similar logic to L-shape but with different angle pattern
            // Simplified check - in real implementation would be more sophisticated
            return true; // Placeholder
        }

        private double CalculateAngle(Point2D p1, Point2D p2, Point2D p3)
        {
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dx2 = p3.X - p2.X;
            double dy2 = p3.Y - p2.Y;

            double angle1 = Math.Atan2(dy1, dx1);
            double angle2 = Math.Atan2(dy2, dx2);

            double angle = (angle2 - angle1) * 180 / Math.PI;
            
            if (angle < 0) angle += 360;

            return angle;
        }

        #endregion
    }

    #region Data Models

    public class Point2D
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class IrregularShapeAnalysis
    {
        public ShapeType ShapeType { get; set; }
        public ComplexityLevel Complexity { get; set; }
        public string SuggestedApproach { get; set; }
    }

    public enum ShapeType
    {
        Rectangular,
        LShape,
        TShape,
        Irregular
    }

    public enum ComplexityLevel
    {
        Simple,
        Moderate,
        Complex
    }

    public class RebarLayoutSuggestion
    {
        public string MainDirection { get; set; }
        public string DistributionDirection { get; set; }
        public List<string> SpecialRequirements { get; set; }
    }

    public class Opening
    {
        public string Id { get; set; }
        public double Size { get; set; } // mm
        public Point2D Center { get; set; }
    }

    public class OpeningReinforcementPlan
    {
        public List<OpeningReinforcement> Reinforcements { get; set; }

        public OpeningReinforcementPlan()
        {
            Reinforcements = new List<OpeningReinforcement>();
        }
    }

    public class OpeningReinforcement
    {
        public string OpeningId { get; set; }
        public double Size { get; set; }
        public bool TrimmerBarsRequired { get; set; }
        public int TrimmerBarDiameter { get; set; }
        public int TrimmerBarCount { get; set; }
        public double AdditionalLength { get; set; }
        public string Notes { get; set; }
        public bool RequiresDetailedAnalysis { get; set; }
    }

    #endregion
}
