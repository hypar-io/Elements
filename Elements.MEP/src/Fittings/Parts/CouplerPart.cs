using System.Collections.Generic;

namespace Elements.Fittings
{
    public class CouplerPart
    {
        public double Diameter { get; set; }

        public double Length { get; set; }

        public double Extension { get; set; }

        public static List<CouplerPart> LoadFromCSV(string path, Units.LengthUnit unitInCSV = Units.LengthUnit.Meter)
        {
            var parts = FittingCatalog.LoadFittingPartsFromCsv(path, (fields, map) =>
            {
                var part = new CouplerPart();
                var isSuccessful = true;
                var conversion = Units.GetConversionToMeters(unitInCSV);

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Diameter), map, fields, out var diameter);
                part.Diameter = diameter * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Length), map, fields, out var length);
                part.Length = length * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Extension), map, fields, out var extension, 0);
                part.Extension = extension * conversion;

                return isSuccessful ? part : null;
            });
            return parts;
        }
    }
}