using System;
using System.Collections.Generic;
using System.IO;
using Elements;
using Microsoft.Win32.SafeHandles;

namespace Elements.DXF
{
    public class ModelToDxf
    {
        private Dictionary<Type, IRenderDxf> _dxfCreator = new Dictionary<Type, IRenderDxf>();

        public ModelToDxf()
        {
            _dxfCreator.Add(typeof(Floor), new FloorToDXF());
        }

        public Stream Render(Model model)
        {
            var doc = new netDxf.DxfDocument(netDxf.Header.DxfVersion.AutoCad2018);
            var context = new DxfRenderContext();
            context.Model = model;

            foreach (var element in model.Elements.Values)
            {
                if (!_dxfCreator.TryGetValue(element.GetType(), out var converter))
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
