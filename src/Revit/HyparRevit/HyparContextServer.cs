using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Serilog;

namespace Hypar.Revit
{
    public class HyparDirectContextServer : IDirectContext3DServer
    {
        private Guid _serverId = new Guid("d8a24d18-1bf6-466c-b55e-083e8ac4439e");

        private ILogger _logger;

        private Outline _outline;

        private DisplayStyle _lastDisplayStyle = DisplayStyle.Undefined;

        private Dictionary<string, RenderData> _renderDataCache = new Dictionary<string, RenderData>();

        public HyparDirectContextServer(ILogger logger)
        {
            _logger = logger;
        }

        public bool CanExecute(View dBView)
        {
            return dBView.ViewType == ViewType.ThreeD;
        }

        public string GetApplicationId()
        {
            return string.Empty;
        }

        public Outline GetBoundingBox(View dBView)
        {
            return _outline;
        }

        public string GetDescription()
        {
            return "A server for visualizing Hypar workflow data.";
        }

        public string GetName()
        {
            return "Hypar Server";
        }

        public Guid GetServerId()
        {
            return _serverId;
        }

        public ExternalServiceId GetServiceId()
        {
            return ExternalServices.BuiltInExternalServices.DirectContext3DService;
        }

        public string GetSourceId()
        {
            throw new NotImplementedException();
        }

        public string GetVendorId()
        {
            return "HYPR";
        }

        public void RenderScene(View dBView, DisplayStyle displayStyle)
        {
            if (!HyparHubApp.IsSyncing())
            {
                return;
            }

            if (HyparHubApp.CurrentWorkflows == null)
            {
                return;
            }

            // When the display style changes we need to redraw because
            // we'll use a different data layout.
            if (HyparHubApp.RequiresRedraw == false && _lastDisplayStyle == displayStyle)
            {
                // Draw what's in the cached buffers.
                foreach (var renderData in _renderDataCache.Values)
                {
                    DrawContext.FlushBuffer(renderData.VertexBuffer,
                                            renderData.VertexCount,
                                            renderData.IndexBuffer,
                                            renderData.IndexCount,
                                            renderData.VertexFormat,
                                            renderData.Effect,
                                            renderData.PrimitiveType,
                                            0,
                                            renderData.PrimitiveCount);
                }
                return;
            }

            _lastDisplayStyle = displayStyle;

            var executionsToDraw = HyparHubApp.CurrentWorkflows.Values.SelectMany(w => w.FunctionInstances.Where(fi => fi.SelectedOptionExecutionId != null).Select(fi => fi.Id)).ToList();

            if (executionsToDraw.Count == 0)
            {
                _logger.Debug("There were no executions to draw...");
                HyparHubApp.RequiresRedraw = false;
                return;
            }

            _outline = new Outline(new XYZ(), new XYZ());

            // TODO: This is doing way too much drawing!
            // We should be able to only update render data 
            // for executions which are different.
            _renderDataCache.Clear();
            foreach (var workflow in HyparHubApp.CurrentWorkflows.Values)
            {
                foreach (var id in executionsToDraw)
                {
                    var renderDatas = DrawExecution(_logger, workflow.Id, id, _outline, displayStyle);
                    if (renderDatas != null && renderDatas.Count > 0)
                    {
                        for (var i = 0; i < renderDatas.Count; i++)
                        {
                            var renderData = renderDatas[i];
                            _renderDataCache.Add($"{id}_{i}", renderData);
                        }

                    }
                }
            }

            HyparHubApp.RequiresRedraw = false;

            _logger.Debug("Render complete.");
        }

        private static List<RenderData> DrawExecution(ILogger logger,
                                                      string workflowId,
                                                      string executionId,
                                                      Outline outline,
                                                      DisplayStyle displayStyle)
        {
            // Read the workflow from disk
            var execPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".hypar/workflows/{workflowId}/{executionId}/model.json");
            if (!File.Exists(execPath))
            {
                logger.Debug("The execution path {Path} could not be found. Perhaps the workflow was deleted from the cache.", execPath);
                return null;
            }

