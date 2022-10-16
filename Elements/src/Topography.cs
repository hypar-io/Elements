using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using LibTessDotNet.Double;
using Newtonsoft.Json;
using Vertex = Elements.Geometry.Vertex;

namespace Elements
{
    /// <summary>
    /// A topographic mesh defined by an array of elevation values.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/TopographyTests.cs?name=example)]
    /// </example>
    public class Topography : MeshElement, ITessellate
    {
        private double _minElevation = double.PositiveInfinity;

        private double _maxElevation = double.NegativeInfinity;

        private double _depthBelowMinimumElevation = 10;

        private double? _absoluteMinimumElevation;

        internal List<Vertex> _baseVerts = new List<Vertex>();

        /// <summary>
        /// The maximum elevation of the topography.
        /// </summary>
        public double MaxElevation => _maxElevation;

        /// <summary>
        /// The minimum elevation of the topography.
        /// </summary>
        public double MinElevation => _minElevation;

        /// <summary>
        /// A flat list of elevation data which is used to generate the topographic
        /// mesh's vertices. The elevations will be used with the RowWidth property
        /// to convert the flat list into a square grid.
        /// </summary>
        public double[] Elevations { get; }

        /// <summary>
        /// The origin of the topography.
        /// </summary>
        public Vector3 Origin { get; }

        /// <summary>
        /// The number of cells 'across' the topography.
        /// </summary>
        public int RowWidth { get; }

        /// <summary>
        /// The width of a cell.
        /// </summary>
        public double CellWidth { get; }

        /// <summary>
        /// The height of a cell.
        /// </summary>
        public double CellHeight { get; }

        /// <summary>
        /// The depth of the the topography's mass below the topography's minimum elevation.
        /// </summary>
        /// <value></value>
        public double DepthBelowMinimumElevation
        {
            get { return _depthBelowMinimumElevation; }
            set
            {
                if (value != _depthBelowMinimumElevation)
                {
                    _depthBelowMinimumElevation = value < 1 ? 1.0 : value;
                    RaisePropertyChanged("DepthBelowMinimumElevation");
                }
            }
        }

        /// <summary>
        /// The absolute minimum elevation of the topography's mass.
        /// </summary>
        /// <value>If this value is not null, DepthBelowMinimumElevation will be ignored.</value>
        public double? AbsoluteMinimumElevation
        {
            get { return _absoluteMinimumElevation; }
            set
            {
                if (value != null && value != _absoluteMinimumElevation)
                {
                    _absoluteMinimumElevation = Math.Min(value.Value, MinElevation - 1);
                    RaisePropertyChanged("AbsoluteMinimumElevation");
                }
            }
        }

        /// <summary>
        /// Create a topography.
        /// </summary>
        /// <param name="origin">The origin of the topography.</param>
        /// <param name="width">The width the topography. When constructed from a set of elevations, the
        /// width and the length of the topography will be the same.</param>
        /// <param name="elevations">An array of elevation samples which will be converted to a square array of width.</param>
        /// <param name="material">The topography's material.</param>
        /// <param name="transform">The topography's transform.</param>
        /// <param name="id">The topography's id.</param>
        /// <param name="name">The topography's name.</param>
        public Topography(Vector3 origin,
                          double width,
                          double[] elevations,
                          Material material = null,
                          Transform transform = null,
                          Guid id = default(Guid),
                          string name = null) : base(material != null ? material : BuiltInMaterials.Topography,
                                                     transform != null ? transform : new Transform(),
                                                     false,
                                                     id == null ? Guid.NewGuid() : id,
                                                     name)
        {
            this.Origin = origin;
            this.Elevations = elevations;
            this.RowWidth = (int)Math.Sqrt(elevations.Length);
            this.CellWidth = width / (this.RowWidth - 1);
            this.CellHeight = this.CellWidth;
            ConstructMeshes();
            RegisterPropertyChangeHandlers();
        }

