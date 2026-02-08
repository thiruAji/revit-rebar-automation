using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using RebarAutomation.Models;

namespace RebarAutomation.RevitIntegration
{
    /// <summary>
    /// Creates and places rebar elements in Revit model
    /// </summary>
    public class RebarPlacer
    {
        private readonly Document _doc;
        private readonly SlabDesignInput _input;
        private readonly SlabDesignOutput _output;

        public RebarPlacer(Document doc, SlabDesignInput input, SlabDesignOutput output)
        {
            _doc = doc;
            _input = input;
            _output = output;
        }

        /// <summary>
        /// Creates complete rebar set for a slab element
        /// </summary>
        public List<Rebar> CreateRebarSet(Floor slab)
        {
            var rebarList = new List<Rebar>();

            using (Transaction trans = new Transaction(_doc, "Create Slab Rebar"))
            {
                trans.Start();

                try
                {
                    // Get slab geometry
                    var slabGeometry = GetSlabGeometry(slab);

                    // Place main bars (bottom)
                    var mainBarsBottom = PlaceMainBars(slab, slabGeometry, true);
                    rebarList.AddRange(mainBarsBottom);

                    // Place main bars (top) if required
                    if (_input.SupportType != SupportType.SimpleSupport)
                    {
                        var mainBarsTop = PlaceMainBars(slab, slabGeometry, false);
                        rebarList.AddRange(mainBarsTop);
                    }

                    // Place distribution bars
                    var distBars = PlaceDistributionBars(slab, slabGeometry);
                    rebarList.AddRange(distBars);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    throw new Exception($"Failed to create rebar: {ex.Message}", ex);
                }
            }

            return rebarList;
        }

        /// <summary>
        /// Places main reinforcement bars
        /// </summary>
        private List<Rebar> PlaceMainBars(Floor slab, SlabGeometry geometry, bool isBottom)
        {
            var rebarList = new List<Rebar>();

            // Get rebar type
            RebarBarType barType = GetRebarBarType(_output.MainBarDiameter);
            if (barType == null)
            {
                throw new Exception($"Rebar bar type for diameter {_output.MainBarDiameter}mm not found");
            }

            // Get rebar hook type (standard 90-degree hook)
            RebarHookType hookType = GetStandardHookType();

            // Determine cover
            double cover = isBottom ? _input.BottomCover : _input.TopCover;

            // Create rebar shape (straight bars)
            RebarShape rebarShape = GetStraightRebarShape();

            // Calculate bar positions
            double spacing = _output.MainBarSpacing;
            int barCount = _output.MainBarCount;

            // Get the shorter span direction
            XYZ direction = geometry.ShortSpanDirection;
            XYZ perpDirection = geometry.LongSpanDirection;
            
            double barLength = geometry.LongSpanLength;

            // Create bars along the short span
            for (int i = 0; i < barCount; i++)
            {
                double offset = i * spacing / 304.8; // Convert mm to feet
                XYZ startPoint = geometry.Origin + perpDirection * offset;
                XYZ endPoint = startPoint + direction * (barLength / 304.8);

                // Create curve for rebar
                Curve barCurve = Line.CreateBound(startPoint, endPoint);
                List<Curve> curves = new List<Curve> { barCurve };

                // Create rebar
                Rebar rebar = Rebar.CreateFromCurves(
                    _doc,
                    RebarStyle.Standard,
                    barType,
                    hookType,
                    hookType,
                    slab,
                    geometry.Normal,
                    curves,
                    RebarHookOrientation.Right,
                    RebarHookOrientation.Left,
                    true,
                    true
                );

                if (rebar != null)
                {
                    // Set rebar properties
                    SetRebarProperties(rebar, isBottom ? "M1" : "M2", _output.MainBarDiameter, spacing);
                    rebarList.Add(rebar);
                }
            }

            return rebarList;
        }

        /// <summary>
        /// Places distribution bars perpendicular to main bars
        /// </summary>
        private List<Rebar> PlaceDistributionBars(Floor slab, SlabGeometry geometry)
        {
            var rebarList = new List<Rebar>();

            RebarBarType barType = GetRebarBarType(_output.DistBarDiameter);
            if (barType == null)
            {
                throw new Exception($"Rebar bar type for diameter {_output.DistBarDiameter}mm not found");
            }

            RebarHookType hookType = GetStandardHookType();
            RebarShape rebarShape = GetStraightRebarShape();

            double spacing = _output.DistBarSpacing;
            int barCount = _output.DistBarCount;

            // Distribution bars run perpendicular to main bars
            XYZ direction = geometry.LongSpanDirection;
            XYZ perpDirection = geometry.ShortSpanDirection;
            
            double barLength = geometry.ShortSpanLength;

            for (int i = 0; i < barCount; i++)
            {
                double offset = i * spacing / 304.8;
                XYZ startPoint = geometry.Origin + perpDirection * offset;
                XYZ endPoint = startPoint + direction * (barLength / 304.8);

                Curve barCurve = Line.CreateBound(startPoint, endPoint);
                List<Curve> curves = new List<Curve> { barCurve };

                Rebar rebar = Rebar.CreateFromCurves(
                    _doc,
                    RebarStyle.Standard,
                    barType,
                    hookType,
                    hookType,
                    slab,
                    geometry.Normal,
                    curves,
                    RebarHookOrientation.Right,
                    RebarHookOrientation.Left,
                    true,
                    true
                );

                if (rebar != null)
                {
                    SetRebarProperties(rebar, "D1", _output.DistBarDiameter, spacing);
                    rebarList.Add(rebar);
                }
            }

            return rebarList;
        }

