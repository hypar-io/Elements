using System.Collections.Generic;

namespace Elements.Fittings
{
    public class PipePart
    {
        public double Diameter { get; set; }

        public static List<PipePart> LoadFromCSV(string path, Units.LengthUnit unitInCSV = Units.LengthUnit.Meter)
        {
            var parts = FittingCatalog.LoadFittingPartsFromCsv(path, (fields, map) =>
            {
                var part = new PipePart();
                var isSuccessful = true;
                var conversion = Units.GetConversionToMeters(unitInCSV);

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Diameter), map, fields, out var diameter);
                part.Diameter = diameter * conversion;

                return isSuccessful ? part : null;
            });
            return parts;
        }
    }
}