namespace RebarAutomation.Models
{
    /// <summary>
    /// Represents the output of slab analysis and design calculations
    /// </summary>
    public class SlabDesignOutput
    {
        // Slab Classification
        public bool IsOneWay { get; set; }
        public bool IsTwoWay { get; set; }
        public double LyLxRatio { get; set; }

        // Design Moments
        public double PositiveMomentX { get; set; } // kNm/m
        public double NegativeMomentX { get; set; } // kNm/m
        public double PositiveMomentY { get; set; } // kNm/m
        public double NegativeMomentY { get; set; } // kNm/m

        // Required Steel Area
        public double AstMainBottom { get; set; } // mm²/m
        public double AstMainTop { get; set; } // mm²/m
        public double AstDistribution { get; set; } // mm²/m

        // Rebar Details - Main Direction
        public int MainBarDiameter { get; set; } // mm
        public double MainBarSpacing { get; set; } // mm
        public int MainBarCount { get; set; }

        // Rebar Details - Distribution Direction
        public int DistBarDiameter { get; set; } // mm
        public double DistBarSpacing { get; set; } // mm
        public int DistBarCount { get; set; }

        // Development Length
        public double DevelopmentLength { get; set; } // mm
        public double LapLength { get; set; } // mm

        // Validation Results
        public bool IsValid { get; set; }
        public List<string> ValidationMessages { get; set; }
        public List<string> Warnings { get; set; }

        // Deflection Check
        public double ActualSpanDepthRatio { get; set; }
        public double AllowableSpanDepthRatio { get; set; }
        public bool DeflectionCheckPassed { get; set; }

        public SlabDesignOutput()
        {
            ValidationMessages = new List<string>();
            Warnings = new List<string>();
            IsValid = true;
        }
    }
}
