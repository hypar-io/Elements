using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;
using Xunit;

namespace Elements.Tests
{
    public class RoutingHintLineTests
    {
        [Fact]
        public void IsNearby2D()
        {
            var polyline = new Polyline(new Vector3[] { (0, 0), (0, 5), (5, 5) });
            var hint_1 = new RoutingHintLine(polyline, factor: 1, influence: 1, userDefined: true, is2D: true);
            var hint_0 = new RoutingHintLine(polyline, factor: 1, influence: 0, userDefined: true, is2D: true);

            //Point not near polyline
            Assert.False(hint_0.IsNearby((5, 0)));
            Assert.False(hint_1.IsNearby((5, 0)));

            //Point on elevation
            Assert.True(hint_0.IsNearby((0, 2, 10)));
            Assert.True(hint_1.IsNearby((0, 2, -10)));

            //Point close
            Assert.False(hint_0.IsNearby((3, 5.1)));
            Assert.True(hint_1.IsNearby((3, 5.1)));

            //On influence edge
            Assert.True(hint_1.IsNearby((1, 3)));

            //Point very close
            Assert.True(hint_0.IsNearby((3, 5.000000001)));
            Assert.True(hint_1.IsNearby((3, 5.000000001)));
        }

        [Fact]
        public void IsNearby3D()
        {
            var polyline = new Polyline(new Vector3[] { (0, 0), (0, 5), (0, 5, 5) });
            var hint_1 = new RoutingHintLine(polyline, factor: 1, influence: 1, userDefined: true, is2D: false);
            var hint_0 = new RoutingHintLine(polyline, factor: 1, influence: 0, userDefined: true, is2D: false);

            //Point not near polyline
            Assert.False(hint_0.IsNearby((5, 0)));
            Assert.False(hint_1.IsNearby((5, 0)));

            //Point on elevation
            Assert.False(hint_0.IsNearby((0, 2, 10)));
            Assert.False(hint_1.IsNearby((0, 2, -10)));
            Assert.True(hint_1.IsNearby((0, 2, 0.5)));

            //Point close
            Assert.False(hint_0.IsNearby((0.1, 5.1, 3)));
            Assert.True(hint_1.IsNearby((0.1, 5.1, 3)));

            //On influence edge
            Assert.True(hint_1.IsNearby((1, 5, 3)));

            //Point very close
            Assert.True(hint_0.IsNearby((0.000000001, 4)));
            Assert.True(hint_1.IsNearby((0.000000001, 4)));
        }

        [Fact]
        public void Affects2D()
        {
            var polyline = new Polyline(new Vector3[] { (5, 0), (0, 0), (0, 5), (5, 5) });
            var hint_1 = new RoutingHintLine(polyline, factor: 1, influence: 1, userDefined: true, is2D: true);
            var hint_0 = new RoutingHintLine(polyline, factor: 1, influence: 0, userDefined: true, is2D: true);

            //Line not near polyline
            Assert.False(hint_0.Affects((5, 0), (5, 3)));
            Assert.False(hint_1.Affects((5, 0), (5, 3)));

            //Line on elevation
            Assert.True(hint_0.Affects((0, 2, 10), (0, 4, 10)));
            Assert.True(hint_1.Affects((0, 2, -10), (0, 4, -10)));

            //Line close
            Assert.False(hint_0.Affects((3, 5.1), (4, 5.1)));
            Assert.True(hint_1.Affects((3, 5.1), (4, 5.1)));

            //Line influence edge
            Assert.True(hint_1.Affects((1, 3), (1, 4)));

            //Line very close
            Assert.True(hint_0.Affects((3, 5.000000001), (4, 5.000000001)));
            Assert.True(hint_1.Affects((3, 5.000000001), (4, 5.000000001)));

            //Vertical Line
            Assert.False(hint_0.Affects((3, 5), (3, 5, 1)));
            Assert.False(hint_1.Affects((3, 5), (3, 5, 1)));

            //Perpendicular line inside influence
            Assert.False(hint_1.Affects((5, 0), (5, 0.5)));

            //Lays on different segments
            Assert.False(hint_0.Affects((5, 0), (5, 5)));
            Assert.False(hint_1.Affects((5.1, 0), (5.1, 5)));
        }

        [Fact]
        public void Affects3D()
        {
            var polyline = new Polyline(new Vector3[] { (0, 0, 5), (0, 0), (0, 5), (0, 5, 5) });
            var hint_1 = new RoutingHintLine(polyline, factor: 1, influence: 1, userDefined: true, is2D: false);
            var hint_0 = new RoutingHintLine(polyline, factor: 1, influence: 0, userDefined: true, is2D: false);

            //Line not near polyline
            Assert.False(hint_0.Affects((5, 0), (5, 3)));
            Assert.False(hint_1.Affects((5, 0), (5, 3)));

            //Line on elevation
            Assert.False(hint_0.Affects((0, 2, 10), (0, 4, 10)));
            Assert.False(hint_1.Affects((0, 2, -10), (0, 4, -10)));
            Assert.True(hint_1.Affects((0, 2, 0.5), (0, 4, 0.5)));

            //Line close
            Assert.False(hint_0.Affects((0.1, 3), (0.1, 4)));
            Assert.True(hint_1.Affects((0.1, 3), (0.1, 4)));

            //Line influence edge
            Assert.True(hint_1.Affects((1, 3), (1, 4)));

            //Line very close
            Assert.True(hint_0.Affects((0.000000001, 3), (0.000000001, 4)));
            Assert.True(hint_1.Affects((0.000000001, 3), (0.000000001, 4)));

            //Vertical Line
            Assert.False(hint_0.Affects((3, 5), (3, 5, 1)));
            Assert.False(hint_1.Affects((3, 5), (3, 5, 1)));
            Assert.True(hint_0.Affects((0, 5), (0, 5, 1)));
            Assert.True(hint_1.Affects((0.5, 5), (0.5, 5, 1)));

            //Perpendicular line inside influence
            Assert.False(hint_1.Affects((0, 0), (0.5, 0)));

            //Lays on different segments
            Assert.False(hint_0.Affects((0, 0, 5), (0, 5, 5)));
            Assert.False(hint_1.Affects((0.1, 0, 5), (0.1, 5, 5)));
        }
    }
}