            var errors = new List<string>();
            var model = Elements.Model.FromJson(File.ReadAllText(execPath), errors);
            foreach (var error in errors)
            {
                logger.Debug("{Error}", error);
            }

            var geoms = model.AllElementsOfType<Elements.GeometricElement>();

            var instances = model.AllElementsOfType<Elements.ElementInstance>();

            logger.Debug("Found {ElementCount} elements.", geoms.Count());
            if (geoms.Count() == 0)
            {
                return null;
            }

            var meshes = new List<Elements.Geometry.Mesh>();
            var edgeSet = new List<List<Elements.Geometry.Line>>();
            var edges = new List<Elements.Geometry.Line>();
            edgeSet.Add(edges);

            // Draw the meshes and edges in batches. Arbitrarily large meshes
            // will overun the max value of ushorts causing data
            // not to show up.
            var meshBatchSize = 500;
            var edgeBatchSize = 1000;
            var mesh = new Elements.Geometry.Mesh();

            var count = 0;
            foreach (var geom in geoms)
            {
                if (geom.Representation == null || geom.Representation.SolidOperations.Count() == 0)
                {
                    continue;
                }

                foreach (var solidOp in geom.Representation.SolidOperations)
                {
                    solidOp.Solid.Tessellate(ref mesh, geom.Transform, geom.Material.Color);
                    foreach (var e in solidOp.Solid.Edges.Values)
                    {
                        edges.Add(new Elements.Geometry.Line(geom.Transform.OfPoint(e.Left.Vertex.Point), geom.Transform.OfPoint(e.Right.Vertex.Point)));
                        if (edges.Count >= edgeBatchSize)
                        {
                            edgeSet.Add(edges);
                            edges = new List<Elements.Geometry.Line>();
                        }
                    }
                }

                count++;

                if (count > meshBatchSize)
                {
                    meshes.Add(mesh);
                    mesh = new Elements.Geometry.Mesh();
                    count = 0;
                }
            }

            foreach (var instance in instances)
            {
                if (instance.BaseDefinition.Representation == null || instance.BaseDefinition.Representation.SolidOperations.Count() == 0)
                {
                    continue;
                }

                foreach (var solidOp in instance.BaseDefinition.Representation.SolidOperations)
                {
                    solidOp.Solid.Tessellate(ref mesh, instance.Transform, instance.BaseDefinition.Material.Color);
                    foreach (var e in solidOp.Solid.Edges.Values)
                    {
                        edges.Add(new Elements.Geometry.Line(instance.Transform.OfPoint(e.Left.Vertex.Point), instance.Transform.OfPoint(e.Right.Vertex.Point)));
                        if (edges.Count >= edgeBatchSize)
                        {
                            edgeSet.Add(edges);
                            edges = new List<Elements.Geometry.Line>();
                        }
                    }
                }

                count++;

                if (count > meshBatchSize)
                {
                    meshes.Add(mesh);
                    mesh = new Elements.Geometry.Mesh();
                    count = 0;
                }
            }
            meshes.Add(mesh);
            edgeSet.Add(edges);

            var renderDatas = new List<RenderData>();
            logger.Debug("There are {MeshCount} meshes to be drawn.", meshes.Count);
            foreach (var subMesh in meshes)
            {
                try
                {
                    if (displayStyle != DisplayStyle.Wireframe)
                    {
                        renderDatas.Add(DrawMesh(subMesh, ref outline, displayStyle));
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message);
                }
            }

            if (displayStyle != DisplayStyle.Shading)
            {
                foreach (var e in edgeSet)
                {
                    renderDatas.Add(DrawEdges(e));
                }
            }

            return renderDatas;
        }

