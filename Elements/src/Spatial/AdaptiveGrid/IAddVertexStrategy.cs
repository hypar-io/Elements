using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Interface for adding new vertices based on chosen strategy.
    /// </summary>
    public interface IAddVertexStrategy
    {
        /// <summary>
        /// Function called by grid were new vertices are added and connected.
        /// </summary>
        /// <param name="grid">Grid to add vertices into</param>
        /// <param name="position">Position of base vertex.</param>
        /// <param name="cut">Should new edges be automatically cut with the rest of the grid.</param>
        /// <returns></returns>
        Vertex Add(AdaptiveGrid grid, Vector3 position, bool cut);
    }

    /// <summary>
    /// Connect strategy creates edges between new vertex created from base position and any number of given vertices.
    /// </summary>
    public class ConnectVertexStrategy : IAddVertexStrategy 
    {
        /// <summary>
        /// Create new Connect strategy.
        /// </summary>
        /// <param name="connections">Vertices to connect with.</param>
        public ConnectVertexStrategy(params Vertex[] connections)
        {
            _connections = connections;
        }

        /// <summary>
        /// Function called by grid were new vertices are added and connected.
        /// </summary>
        /// <param name="grid">Grid to add vertices into</param>
        /// <param name="position">Position of base vertex.</param>
        /// <param name="cut">Should new edges be automatically cut with the rest of the grid.</param>
        /// <returns></returns>
        public Vertex Add(AdaptiveGrid grid, Vector3 position, bool cut)
        {
            if (_connections == null || !_connections.Any())
            {
                throw new ArgumentException("Vertex should be connected to at least one other Vertex");
            }

            Vertex v = grid.AddVertex(position);
            foreach (var c in _connections)
            {
                if (v.Id != c.Id)
                {
                    grid.AddEdge(v, c, cut);
                }
            }

            return v;
        }

        private Vertex[] _connections;
    }

    /// <summary>
    /// ConnectWithAngle strategy that connects two points in a way so incoming edge of other vertex has certain angle with given direction.
    /// Creates one middle vertex to achieve this but it can be skipped if two points are already aligned.
    /// </summary>
    public class ConnectVertexWithAngleStrategy : IAddVertexStrategy
    {
        /// <summary>
        /// Create new ConnectWithAngle strategy.
        /// </summary>
        /// <param name="other">Other position to connect.</param>
        /// <param name="direction">Reference direction.</param>
        /// <param name="angle">Required angle between edge incoming into other vertex and referenced direction.</param>
        public ConnectVertexWithAngleStrategy(Vector3 other, Vector3 direction, double angle)
        {
            _other = other;
            _angle = angle;
            _direction = direction.Unitized();
        }

        /// <summary>
        /// Function called by grid were new vertices are added and connected.
        /// </summary>
        /// <param name="grid">Grid to add vertices into</param>
        /// <param name="position">Position of base vertex.</param>
        /// <param name="cut">Should new edges be automatically cut with the rest of the grid.</param>
        /// <returns></returns>
        public Vertex Add(AdaptiveGrid grid, Vector3 position, bool cut)
        {
            var d = _other - position;
            var delta = d.Dot(_direction);
            Vector3 middlePoint = new Vector3();
            var dot = d.Unitized().Dot(_direction);

            var startVertex = grid.AddVertex(position);
            _endVertex = grid.AddVertex(_other);

            if (Math.Abs(dot).ApproximatelyEquals(1) || dot.ApproximatelyEquals(0))
            {
                grid.AddEdge(startVertex, _endVertex, cut);
                return startVertex;
            }

            if(_angle.ApproximatelyEquals(0))
            {
                middlePoint = _other - _direction * delta;
            }
            else if(_angle.ApproximatelyEquals(90))
            {
                middlePoint = position + _direction * delta;
            }
            else
            {
                var tan = Math.Tan(Units.DegreesToRadians(_angle));
                var p = _other - _direction * delta;
                var ortho = p - position;
                var orthoDelta = ortho.Length();
                var orthoDistance = Math.Abs(delta * tan);
                if (orthoDistance < Math.Abs(orthoDelta) + Vector3.EPSILON)
                {
                    middlePoint = p - ortho.Unitized() * orthoDistance;
                }
                else
                {
                    var smallDelta = orthoDelta * tan;
                    bool sign = delta > 0;
                    middlePoint = position + (sign ? _direction : _direction.Negate()) * (Math.Abs(delta) - smallDelta);
                }
            }

            if (middlePoint.IsAlmostEqualTo(startVertex.Point, grid.Tolerance) ||
                middlePoint.IsAlmostEqualTo(_endVertex.Point, grid.Tolerance))
            {
                grid.AddEdge(startVertex, _endVertex , cut);
            }
            else
            {
                _middleVertex = grid.AddVertex(middlePoint);
                grid.AddEdge(_middleVertex, _endVertex, cut);
                grid.AddEdge(startVertex, _middleVertex, cut);
            }

            return startVertex;
        }

        /// <summary>
        /// Created middle vertex.
        /// </summary>
        public Vertex MiddleVertex
        {
            get { return _middleVertex; }
        }

        /// <summary>
        /// Created end vertex.
        /// </summary>
        public Vertex EndVertex
        {
            get { return _endVertex; }
        }

        private Vector3 _other;
        private double _angle;
        private Vector3 _direction;
        private Vertex _middleVertex = null;
        private Vertex _endVertex = null;
    }

}
