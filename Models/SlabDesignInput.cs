namespace RebarAutomation.Models
{
    /// <summary>
    /// Represents input parameters for slab rebar design
    /// </summary>
    public class SlabDesignInput
    {
        // Dimensions
        public double Length { get; set; } // in mm
        public double Width { get; set; } // in mm
        public double Thickness { get; set; } // in mm

        // Loads
        public double DeadLoad { get; set; } // kN/m²
        public double LiveLoad { get; set; } // kN/m²
        public double FloorFinishLoad { get; set; } // kN/m²

        // Material Properties
        public ConcreteGrade ConcreteGrade { get; set; }
        public SteelGrade SteelGrade { get; set; }

        // Cover Requirements
        public double TopCover { get; set; } // mm
        public double BottomCover { get; set; } // mm
        public double SideCover { get; set; } // mm

        // Design Preferences
        public int[] PreferredBarDiameters { get; set; } // e.g., [10, 12, 16, 20]
        public double MaxBarSpacing { get; set; } // mm
        public double MinBarSpacing { get; set; } // mm

        // Support Conditions
        public SupportType SupportType { get; set; }
        public bool IsOneWaySlab { get; set; }
        public bool IsTwoWaySlab { get; set; }

        public SlabDesignInput()
        {
            // Default values
            ConcreteGrade = ConcreteGrade.M25;
            SteelGrade = SteelGrade.Fe415;
            TopCover = 25;
            BottomCover = 25;
            SideCover = 25;
            PreferredBarDiameters = new int[] { 10, 12, 16, 20 };
            MaxBarSpacing = 300;
            MinBarSpacing = 100;
            FloorFinishLoad = 1.0; // Default 1 kN/m²
        }
    }

    public enum ConcreteGrade
    {
        M20, M25, M30, M35, M40, M45, M50
    }

    public enum SteelGrade
    {
        Fe415, Fe500, Fe550
    }

    public enum SupportType
    {
        SimpleSupport,
        FixedSupport,
        ContinuousSupport
    }
}
