using Elements.Geometry;
using IFC;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    /// <summary>Extracts all representation data from IfcProduct using list of IIfcRepresentationParsers.</summary>
    internal class IfcRepresentationDataExtractor
    {
        private readonly List<IIfcRepresentationParser> _ifcRepresentationParsers;

        /// <summary>
        /// Create an IfcRepresentationDataExtractor.
        /// </summary>
        public IfcRepresentationDataExtractor()
        {
            _ifcRepresentationParsers = new List<IIfcRepresentationParser>();
        }

        public void AddRepresentationParser(IIfcRepresentationParser ifcRepresentationParser)
        {
            _ifcRepresentationParsers.Add(ifcRepresentationParser);
        }

        /// <summary>
        /// Parse IfcRepresentationItems of <paramref name="ifcProduct"/> and merge the results
        /// into a single RepresentationData.
        /// </summary>
        /// <param name="ifcProduct">
        /// IfcRepresentationItems of this IfcProduct will be parsed.
        /// </param>
        public RepresentationData ExtractRepresentationData(IfcProduct ifcProduct)
        {
            var rep = ifcProduct.Representation;

            if (rep == null) 
            {
                return null;
            }

            var repItems = rep.Representations.SelectMany(r => r.Items);
            var representation = ParseRepresentationItems(repItems);
            var productTransform = GetTransformFromIfcProduct(ifcProduct);

            if (representation.Transform is not null)
            {
                productTransform.Concatenate(representation.Transform);
            }

            representation.Transform = productTransform;
            return representation;
        }

        /// <summary>
        /// Parse <paramref name="repItem"/> into RepresentationData. Returns null, if the item cannot be parsed.
        /// </summary>
        /// <param name="repItem">IfcRepresentationItem that will be parsed.</param>
        public RepresentationData ParseRepresentationItem(IfcRepresentationItem repItem)
        {
            var matchingParsers = _ifcRepresentationParsers.Where(parser => parser.CanParse(repItem));

            if (!matchingParsers.Any())
            {
                // TODO: There are many representation types that aren't supported now.
                return null;
            }

            var repParser = matchingParsers.First();
            var parsedItem = repParser.ParseRepresentationItem(repItem);
            return parsedItem;
        }

        /// <summary>
        /// Parse <paramref name="repItems"/> and merge results into single RepresentationData.
        /// </summary>
        /// <param name="repItems">IfcRepresentationItems that will be parsed.</param>
        public RepresentationData ParseRepresentationItems(IEnumerable<IfcRepresentationItem> repItems)
        {
            var parsedItems = new List<RepresentationData>();

            foreach (var repItem in repItems)
            {
                var parsedItem = ParseRepresentationItem(repItem);

                if (parsedItem == null)
                {
                    continue;
                }

                parsedItems.Add(parsedItem);
            }

            var repData = new RepresentationData(parsedItems);
            return repData;
        }

        /// <summary>
        /// Parse Transform from <paramref name="ifcProduct"/>.
        /// </summary>
        private static Transform GetTransformFromIfcProduct(IfcProduct ifcProduct)
        {
            var transform = new Transform();
            transform.Concatenate(ifcProduct.ObjectPlacement.ToTransform());

            if (ifcProduct is not IfcBuildingElement ifcBuildingElement)
            {
                return transform;
            }

            // Check if the building element is contained in a building storey
            foreach (var cis in ifcBuildingElement.ContainedInStructure)
            {
                transform.Concatenate(cis.RelatingStructure.ObjectPlacement.ToTransform());
            }

            // Origins of some IfcProducts (like doors and windows) are placed in the corner
            // when in Hypar they are in the center.
            if (ifcProduct is IfcDoor ifcDoor)
            {
                transform = GetTransformWithCorrectedOffset(transform, (IfcLengthMeasure) ifcDoor.OverallWidth);
            } 
            else if (ifcProduct is IfcWindow ifcWindow)
            {
                transform = GetTransformWithCorrectedOffset(transform, (IfcLengthMeasure) ifcWindow.OverallWidth);
            }

            return transform;
        }

        private static Transform GetTransformWithCorrectedOffset(Transform transform, double width)
        {
            var correctedOrigin = transform.Origin + 0.5 * width * transform.XAxis;
            var correctedTransform = new Transform(correctedOrigin,
                                                   transform.XAxis,
                                                   transform.YAxis,
                                                   transform.ZAxis);
            return correctedTransform;
        }
    }
}
