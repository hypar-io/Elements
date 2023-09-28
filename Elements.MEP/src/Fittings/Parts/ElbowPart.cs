using System.Collections.Generic;

namespace Elements.Fittings
{
    public class ElbowPart
    {
        public double Diameter { get; set; }

        public double Angle { get; set; }

        public double SideLength { get; set; }

        public double Extension { get; set; }

        public static List<ElbowPart> LoadFromCSV(string path, Units.LengthUnit unitInCSV = Units.LengthUnit.Meter)
        {
            var parts = FittingCatalog.LoadFittingPartsFromCsv(path, (fields, map) =>
            {
                var part = new ElbowPart();
                var isSuccessful = true;
                var conversion = Units.GetConversionToMeters(unitInCSV);

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Diameter), map, fields, out var diameter);
                part.Diameter = diameter * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Angle), map, fields, out var angle);
                part.Angle = angle;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(SideLength), map, fields, out var sideLength);
                part.SideLength = sideLength * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Extension), map, fields, out var extension, 0);
                part.Extension = extension * conversion;

                return isSuccessful ? part : null;
            });
            return parts;
        }
    }
}