using System.Collections.Generic;

namespace Elements.Fittings
{
    public class TeePart
    {
        public double Diameter { get; set; }

        public double Extension { get; set; }

        public double BranchDiameter { get; set; }

        public double BranchExtension { get; set; }

        public double Angle { get; set; }

        public double TrunkLength { get; set; }

        public double MainLength { get; set; }

        public double BranchLength { get; set; }

        public static List<TeePart> LoadFromCSV(string path, Units.LengthUnit unitInCSV = Units.LengthUnit.Meter)
        {
            var parts = FittingCatalog.LoadFittingPartsFromCsv(path, (fields, map) =>
            {
                var part = new TeePart();
                var isSuccessful = true;
                var conversion = Units.GetConversionToMeters(unitInCSV);

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Diameter), map, fields, out var diameter);
                part.Diameter = diameter * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Extension), map, fields, out var extension, 0);
                part.Extension = extension * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(BranchDiameter), map, fields, out var branchDiameter);
                part.BranchDiameter = branchDiameter * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(BranchExtension), map, fields, out var branchExtension, 0);
                part.BranchExtension = branchExtension * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Angle), map, fields, out var angle);
                part.Angle = angle;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(TrunkLength), map, fields, out var trunkLength);
                part.TrunkLength = trunkLength * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(MainLength), map, fields, out var mainLength);
                part.MainLength = mainLength * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(BranchLength), map, fields, out var branchLength);
                part.BranchLength = branchLength * conversion;

                return isSuccessful ? part : null;
            });
            return parts;
        }
    }
}