        public static RenderData DrawEdges(List<Elements.Geometry.Line> edges)
        {
            var numVertices = edges.Count * 2;
            var numPrimitives = edges.Count;
            var pType = PrimitiveType.LineList;
            var numIndices = GetPrimitiveSize(pType) * numPrimitives;
            var vertexFormatBits = VertexFormatBits.Position;
            var vertexFormat = new VertexFormat(vertexFormatBits);
            var vBuffer = new VertexBuffer(GetVertexSize(vertexFormatBits) * numVertices);
            var iBuffer = new IndexBuffer(numIndices);
            vBuffer.Map(GetVertexSize(vertexFormatBits) * numVertices);
            iBuffer.Map(numIndices);

            var vertices = new List<VertexPosition>();
            var lines = new List<IndexLine>();

            var index = 0;
            foreach (var e in edges)
            {
                vertices.Add(new VertexPosition(e.Start.ToXYZFeet()));
                vertices.Add(new VertexPosition(e.End.ToXYZFeet()));
                lines.Add(new IndexLine(index, index + 1));
                index += 2;
            }
            var p = vBuffer.GetVertexStreamPosition();
            p.AddVertices(vertices);
            var iPos = iBuffer.GetIndexStreamLine();
            iPos.AddLines(lines);

            vBuffer.Unmap();
            iBuffer.Unmap();

            var effect = new EffectInstance(vertexFormatBits);

            var renderData = new RenderData()
            {
                VertexBuffer = vBuffer,
                VertexCount = numVertices,
                IndexBuffer = iBuffer,
                IndexCount = numIndices,
                VertexFormat = vertexFormat,
                Effect = effect,
                PrimitiveType = pType,
                PrimitiveCount = numPrimitives
            };

            DrawContext.FlushBuffer(vBuffer, numVertices, iBuffer, numIndices, vertexFormat, effect, pType, 0, numPrimitives);

            return renderData;
        }

        public static RenderData DrawMesh(Elements.Geometry.Mesh mesh, ref Outline outline, DisplayStyle displayStyle)
        {
            var min = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue);
            var max = new XYZ(double.MinValue, double.MinValue, double.MinValue);

            var numVertices = mesh.Triangles.Count * 3;
            var numPrimitives = mesh.Triangles.Count;
            var pType = PrimitiveType.TriangleList;
            var numIndices = GetPrimitiveSize(pType) * numPrimitives;
            VertexFormatBits vertexFormatBits;
            switch (displayStyle)
            {
                case DisplayStyle.HLR:
                    vertexFormatBits = VertexFormatBits.Position;
                    break;
                case DisplayStyle.FlatColors:
                    vertexFormatBits = VertexFormatBits.PositionColored;
                    break;
                default:
                    vertexFormatBits = VertexFormatBits.PositionNormalColored;
                    break;
            }
            var vertexFormat = new VertexFormat(vertexFormatBits);

            var vBuffer = new VertexBuffer(GetVertexSize(vertexFormatBits) * numVertices);
            var iBuffer = new IndexBuffer(numIndices);

            vBuffer.Map(GetVertexSize(vertexFormatBits) * numVertices);
            iBuffer.Map(numIndices);

            var verticesHLR = new List<VertexPosition>();
            var verticesFlat = new List<VertexPositionColored>();
            var vertices = new List<VertexPositionNormalColored>();
            var triangles = new List<IndexTriangle>();

            // We duplicate the vertices on each triangle so that
            // we can get the correct number of face normals.
            foreach (var t in mesh.Triangles)
            {
                foreach (var v in t.Vertices)
                {
                    var pos = v.Position.ToXYZFeet();

                    outline.AddPoint(pos);

                    switch (vertexFormatBits)
                    {
                        case VertexFormatBits.Position:
                            verticesHLR.Add(new VertexPosition(pos));
                            break;
                        case VertexFormatBits.PositionColored:
                            verticesFlat.Add(new VertexPositionColored(pos, v.Color.ToColorWithTransparency()));
                            break;
                        default:
                            vertices.Add(new VertexPositionNormalColored(pos, t.Normal.ToXYZ(), v.Color.ToColorWithTransparency()));
                            break;
                    }

                    if (pos.X < min.X && pos.Y < min.Y && pos.Z < min.Z)
                    {
                        min = pos;
                    }
                    if (pos.X > min.X && pos.Y > min.Y && pos.Z > min.Z)
                    {
                        max = pos;
                    }
                }

                triangles.Add(new IndexTriangle(t.Vertices[0].Index, t.Vertices[1].Index, t.Vertices[2].Index));
            }

