using Antlr4.Runtime;
using Elements.Geometry;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    internal class IfcRepresentationDataExtractor
    {
        private readonly List<IIfcRepresentationParser> _ifcRepresentationParsers;
        private readonly MaterialExtractor _materialExtractor;

        public IfcRepresentationDataExtractor(MaterialExtractor materialExtractor)
        {
            _ifcRepresentationParsers = new List<IIfcRepresentationParser>();
            _materialExtractor = materialExtractor;
        }

        public void AddRepresentationParser(IIfcRepresentationParser ifcRepresentationParser)
        {
            _ifcRepresentationParsers.Add(ifcRepresentationParser);
        }

        public RepresentationData ExtractRepresentationData(IfcProduct ifcProduct)
        {
            var rep = ifcProduct.Representation;

            if (rep == null) 
            {
                return null;
            }

            var repItems = rep.Representations.SelectMany(r => r.Items);
            var representation = ParseRepresentationItems(repItems);
            representation.Transform = GetTransformFromIfcProduct(ifcProduct);
            return representation;
        }

        public RepresentationData ParseRepresentationItems(IEnumerable<IfcRepresentationItem> repItems)
        {
            var parsedItems = new List<RepresentationData>();

            foreach (var repItem in repItems)
            {
                var material = _materialExtractor.ExtractMaterial(repItem);
                var matchingParsers = _ifcRepresentationParsers.Where(parser => parser.Matches(repItem));

                if (!matchingParsers.Any())
                {
                    // TODO: We don't parse some representation types. The list is below.
                    continue;
                }

                var repParser = matchingParsers.First();
                var parsedItem = repParser.ParseRepresentationItem(repItem);

                if (parsedItem == null)
                {
                    continue;
                }

                parsedItem.Material = material ?? parsedItem.Material;
                parsedItems.Add(parsedItem);
            }

            var repData = new RepresentationData(parsedItems);
            return repData;
        }

        private static Transform GetTransformFromIfcProduct(IfcProduct ifcProduct)
        {
            var transform = new Transform();
            transform.Concatenate(ifcProduct.ObjectPlacement.ToTransform());

            if (!(ifcProduct is IfcBuildingElement ifcBuildingElement))
            {
                return transform;
            }

            // Check if the building element is contained in a building storey
            foreach (var cis in ifcBuildingElement.ContainedInStructure)
            {
                transform.Concatenate(cis.RelatingStructure.ObjectPlacement.ToTransform());
            }

            return transform;
        }
    }
}
