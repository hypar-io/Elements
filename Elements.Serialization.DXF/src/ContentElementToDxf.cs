using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Serialization.DXF.Extensions;
using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
using IxMilia.Dxf.Entities;

namespace Elements.Serialization.DXF
{
    /// <summary>
    /// Convert a floor to a dxf entity.
    /// </summary>
    public class ContentElementToDXF : DxfConverter<ContentElement>
    {
        /// <summary>
        /// Create a DXF Entity Object for a ContentElement.
        /// </summary>        
        public override void TryAddDxfEntity(DxfFile doc, ContentElement contentElement, DxfRenderContext context)
        {
            try
            {
                var chosenSymbol = pickSymbolByContext(contentElement, context);
                if (chosenSymbol == null)
                {
                    Console.WriteLine($"Symbol for {contentElement.Id} was null");
                    chosenSymbol = generateSymbolFromBoundingBox(contentElement, context);
                    return;
                }
                // TODO: make all this handle await?
                var geometry = chosenSymbol.GetGeometryAsync().GetAwaiter().GetResult();
                if (geometry == null)
                {
                    Console.WriteLine($"Failed to get geometry for {contentElement.Id}");
                    return;
                }
                var polygons = geometry.OfType<Polygon>().Select(p => p.ToDxf()).Where(e => e != null).ToList();
                var polylines = geometry.OfType<Polyline>().Select(p => p.ToDxf()).Where(e => e != null).ToList();
                var entities = new List<DxfEntity>(polygons.Union(polylines));
                if (entities.Count() == 0)
                {
                    Console.WriteLine($"No entities for {contentElement.Id}");
                    return;
                }
                var blockName = contentElement.GetBlockName();
                var block = new DxfBlock
                {
                    BasePoint = contentElement.Transform.ToDxfPoint(context),
                    Name = blockName
                };
                doc.BlockRecords.Add(new DxfBlockRecord(blockName));

                foreach (var p in entities)
                {
                    block.Entities.Add(p);
                }
                AddElementToLayer(doc, contentElement, entities, context);
                doc.Blocks.Add(block);
                // if it's not being used as an element definition, 
                // add an instance of it to the drawing.
                if (!contentElement.IsElementDefinition)
                {
                    var insert = new DxfInsert
                    {
                        Name = blockName,
                        Location = contentElement.Transform.ToDxfPoint(context),
                    };
                    doc.Entities.Add(insert);
                    AddElementToLayer(doc, contentElement, new[] { insert }, context);
                }
            }
            catch (Exception e)
            {
                //TODO: implement exception logging
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private Symbol generateSymbolFromBoundingBox(ContentElement contentElement, DxfRenderContext context)
        {
            var bbox = contentElement.BoundingBox;
            var min = bbox.Min;
            var max = bbox.Max;
            var polygon = new Polygon((min.X, min.Y), (max.X, min.Y), (max.X, max.Y), (min.X, max.Y));
            // TODO: handle context orientation
            return new Symbol(new GeometryReference(null, new List<object> { polygon }), SymbolCameraPosition.Top);
        }

        private Symbol pickSymbolByContext(ContentElement contentElement, DxfRenderContext context)
        {
            var contentTransform = contentElement.Transform;
            var drawingRangeTransform = context.DrawingRange?.Transform ?? new Transform();
            //TODO — pick an appropriate symbol based on the context orientation

            var symbol = contentElement?.Symbols?.FirstOrDefault(s => s.CameraPosition == SymbolCameraPosition.Top);
            return symbol;
        }
    }
}