        [JsonConstructor]
        internal Topography(Mesh mesh,
                            double[] elevations,
                            Vector3 origin,
                            int rowWidth,
                            double cellWidth,
                            double cellHeight,
                            Material material,
                            Transform transform,
                            Guid id,
                            string name) : base(material,
                                                transform,
                                                false,
                                                id,
                                                name)
        {
            this.Elevations = elevations;
            this.Origin = origin;
            this.RowWidth = rowWidth;
            this.CellWidth = cellWidth;
            this.CellHeight = cellHeight;
            if (mesh == null)
            {
                ConstructMeshes();
            }
            else
            {
                this.Mesh = mesh;
                var bbox = new BBox3(mesh.Vertices.Select(v => v.Position));
                this._minElevation = bbox.Min.Z;
                this._maxElevation = bbox.Max.Z;
            }
            RegisterPropertyChangeHandlers();
        }

        /// <summary>
        /// Create a topography from a custom mesh. It is assumed that the mesh is an open mesh that's roughly parallel to the XY plane.
        /// </summary>
        /// <param name="mesh">The mesh geometry of the topography.</param>
        /// <param name="material">The topography's material.</param>
        /// <param name="transform">The topography's transform.</param>
        /// <param name="id">The topography's id.</param>
        /// <param name="name">The topography's name.</param>
        /// <returns></returns>
        public Topography(Mesh mesh, Material material, Transform transform, Guid id, string name) : base(material,
                                                                                                            transform,
                                                                                                            false,
                                                                                                            id,
                                                                                                            name)
        {
            var newMesh = new Mesh(mesh);
            var bbox = new BBox3(mesh.Vertices.Select(v => v.Position));
            this._minElevation = bbox.Min.Z;
            this._maxElevation = bbox.Max.Z;
            double absoluteMinimumElevation = this.AbsoluteMinimumElevation ?? this.MinElevation - this.DepthBelowMinimumElevation;
            var nakedBoundaries = mesh.GetNakedBoundaries();
            var basePlane = new Plane((0, 0, absoluteMinimumElevation), (0, 0, 1));
            foreach (var polygon in nakedBoundaries)
            {
                // construct bottom
                var bottomPolygon = polygon.Project(basePlane);
                var tess = new Tess
                {
                    NoEmptyPolygons = true
                };
                tess.AddContour(bottomPolygon.Reversed().ToContourVertexArray());
                tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);
                var faceMesh = tess.ToMesh(normal: (0, 0, -1));
                this._baseVerts = new List<Vertex>(faceMesh.Vertices);
                newMesh.AddMesh(faceMesh);
                // construct sides
                var upperSegments = polygon.Segments();
                var lowerSegments = bottomPolygon.Segments();
                for (int i = 0; i < upperSegments.Count(); i++)
                {
                    var topEdge = upperSegments[i];
                    var bottomEdge = lowerSegments[i];
                    var normal = topEdge.Direction().Cross(Vector3.ZAxis).Unitized();
                    var a = newMesh.AddVertex(topEdge.Start, normal: normal);
                    var b = newMesh.AddVertex(topEdge.End, normal: normal);
                    var c = newMesh.AddVertex(bottomEdge.End, normal: normal);
                    var d = newMesh.AddVertex(bottomEdge.Start, normal: normal);
                    newMesh.AddTriangle(a, c, b);
                    newMesh.AddTriangle(a, d, c);
                }
            }
            this.Mesh = newMesh;
        }

