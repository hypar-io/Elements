using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Spatial;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Elements
{
    /// <summary>
    /// A tiled ceiling.
    /// </summary>
    public class TiledCeiling : BaseCeiling
    {
        private const string _tileCellName = "Tile";
        private const string _spaceCellName = "Spacing";
        private const double _minDistance = 0.001;

        private Grid2d _grid;
        private List<Polygon> _tiles;

        /// <summary>
        /// The tile width.
        /// </summary>
        public double TileWidth { get; protected set; }

        /// <summary>
        /// The tile length.
        /// </summary>
        public double TileLength { get; protected set; }

        /// <summary>
        /// The space between tiles.
        /// </summary>
        public double SpaceBetween { get; protected set; }

        /// <summary>
        /// The grid orientation.
        /// </summary>
        public Transform GridOrientation { get; protected set; }

        /// <summary>
        /// Construct a ceiling from tiles.
        /// </summary>
        /// <param name="perimeter">The plan profile of the ceiling. It must lie on the XY plane.
        /// The Z coordinate will be ignored</param>
        /// <param name="elevation">The elevation of the ceiling.</param>
        /// <param name="tileWidth">The tile width.</param>
        /// <param name="tileLength">The tile length.</param>
        /// <param name="spaceBetween">The space between tiles.</param>
        /// <param name="gridOrientation">The grid orientation.</param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An optional transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        public TiledCeiling(Polygon perimeter,
                      double elevation,
                      double tileWidth,
                      double tileLength,
                      double spaceBetween,
                      Transform gridOrientation = null,
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      Guid id = default(Guid),
                      string name = null)
            : base(perimeter, elevation, material, transform, representation, isElementDefinition, id, name)
        {
            if (tileWidth < _minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(tileWidth), $"The ceiling could not be created. The tile width must be greater than or equal to {_minDistance}.");
            }
            if (tileLength < _minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(tileLength), $"The ceiling could not be created. The tile length must be greater than or equal to {_minDistance}.");
            }
            if (spaceBetween < _minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(spaceBetween), $"The ceiling could not be created. The space between tiles must be greater than or equal to {_minDistance}.");
            }

            this.TileWidth = tileWidth;
            this.TileLength = tileLength;
            this.SpaceBetween = spaceBetween;
            this.GridOrientation = gridOrientation;
        }

        /// <summary>
        /// Construct a ceiling from tiles.
        /// </summary>
        /// <param name="perimeter">The plan perimeter of the ceiling. It must lie on the XY plane.
        /// Z coordinate will be used as elevation</param>
        /// <param name="tileWidth">The tile width.</param>
        /// <param name="tileLength">The tile length.</param>
        /// <param name="spaceBetween">The space between tiles.</param>
        /// <param name="gridOrientation">The grid orientation.</param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An optional transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        public TiledCeiling(Polygon perimeter,
                      double tileWidth,
                      double tileLength,
                      double spaceBetween,
                      Transform gridOrientation = null,
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      Guid id = default(Guid),
                      string name = null)
            : base(perimeter, material, transform, representation, isElementDefinition, id, name)
        {
            if (tileWidth < _minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(tileWidth), $"The ceiling could not be created. The tile width must be greater than or equal to {_minDistance}.");
            }
            if (tileLength < _minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(tileLength), $"The ceiling could not be created. The tile length must be greater than or equal to {_minDistance}.");
            }
            if (spaceBetween < _minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(spaceBetween), $"The ceiling could not be created. The space between tiles must be greater than or equal to {_minDistance}.");
            }

            this.TileWidth = tileWidth;
            this.TileLength = tileLength;
            this.SpaceBetween = spaceBetween;
            this.GridOrientation = gridOrientation;
        }

        /// <summary>
        /// Construct a ceiling from tiles. It's a private constructor that doesn't add elevation to transform
        /// </summary>
        /// <param name="perimeter">The plan profile of the ceiling. It must lie on the XY plane.
        /// The Z coordinate will be ignored</param>
        /// <param name="elevation">The elevation of the ceiling.</param>
        /// <param name="tileWidth"></param>
        /// <param name="tileLength"></param>
        /// <param name="spaceBetween"></param>
        /// <param name="gridOrientation"></param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An optional transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        [JsonConstructor]
        protected TiledCeiling(Polygon perimeter,
                      double elevation,
                      double tileWidth,
                      double tileLength,
                      double spaceBetween,
                      Transform gridOrientation = null,
                      Guid id = default(Guid),
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      string name = null)
            : base(perimeter, elevation, id, material, transform, representation, isElementDefinition, name)
        {
            if (tileWidth < _minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(tileWidth), $"The ceiling could not be created. The tile width must be greater than or equal to {_minDistance}.");
            }
            if (tileLength < _minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(tileLength), $"The ceiling could not be created. The tile length must be greater than or equal to {_minDistance}.");
            }
            if (spaceBetween < _minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(spaceBetween), $"The ceiling could not be created. The space between tiles must be greater than or equal to {_minDistance}.");
            }

            this.TileWidth = tileWidth;
            this.TileLength = tileLength;
            this.SpaceBetween = spaceBetween;
            this.GridOrientation = gridOrientation;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();

            var tiles = GetTiles();
            foreach (var tile in tiles)
            {
                Representation.SolidOperations.Add(new Lamina(tile));
            }
        }

        /// <summary>
        /// Get tiles.
        /// </summary>
        /// <returns>List of tiles.</returns>
        public List<Polygon> GetTiles()
        {
            if (_tiles == null)
            {
                _tiles = BuildCeilingTiles();
            }

            return _tiles;
        }

        /// <summary>
        /// Get an underlying grid.
        /// </summary>
        /// <returns>An underlying grid.</returns>
        public Grid2d GetGrid()
        {
            if (_grid == null)
            {
                _grid = BuildGrid();
            }

            return _grid;
        }

        /// <summary>
        /// Get count of U cells.
        /// </summary>
        /// <returns>Count of U cells.</returns>
        public int GetCountOfUCells()
        {
            return GetGrid().U.GetCells().Count(cell => cell.Type?.Contains(_spaceCellName) == false);
        }

        /// <summary>
        /// Get count of V cells.
        /// </summary>
        /// <returns>Count of v cells.</returns>
        public int GetCountOfVCells()
        {
            return GetGrid().V.GetCells().Count(cell => cell.Type?.Contains(_spaceCellName) == false);
        }

        /// <summary>
        /// Get the tile cells as Grid2d.
        /// </summary>
        /// <returns>List of tiles grids.</returns>
        public List<Grid2d> GetTileCells()
        {
            return GetGrid().GetCells()
                    .Where(c => c.Type?.Contains(_spaceCellName) == false)
                    .ToList();
        }

        private List<Polygon> BuildCeilingTiles()
        {
            var cells = GetGrid().GetCells();

            if (cells.Count == 1 && cells[0].IsSingleCell)
            {
                return cells[0].GetTrimmedCellGeometry().OfType<Polygon>().ToList();
            }

            return cells
                    .Where(c => c.Type?.Contains(_spaceCellName) == false)
                    .SelectMany(c => c.GetTrimmedCellGeometry())
                    .OfType<Polygon>()
                    .ToList();
        }

        private Grid2d BuildGrid()
        {
            var grid = GridOrientation == null ? new Grid2d(Perimeter) : new Grid2d(Perimeter, GridOrientation);

            var patternU = new List<(string typeName, double length)>()
            {
                (_tileCellName, TileLength),
                (_spaceCellName, SpaceBetween)
            };
            var patternV = new List<(string typeName, double length)>()
            {
                (_tileCellName, TileWidth),
                (_spaceCellName, SpaceBetween)
            };
            grid.U.DivideByPattern(patternU);
            grid.V.DivideByPattern(patternV);

            var uCells = grid.U.GetCells();
            for (var i = 0; i < uCells.Count; i++)
            {
                uCells[i].Type = patternU[i % patternU.Count].typeName;
            }

            var vCells = grid.V.GetCells();
            for (var i = 0; i < vCells.Count; i++)
            {
                vCells[i].Type = patternV[i % patternV.Count].typeName;
            }

            return grid;
        }
    }
}
