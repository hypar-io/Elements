using System;
using System.Collections.Generic;
using System.IO;
using Elements;

namespace Elements.Serialization.DXF
{
    /// <summary>
    /// This class is capable of rendering a model to DXF using an internal list of dxfCreators
    /// </summary>
    public class ModelToDxf
    {
        private Dictionary<Type, IRenderDxf> _dxfCreators = new Dictionary<Type, IRenderDxf>();

        /// <summary>
        /// Construct a ModelToDxf instance with all of the built in DXFCreators.
        /// </summary>
        public ModelToDxf()
        {
            _dxfCreators.Add(typeof(Floor), new FloorToDXF());
        }

        /// <summary>
        /// Renders the model in dxf to the returned stream.
        /// </summary>
        /// <param name="model">The model to render</param>
        public Stream Render(Model model)
        {
            var doc = new netDxf.DxfDocument(netDxf.Header.DxfVersion.AutoCad2018);
            var context = new DxfRenderContext();
            context.Model = model;

            foreach (var element in model.Elements.Values)
            {
                if (!_dxfCreators.TryGetValue(element.GetType(), out var converter))
                {
                    continue;
                }
                if (converter.TryToCreateDxfEntity(element, context, out var entity))
                {
                    doc.AddEntity(entity);
                }
            }

            var stream = new MemoryStream();
            doc.Save(stream);

            return stream;
        }
    }
}
