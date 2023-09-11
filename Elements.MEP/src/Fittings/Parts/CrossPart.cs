using glTFLoader.Schema;
using System.Collections.Generic;

namespace Elements.Fittings
{
    public class CrossPart
    {
        public double PipeDiameter { get; set; }

        public double PipeLength { get; set; }

        public double PipeExtension { get; set; }

        public double BranchDiameter1 { get; set; }

        public double BranchLength1 { get; set; }

        public double Angle1 { get; set; }

        public double BranchExtension1 { get; set; }

        public double BranchDiameter2 { get; set; }

        public double BranchLength2 { get; set; }

        public double Angle2 { get; set; }

        public double BranchExtension2 { get; set; }

        public static List<CrossPart> LoadFromCSV(string path, Units.LengthUnit unitInCSV = Units.LengthUnit.Meter)
        {
            var parts = FittingCatalog.LoadFittingPartsFromCsv(path, (fields, map) =>
            {
                var part = new CrossPart();
                var isSuccessful = true;
                var conversion = Units.GetConversionToMeters(unitInCSV);

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(PipeDiameter), map, fields, out var diameter);
                part.PipeDiameter = diameter * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(PipeLength), map, fields, out var pipeLength);
                part.PipeLength = pipeLength * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(PipeExtension), map, fields, out var extension, 0);
                part.PipeExtension = extension * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(BranchDiameter1), map, fields, out var branchDiameter1);
                part.BranchDiameter1 = branchDiameter1 * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(BranchLength1), map, fields, out var branchLength1);
                part.BranchLength1 = branchLength1 * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Angle1), map, fields, out var angle1);
                part.Angle1 = angle1;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(BranchExtension1), map, fields, out var extension1, 0);
                part.BranchExtension1 = extension * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(BranchDiameter2), map, fields, out var branchDiameter2);
                part.BranchDiameter2 = branchDiameter2 * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(BranchLength2), map, fields, out var branchLength2);
                part.BranchLength2 = branchLength2 * conversion;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(Angle2), map, fields, out var angle2);
                part.Angle2 = angle2;

                isSuccessful &= FittingCatalog.TryReadDouble(nameof(BranchExtension2), map, fields, out var extension2, 0);
                part.BranchExtension2 = extension * conversion;

                return isSuccessful ? part : null;
            });
            return parts;
        }
    }
}