        internal void RegisterPropertyChangeHandlers()
        {
            this.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DepthBelowMinimumElevation" || args.PropertyName == "AbsoluteMinimumElevation")
                {
                    var minHeight = this.AbsoluteMinimumElevation ?? this.MinElevation - this.DepthBelowMinimumElevation;
                    MoveBaseVerts(minHeight);
                }
            };
        }
        internal void ConstructMeshes()
        {

            var generatedMesh = GenerateMesh(this.Elevations,
                                               this.Origin,
                                               this.RowWidth,
                                               this.CellWidth,
                                               this.CellWidth);
            this._mesh = generatedMesh.Mesh;
            this._minElevation = generatedMesh.MinElevation;
            this._maxElevation = generatedMesh.MaxElevation;
            double absoluteMinimumElevation = this.AbsoluteMinimumElevation ?? this.MinElevation - this.DepthBelowMinimumElevation;
            CreateSidesAndBottomMesh(this._mesh,
                                     this.RowWidth,
                                     absoluteMinimumElevation,
                                     this.CellHeight,
                                     this.CellWidth,
                                     this.Origin);
        }

        private static (Mesh Mesh, double MaxElevation, double MinElevation) GenerateMesh(
            double[] elevations,
            Vector3 origin,
            int rowWidth,
            double cellWidth,
            double cellHeight)
        {
            var minElevation = double.MaxValue;
            var maxElevation = double.MinValue;
            var mesh = new Mesh();

            var r = new Random();

            for (var y = 0; y < rowWidth; y++)
            {
                for (var x = 0; x < rowWidth; x++)
                {
                    var ei = x + y * rowWidth;
                    var el = elevations[ei];
                    minElevation = Math.Min(minElevation, el);
                    maxElevation = Math.Max(maxElevation, el);

                    var u = (double)x / (double)(rowWidth - 1);
                    var v = (double)y / (double)(rowWidth - 1);

                    // Shrink the UV space slightly to avoid
                    // visible edges on applied textures.
                    var uvTol = 0.001;
                    u = u == 0.0 ? uvTol : u;
                    v = v == 0.0 ? uvTol : v;
                    u = u == 1.0 ? 1 - uvTol : u;
                    v = v == 1.0 ? 1 - uvTol : v;

                    var uv = new UV(u, v);

                    // Add the tiniest amount of fuzz to avoid the
                    // triangles being identified as coplanar during
                    // operations like CSG.
                    var fuzz = r.NextDouble() * 0.0001;
                    mesh.AddVertex(origin + new Vector3(x * cellWidth, y * cellHeight, el + fuzz), uv: uv);

                    if (y > 0 && x > 0)
                    {
                        var i = x + y * rowWidth;

                        // Top triangle
                        var a = mesh.Vertices[i];
                        var b = mesh.Vertices[i - 1];
                        var c = mesh.Vertices[i - rowWidth];
                        mesh.AddTriangle(a, b, c);

                        // Bottom triangle
                        var d = mesh.Vertices[i - 1];
                        var e = mesh.Vertices[i - 1 - rowWidth];
                        var f = mesh.Vertices[i - rowWidth];
                        mesh.AddTriangle(d, e, f);
                    }
                }
            }

            mesh.ComputeNormals();
            return (mesh, maxElevation, minElevation);
        }

        private void CreateSidesAndBottomMesh(Mesh mesh,
                                                     int rowWidth,
                                                     double depth,
                                                     double cellHeight,
                                                     double cellWidth,
                                                     Vector3 origin)
        {
            // Track the last created "low"
            // point so that we can merge
            // vertices in the side meshes.
            Vertex lastL = null;
            Vertex lastR = null;
            Vertex lastT = null;
            Vertex lastB = null;

            this._baseVerts.Clear();

            for (var u = 0; u < rowWidth - 1; u++)
            {
                // Left side
                Vertex l1;
                var i1 = u * rowWidth;
                var v1Existing = mesh.Vertices[i1];
                var v1 = mesh.AddVertex(v1Existing.Position, normal: Vector3.XAxis.Negate());
                if (lastL != null)
                {
                    l1 = lastL;
                }
                else
                {
                    var p = new Vector3(v1.Position.X, v1.Position.Y, depth);
                    l1 = mesh.AddVertex(p);
                    _baseVerts.Add(l1);
                }

                var i2 = i1 + rowWidth;
                var v2Existing = mesh.Vertices[i2];
                var v2 = mesh.AddVertex(v2Existing.Position);

                var pl2 = new Vector3(v2.Position.X, v2.Position.Y, depth);
                var l2 = mesh.AddVertex(pl2);
                _baseVerts.Add(l2);
                lastL = l2;

                mesh.AddTriangle(l1, v1, v2);
                mesh.AddTriangle(l2, l1, v2);

                // Right side
                Vertex l3;
                var i3 = u * (rowWidth) + (rowWidth - 1);
                var v3Existing = mesh.Vertices[i3];
                var v3 = mesh.AddVertex(v3Existing.Position, normal: Vector3.XAxis);

                if (lastR != null)
                {
                    l3 = lastR;
                }
                else
                {
                    var p = new Vector3(v3.Position.X, v3.Position.Y, depth);
                    l3 = mesh.AddVertex(p);
                    _baseVerts.Add(l3);
                }

                var i4 = i3 + rowWidth;
                var v4Existing = mesh.Vertices[i4];
                var v4 = mesh.AddVertex(v4Existing.Position);
                var pl4 = new Vector3(v4.Position.X, v4.Position.Y, depth);
                var l4 = mesh.AddVertex(pl4);
                _baseVerts.Add(l4);
                lastR = l4;

                mesh.AddTriangle(l3, v4, v3);
                mesh.AddTriangle(l3, l4, v4);

                // Top side
                Vertex l5;
                var i5 = u;
                var v5Existing = mesh.Vertices[i5];
                var v5 = mesh.AddVertex(v5Existing.Position, normal: Vector3.YAxis);
                if (lastT != null)
                {
                    l5 = lastT;
                }
                else
                {
                    var p = new Vector3(v5.Position.X, v5.Position.Y, depth);
                    l5 = mesh.AddVertex(p);
                    _baseVerts.Add(l5);
                }

                var i6 = i5 + 1;
                var v6Existing = mesh.Vertices[i6];
                var v6 = mesh.AddVertex(v6Existing.Position);
                var pl6 = new Vector3(v6.Position.X, v6.Position.Y, depth);
                var l6 = mesh.AddVertex(pl6);
                _baseVerts.Add(l6);
                lastT = l6;

                mesh.AddTriangle(l5, v6, v5);
                mesh.AddTriangle(l5, l6, v6);

                // Bottom side
                Vertex l7;
                var i7 = rowWidth * rowWidth - u - 1;
                var v7Existing = mesh.Vertices[i7];
                var v7 = mesh.AddVertex(v7Existing.Position, normal: Vector3.YAxis.Negate());

                if (lastB != null)
                {
                    l7 = lastB;
                }
                else
                {
                    var p = new Vector3(v7.Position.X, v7.Position.Y, depth);
                    l7 = mesh.AddVertex(p);
                    _baseVerts.Add(l7);
                }

                var i8 = i7 - 1;
                var v8Existing = mesh.Vertices[i8];
                var v8 = mesh.AddVertex(v8Existing.Position);
                var pl8 = new Vector3(v8.Position.X, v8.Position.Y, depth);
                var l8 = mesh.AddVertex(pl8);
                _baseVerts.Add(l8);
                lastB = l8;

                mesh.AddTriangle(l7, v8, v7);
                mesh.AddTriangle(l7, l8, v8);
            }

            // Add the bottom
            var bb1 = mesh.AddVertex(origin + new Vector3(0, 0, depth));
            var bb2 = mesh.AddVertex(origin + new Vector3((rowWidth - 1) * cellWidth, 0, depth));
            var bb3 = mesh.AddVertex(origin + new Vector3((rowWidth - 1) * cellWidth, (rowWidth - 1) * cellHeight, depth));
            var bb4 = mesh.AddVertex(origin + new Vector3(0, (rowWidth - 1) * cellHeight, depth));

            _baseVerts.AddRange(new[] { bb1, bb2, bb3, bb4 });

            mesh.AddTriangle(bb1, bb3, bb2);
            mesh.AddTriangle(bb1, bb4, bb3);

            mesh.ComputeNormals();
        }

        /// <summary>
        /// Average the vertex placement along the specified edge
        /// of this topography with the vertex placement along the
        /// corresponding edge of a target topography.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="edgeToAverage"></param>
        public void AverageEdges(Topography target, Units.CardinalDirection edgeToAverage)
        {
            if (this.RowWidth != target.RowWidth)
            {
                throw new ArgumentException("The specified topographies do not have the same number of vertices.");
            }

            Vertex[] e1 = null;
            Vertex[] e2 = null;

            switch (edgeToAverage)
            {
                case Units.CardinalDirection.North:
                    e1 = this.GetEdgeVertices(Units.CardinalDirection.North);
                    e2 = target.GetEdgeVertices(Units.CardinalDirection.South);
                    break;
                case Units.CardinalDirection.South:
                    e1 = this.GetEdgeVertices(Units.CardinalDirection.South);
                    e2 = target.GetEdgeVertices(Units.CardinalDirection.North);
                    break;
                case Units.CardinalDirection.East:
                    e1 = this.GetEdgeVertices(Units.CardinalDirection.East);
                    e2 = target.GetEdgeVertices(Units.CardinalDirection.West);
                    break;
                case Units.CardinalDirection.West:
                    e1 = this.GetEdgeVertices(Units.CardinalDirection.West);
                    e2 = target.GetEdgeVertices(Units.CardinalDirection.East);
                    break;
            }

            for (var i = 0; i < e1.Length; i++)
            {
                var pos = e1[i].Position.Average(e2[i].Position);
                var normal = ((e1[i].Normal + e2[i].Normal) / 2).Unitized();
                e1[i].Position = pos;
                e1[i].Normal = normal;
                e2[i].Position = pos;
                e2[i].Normal = normal;
            }
        }

        /// <summary>
        /// Get the vertices along the specified edge of a square topography.
        /// </summary>
        /// <param name="direction">The edge of vertices to return.</param>
        /// <returns>A collection of vertices.</returns>
        public Vertex[] GetEdgeVertices(Units.CardinalDirection direction)
        {
            var range = Enumerable.Range(0, this.RowWidth);
            var start = 0;
            var increment = 1;
            switch (direction)
            {
                case Units.CardinalDirection.North:
                    start = this.Mesh.Vertices.Count - this.RowWidth;
                    break;
                case Units.CardinalDirection.South:
                    start = 0;
                    break;
                case Units.CardinalDirection.East:
                    start = this.RowWidth - 1;
                    increment = this.RowWidth;
                    break;
                case Units.CardinalDirection.West:
                    start = 0;
                    increment = this.RowWidth;
                    break;
                default:
                    return null;
            }
            return range.Select(i => this.Mesh.Vertices[start + i * increment]).ToArray();
        }

        /// <summary>
        /// Trim the topography with the specified perimeter.
        /// </summary>
        /// <param name="perimeter">The perimeter of the trimmed topography.</param>
        public void Trim(Polygon perimeter)
        {
            // This creates a tall trimming object which is extruded
            // from the zero plane but then transformed in -Z by half its
            // elevation to assure a trim through the object.

            var topoCsg = this.Mesh.ToCsg();
            var trim = new Extrude(perimeter, 200000, Vector3.ZAxis, false);
            var t = new Transform(0, 0, -100000);
            var trimCsg = trim._solid.ToCsg().Transform(t.ToMatrix4x4());
            topoCsg = trimCsg.Intersect(topoCsg);
            var mesh = new Mesh();
            topoCsg.Tessellate(ref mesh);
            this.Mesh = mesh;

            // We've trimmed the mesh. We now need to
            // reset the min and max elevation.
            SetBaseVerts();
            SetMinAndMaxElevation();
        }

        /// <summary>
        /// Cut a tunnel through a topography.
        /// </summary>
        /// <param name="path">The path of the tunnel.</param>
        /// <param name="diameter">The diameter of the tunnel.</param>
        public void Tunnel(Curve path, double diameter)
        {
            if (diameter < 0.0)
            {
                throw new ArgumentException("Diameter must be greater than 0.0.", "diameter");
            }

            var topoCsg = this.Mesh.ToCsg();

            var profile = new Circle(diameter / 2).ToPolygon(20);
            var sweep = new Sweep(profile, path, 0.0, 0.0, 0.0, false);
            var tunnelCsg = sweep._solid.ToCsg();

            topoCsg = topoCsg.Subtract(tunnelCsg);
            var mesh = new Mesh();
            topoCsg.Tessellate(ref mesh);
            this.Mesh = mesh;

            SetBaseVerts();
            SetMinAndMaxElevation();
        }

        /// <summary>
        /// Cut and or fill the topography with the specified perimeter to the specified elevation.
        /// </summary>
        /// <param name="perimeters">The perimeters of the cut and fill areas.</param>
        /// <param name="elevation">The final elevation of the cut and fill.</param>
        /// <param name="batterAngle">The angle of the battering surrounding the fill area in degrees.</param>
        /// <param name="fills">Meshes representing the fill volume in the topography.</param>
        /// <param name="cuts">Meshes representing the cut volume in the topography.</param>
        /// <returns>The sum of all cut and fill volumes.</returns>
        public (double CutVolume, double FillVolume) CutAndFill(IList<Polygon> perimeters, double elevation, out List<Mesh> cuts, out List<Mesh> fills, double batterAngle = 45.0)
        {
            if (this.AbsoluteMinimumElevation == null || elevation < this.AbsoluteMinimumElevation)
            {
                // Push the depth of the topography down past
                // the required elevation.
                this.AbsoluteMinimumElevation = elevation - 1;
            }

            if (batterAngle < 0.0)
            {
                throw new ArgumentException("The batter angle must be greater than 0.0", "batterAngle");
            }

            cuts = new List<Mesh>();
            fills = new List<Mesh>();

            var topoCsg = this.Mesh.ToCsg();
            foreach (var p in perimeters)
            {
                topoCsg = CutAndFillInternal(topoCsg, elevation, p, cuts, fills, batterAngle);
            }

            var mesh = new Mesh();
            topoCsg.Tessellate(ref mesh);
            this.Mesh = mesh;
            mesh.MapUVsToBounds();

            return (cuts.Sum(c => c.Volume()), fills.Sum(f => f.Volume()));
        }

        private Csg.Solid CutAndFillInternal(Csg.Solid topoCsg, double elevation, Polygon perimeter, List<Mesh> cuts, List<Mesh> fills, double batterAngle = 45.0)
        {
            // This check isn't perfect. Sites with topography may have
            // areas where the elevation is lower, and areas where the elevation
            // is higher.
            if (elevation < this.MaxElevation)
            {
                // Cut
                var cutSolid = new Extrude(perimeter, 100000, Vector3.ZAxis, false);
                var t = new Transform(0, 0, elevation);
                // Transform the cut volume to the elevation.
                var cutCsg = cutSolid.Solid.ToCsg().Transform(t.ToMatrix4x4());

                // Calculate the volume of the cut
                // as the union of the two solids.
                var cutXsect = topoCsg.Intersect(cutCsg);
                var cut = new Mesh();
                cutXsect.Tessellate(ref cut);
                cuts.Add(cut);

                topoCsg = topoCsg.Subtract(cutCsg);
            }

            if (elevation > this.MinElevation)
            {
                // Fill
                var height = elevation - this.MinElevation;

                var fillSolid = new Extrude(perimeter, height, Vector3.ZAxis, false);
                // Transform the fill volume down to the minimum elevation
                // and fill up to the elevation.
                var fillT = new Transform(0, 0, this.MinElevation);
                var csgT = fillT.ToMatrix4x4();
                var fillCsg = fillSolid.Solid.ToCsg().Transform(csgT);

                var batterWidth = height * Math.Cos(Units.DegreesToRadians(batterAngle));
                var batterProfile = new Polygon(new[]{
                    Vector3.Origin,
                    new Vector3(batterWidth, 0),
                    new Vector3(0, height)
                });

                // Calculate the whole fill by adding the battering.
                var batterSweep = new Sweep(batterProfile, perimeter, 0, 0, 0, false);
                var batterCsg = batterSweep.Solid.ToCsg().Transform(csgT);
                fillCsg = fillCsg.Union(batterCsg);

                var xsect = fillCsg.Subtract(topoCsg);
                var fill = new Mesh();
                xsect.Tessellate(ref fill);
                fills.Add(fill);
                topoCsg = topoCsg.Union(fillCsg);
            }

            return topoCsg;
        }

        /// <summary>
        /// Subtract the provided solid from this topography.
        /// </summary>
        /// <param name="solid">The solid to subtract.</param>
        /// <param name="transform">A transform applied to the solid before intersection.</param>
        /// <returns>An array of triangles that have at least one vertex inside the solid.</returns>
        private void Subtract(Solid solid, Transform transform = null)
        {
            var intersects = new List<Triangle>();
            for (var i = this._mesh.Triangles.Count - 1; i >= 0; i--)
            {
                var t = this._mesh.Triangles[i];
                var xsect = GetIntersectionType(t, solid, transform);
                if (xsect == IntersectionType.Intersect)
                {
                    intersects.Add(t);
                    this._mesh.Triangles.RemoveAt(i);
                }
                else if (xsect == IntersectionType.Inside)
                {
                    this._mesh.Triangles.RemoveAt(i);
                }
            }

            foreach (var t in intersects)
            {
                var input = new List<Vector3>();
                foreach (var v in t.Vertices)
                {
                    input.Add(v.Position);
                }

                foreach (var f in solid.Faces.Values)
                {
                    var fp = f.Plane();
                    if (transform != null)
                    {
                        fp = transform.OfPlane(fp);
                    }
                    input = SutherlandHodgman(input, fp, transform);
                }

                var contour1 = t.ToContourVertexArray();
                input.Reverse();
                var contour2 = input.ToContourVertexArray();

                var tess = new Tess();
                tess.NoEmptyPolygons = true;
                tess.AddContour(contour1);
                tess.AddContour(contour2);
                tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

                try
                {
                    for (var i = 0; i < tess.ElementCount; i++)
                    {
                        var a = MergeOrReturnNew(tess.Vertices[tess.Elements[i * 3]].Position.ToVector3());
                        var b = MergeOrReturnNew(tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3());
                        var c = MergeOrReturnNew(tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3());
                        this._mesh.AddTriangle(a, b, c);
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            for (var i = this._mesh.Triangles.Count - 1; i >= 0; i--)
            {
                var t = this._mesh.Triangles[i];
                var area = t.Area();
                if (area == 0.0 ||
                    double.IsNaN(area) ||
                    area < Vector3.EPSILON ||
                    t.Normal.IsNaN())
                {
                    this._mesh.Triangles.RemoveAt(i);
                }
            }
        }

        private enum IntersectionType
        {
            Inside, Outside, Intersect, Unknown
        }

        private Vertex MergeOrReturnNew(Vector3 v)
        {
            foreach (var vx in this._mesh.Vertices)
            {
                if (vx.Position.IsAlmostEqualTo(v))
                {
                    return vx;
                }
            }
            var newVtx = this._mesh.AddVertex(v, new UV());
            return newVtx;
        }

        private IntersectionType GetIntersectionType(Triangle t, Solid s, Transform trans = null)
        {
            var fCount = s.Faces.Count;
            var inside = 0;
            var outside = 0;
            var xsect = 0;
            foreach (var f in s.Faces.Values)
            {
                var p = f.Plane();
                if (trans != null)
                {
                    p = trans.OfPlane(p);
                }
                var d1 = p.Normal.Dot(t.Vertices[0].Position - p.Origin);
                var d2 = p.Normal.Dot(t.Vertices[1].Position - p.Origin);
                var d3 = p.Normal.Dot(t.Vertices[2].Position - p.Origin);

                if (d1 < 0 && d2 < 0 && d3 < 0)
                {
                    inside++;
                }
                else if (d1 > 0 && d2 > 0 && d3 > 0)
                {
                    outside++;
                }
                else
                {
                    xsect++;
                }
            }

            if (outside == fCount)
            {
                return IntersectionType.Outside;
            }
            else if (inside == fCount)
            {
                return IntersectionType.Inside;
            }
            else if (inside + xsect == fCount)
            {
                return IntersectionType.Intersect;
            }

            return IntersectionType.Unknown;
        }

        private List<Vector3> SutherlandHodgman(List<Vector3> input, Plane p, Transform trans)
        {
            // Implement Sutherland-Hodgman clipping
            // https://www.cs.drexel.edu/~david/Classes/CS430/Lectures/L-05_Polygons.6.pdf
            var output = new List<Vector3>();

            for (var j = 0; j < input.Count; j++)
            {
                // edge of the triangle
                var start = input[j];
                var end = input[j == input.Count - 1 ? 0 : j + 1];
                var d1 = p.Normal.Dot(start - p.Origin);
                var d2 = p.Normal.Dot(end - p.Origin);

                if (d1 < 0 && d2 < 0)
                {
                    //both inside
                    output.Add(start);
                }
                else if (d1 < 0 && d2 > 0)
                {
                    //start inside
                    //end outside
                    //add intersection
                    if (new Line(start, end).Intersects(p, out Vector3 result))
                    {
                        output.Add(start);
                        output.Add(result);
                    }
                }
                else if (d1 > 0 && d2 > 0)
                {
                    //both outside
                }
                else if (d1 > 0 && d2 < 0)
                {
                    //start outside
                    //end inside
                    //add intersection
                    if (new Line(start, end).Intersects(p, out Vector3 result))
                    {
                        output.Add(result);
                    }
                }
            }

            return output;
        }

        private void MoveBaseVerts(double newElevation)
        {
            foreach (var v in this._baseVerts)
            {
                v.Position = new Vector3(v.Position.X, v.Position.Y, newElevation);
            }
        }

        private void SetBaseVerts()
        {
            var minHeight = this.AbsoluteMinimumElevation.HasValue ? this.AbsoluteMinimumElevation.Value : this.MinElevation - this.DepthBelowMinimumElevation;
            this._baseVerts.Clear();
            foreach (var v in this.Mesh.Vertices)
            {
                if (v.Position.Z.ApproximatelyEquals(minHeight))
                {
                    this._baseVerts.Add(v);
                }
            }
        }

        private void SetMinAndMaxElevation()
        {
            // Ignore any vertices that have the depth of the sides.
            var max = double.MinValue;
            var min = double.MaxValue;
            foreach (var v in this.Mesh.Vertices)
            {
                if (!_baseVerts.Contains(v))
                {
                    if (v.Position.Z > max)
                    {
                        max = v.Position.Z;
                    }
                    if (v.Position.Z < min)
                    {
                        min = v.Position.Z;
                    }
                }
            }
            this._minElevation = min;
            this._maxElevation = max;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            return;
        }
    }

    internal static class TopographyExtensions
    {
        internal static ContourVertex[] ToContourVertexArray(this List<Vector3> pts)
        {
            var contour = new ContourVertex[pts.Count];
            for (var i = 0; i < contour.Length; i++)
            {
                var v = pts[i];
                contour[i] = new ContourVertex();
                contour[i].Position = new Vec3 { X = v.X, Y = v.Y, Z = v.Z };
            }
            return contour;
        }
    }
}