            switch (displayStyle)
            {
                case DisplayStyle.HLR:
                    var p = vBuffer.GetVertexStreamPosition();
                    p.AddVertices(verticesHLR);
                    break;
                case DisplayStyle.FlatColors:
                    var pc = vBuffer.GetVertexStreamPositionColored();
                    pc.AddVertices(verticesFlat);
                    break;
                default:
                    var pnc = vBuffer.GetVertexStreamPositionNormalColored();
                    pnc.AddVertices(vertices);
                    break;
            }

            var iPos = iBuffer.GetIndexStreamTriangle();
            iPos.AddTriangles(triangles);

            vBuffer.Unmap();
            iBuffer.Unmap();

            var effect = new EffectInstance(vertexFormatBits);
            // There is no reason why this should work.
            // In other situations, 255 is the 'full' component.
            // In the case of hidden line rendering, 0, 0, 0 makes white.
            if (displayStyle == DisplayStyle.HLR)
            {
                var color = new ColorWithTransparency(0, 0, 0, 0);
                effect.SetColor(color.GetColor());
                effect.SetAmbientColor(color.GetColor());
                effect.SetDiffuseColor(color.GetColor());
            }

            // Create a render data for reuse 
            // on non-update calls.
            var renderData = new RenderData()
            {
                VertexBuffer = vBuffer,
                VertexCount = numVertices,
                IndexBuffer = iBuffer,
                IndexCount = numIndices,
                VertexFormat = vertexFormat,
                Effect = effect,
                PrimitiveType = pType,
                PrimitiveCount = numPrimitives
            };

            if (displayStyle != DisplayStyle.Wireframe && numPrimitives > 0)
            {
                DrawContext.FlushBuffer(vBuffer, numVertices, iBuffer, numIndices, vertexFormat, effect, pType, 0, numPrimitives);
            }

            return renderData;
        }

        public static int GetPrimitiveSize(PrimitiveType primitive)
        {
            switch (primitive)
            {
                case PrimitiveType.LineList: return IndexLine.GetSizeInShortInts();
                case PrimitiveType.PointList: return IndexPoint.GetSizeInShortInts();
                case PrimitiveType.TriangleList: return IndexTriangle.GetSizeInShortInts();
                default: break;
            }
            return IndexTriangle.GetSizeInShortInts();
        }

        public static int GetVertexSize(VertexFormatBits format)
        {
            switch (format)
            {
                case VertexFormatBits.Position: return VertexPosition.GetSizeInFloats();
                case VertexFormatBits.PositionColored:
                    return VertexPositionColored.GetSizeInFloats();
                case VertexFormatBits.PositionNormal:
                    return VertexPositionNormal.GetSizeInFloats();
                case VertexFormatBits.PositionNormalColored:
                    return VertexPositionNormalColored.GetSizeInFloats();
                default: break;
            }
            return VertexPosition.GetSizeInFloats();
        }

        public bool UseInTransparentPass(View dBView)
        {
            return false;
        }

        public bool UsesHandles()
        {
            return false;
        }
    }

    /// <summary>
    /// An object which stores render buffer data.
    /// </summary>
    public class RenderData
    {
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public int VertexCount { get; set; }
        public int IndexCount { get; set; }
        public VertexFormat VertexFormat { get; set; }
        public EffectInstance Effect { get; set; }
        public PrimitiveType PrimitiveType { get; set; }
        public int PrimitiveCount { get; set; }
    }
}