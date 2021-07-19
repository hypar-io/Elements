using System;
using System.Collections.Generic;
using System.IO;
using Elements;
using IxMilia.Dxf;

namespace Elements.Serialization.DXF
{
    /// <summary>
    /// This class is capable of rendering a model to DXF using an internal list of dxfCreators
    /// </summary>
    public class ModelToDxf
    {
        /// <summary>
        /// Create a new instance of the renderer.
        /// </summary>
        public ModelToDxf()
        {
            context = new DxfRenderContext();
        }

        private DxfRenderContext context;

        private Dictionary<Type, IRenderDxf> _dxfCreators = new Dictionary<Type, IRenderDxf>
        {
            {typeof(ContentElement), new ContentElementToDXF()},
            {typeof(ElementInstance), new ElementInstanceToDXF()}
        };

        /// <summary>
        /// Renders the model in dxf to the returned stream.
        /// </summary>
        /// <param name="model">The model to render</param>
        public Stream Render(Model model)
        {
            var doc = new DxfFile();
            context.Model = model;

            foreach (var element in model.Elements.Values)
            {
                if (_dxfCreators.TryGetValue(element.GetType(), out var converter))
                {
                    converter.TryAddDxfEntity(doc, element, context);
                }
                else if (element is GeometricElement geomElement)
                {
                    // fall back to geometric converter
                    new GeometricElementToDxf().TryAddDxfEntity(doc, geomElement, context);
                }
            }

            var stream = new MemoryStream();
            doc.Header.Version = DxfAcadVersion.R2013;
            doc.Save(stream);
            return stream;
        }
        /// <summary>
        /// Set the mapping configuration (layer settings, lineweights, etc) for this renderer.
        /// </summary>
        public void SetMappingConfiguration(MappingConfiguration config)
        {
            this.context.MappingConfiguration = config;
        }
    }
}
