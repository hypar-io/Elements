using System.Collections.Generic;

namespace Elements.Fittings
{
    public class ReducerPart
    {
        public double DiameterLarge { get; set; }

        public double DiameterSmall { get; set; }

        public double Length { get; set; }

        public double ExtensionLarge { get; set; }

        public double ExtensionSmall { get; set; }

        public static List<ReducerPart> LoadFromCSV(string path, Units.LengthUnit unitInCSV = Units.LengthUnit.Meter)
        {
            var parts = FittingCatalog.LoadFittingPartsFromCsv(path, (fields, map) =>
            {
                var part = new ReducerPart();
                var isSuccessful = true;
                var conversion = Units.GetConversionToMeters(unitInCSV);

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(DiameterLarge), map, fields, out var diameterLarge);
                part.DiameterLarge = diameterLarge * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(DiameterSmall), map, fields, out var diameterSmall);
                part.DiameterSmall = diameterSmall * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Length), map, fields, out var length);
                part.Length = length * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(ExtensionLarge), map, fields, out var extensionLarge, 0);
                part.ExtensionLarge = extensionLarge * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(ExtensionSmall), map, fields, out var extensionSmall, 0);
                part.ExtensionSmall = extensionSmall * conversion;

                return isSuccessful ? part : null;
            });
            return parts;
        }
    }
}