        /// <summary>
        /// Sets rebar properties (mark, spacing, etc.)
        /// </summary>
        private void SetRebarProperties(Rebar rebar, string mark, int diameter, double spacing)
        {
            // Set bar mark
            Parameter markParam = rebar.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
            if (markParam != null && !markParam.IsReadOnly)
            {
                markParam.Set(mark);
            }

            // Set comments (diameter and spacing info)
            Parameter commentParam = rebar.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            if (commentParam != null && !commentParam.IsReadOnly)
            {
                commentParam.Set($"φ{diameter}mm @ {spacing}mm c/c");
            }
        }

        /// <summary>
        /// Gets rebar bar type by diameter
        /// </summary>
        private RebarBarType GetRebarBarType(int diameter)
        {
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            var barTypes = collector.OfClass(typeof(RebarBarType)).Cast<RebarBarType>();

            // Try to find exact match by diameter
            foreach (var barType in barTypes)
            {
                Parameter diamParam = barType.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
                if (diamParam != null)
                {
                    double barDiameter = diamParam.AsDouble() * 304.8; // Convert feet to mm
                    if (Math.Abs(barDiameter - diameter) < 0.5)
                    {
                        return barType;
                    }
                }
            }

            // If not found, return first available
            return barTypes.FirstOrDefault();
        }

        /// <summary>
        /// Gets standard hook type (90 degrees)
        /// </summary>
        private RebarHookType GetStandardHookType()
        {
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            var hookTypes = collector.OfClass(typeof(RebarHookType)).Cast<RebarHookType>();
            
            // Look for standard 90-degree hook
            return hookTypes.FirstOrDefault(h => h.Name.Contains("90")) ?? hookTypes.FirstOrDefault();
        }

        /// <summary>
        /// Gets straight rebar shape
        /// </summary>
        private RebarShape GetStraightRebarShape()
        {
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            var shapes = collector.OfClass(typeof(RebarShape)).Cast<RebarShape>();
            
            return shapes.FirstOrDefault(s => s.Name.Contains("00")) ?? shapes.FirstOrDefault();
        }

        /// <summary>
        /// Extracts slab geometry information
        /// </summary>
        private SlabGeometry GetSlabGeometry(Floor slab)
        {
            var geometry = new SlabGeometry();

            // Get slab face
            Options opt = new Options();
            opt.ComputeReferences = true;
            GeometryElement geomElem = slab.get_Geometry(opt);

            foreach (GeometryObject geomObj in geomElem)
            {
                Solid solid = geomObj as Solid;
                if (solid != null && solid.Faces.Size > 0)
                {
                    // Get top face
                    PlanarFace topFace = null;
                    foreach (Face face in solid.Faces)
                    {
                        PlanarFace pf = face as PlanarFace;
                        if (pf != null && pf.FaceNormal.Z > 0.9)
                        {
                            topFace = pf;
                            break;
                        }
                    }

                    if (topFace != null)
                    {
                        geometry.Normal = topFace.FaceNormal;
                        
                        // Get bounding box
                        BoundingBoxUV bbox = topFace.GetBoundingBox();
                        UV min = bbox.Min;
                        UV max = bbox.Max;

                        XYZ p1 = topFace.Evaluate(min);
                        XYZ p2 = topFace.Evaluate(new UV(max.U, min.V));
                        XYZ p3 = topFace.Evaluate(max);

                        double length1 = p1.DistanceTo(p2) * 304.8; // Convert to mm
                        double length2 = p2.DistanceTo(p3) * 304.8;

                        if (length1 < length2)
                        {
                            geometry.ShortSpanLength = length1;
                            geometry.LongSpanLength = length2;
                            geometry.ShortSpanDirection = (p2 - p1).Normalize();
                            geometry.LongSpanDirection = (p3 - p2).Normalize();
                        }
                        else
                        {
                            geometry.ShortSpanLength = length2;
                            geometry.LongSpanLength = length1;
                            geometry.ShortSpanDirection = (p3 - p2).Normalize();
                            geometry.LongSpanDirection = (p2 - p1).Normalize();
                        }

                        geometry.Origin = p1;
                    }
                }
            }

            return geometry;
        }
    }

    /// <summary>
    /// Helper class to store slab geometry information
    /// </summary>
    public class SlabGeometry
    {
        public XYZ Origin { get; set; }
        public XYZ Normal { get; set; }
        public XYZ ShortSpanDirection { get; set; }
        public XYZ LongSpanDirection { get; set; }
        public double ShortSpanLength { get; set; } // mm
        public double LongSpanLength { get; set; } // mm
    }
}
