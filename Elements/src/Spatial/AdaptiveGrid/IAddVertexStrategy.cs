using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAddVertexStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        Vertex Add(AdaptiveGrid grid, Vector3 position);
    }

    public class AddConnectVertex : IAddVertexStrategy 
    {
        public AddConnectVertex(params Vertex[] connections)
        {
            _connections = connections;
        }

        public Vertex Add(AdaptiveGrid grid, Vector3 position)
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
                    grid.AddEdge(v.Id, c.Id);
                }
            }

            return v;
        }

        private Vertex[] _connections;
    }

    public class AddConnectVertexWithAngle : IAddVertexStrategy
    {
        public AddConnectVertexWithAngle(Vector3 other, Vector3 direction, double angle)
        {
            _other = other;
            _angle = angle;
            _direction = direction.Unitized();
        }

        public Vertex Add(AdaptiveGrid grid, Vector3 position)
        {
            var d = _other - position;
            var delta = d.Dot(_direction);
            Vector3 middlePoint = new Vector3();
            var dot = d.Unitized().Dot(_direction);

            var startVertex = grid.AddVertex(position);
            _endVertex = grid.AddVertex(_other);

            if (Math.Abs(dot).ApproximatelyEquals(1) || dot.ApproximatelyEquals(0))
            {
                grid.AddCutEdge(startVertex.Id, _endVertex.Id);
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
                grid.AddCutEdge(startVertex.Id, _endVertex.Id);
            }
            else
            {
                _middleVertex = grid.AddVertex(middlePoint);
                grid.AddCutEdge(_middleVertex.Id, _endVertex.Id);
                grid.AddCutEdge(startVertex.Id, _middleVertex.Id);
            }

            return startVertex;
        }

        public Vertex MiddleVertex
        {
            get { return _middleVertex; }
        }

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
