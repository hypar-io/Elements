using System;

namespace Hypar.Elements
{
    /// <summary>
    /// An Opening in a Wall.
    /// </summary>
    public class Opening
    {
        /// <summary>
        /// The distance along the wall to the lower left corner of the Opening.
        /// </summary>
        public double DistanceAlongWall{get;}

        /// <summary>
        /// The base height of the Opening.
        /// </summary>
        public double BaseHeight{get;}

        /// <summary>
        /// The height of the Opening.
        /// </summary>
        public double Height{get;}

        /// <summary>
        /// The width of the Opening.
        /// </summary>
        public double Width{get;}

        /// <summary>
        /// Construct an Opening.
        /// </summary>
        /// <param name="distanceAlongWall">The distance along the Wall.</param>
        /// <param name="baseHeight">The base height of the Opening.</param>
        /// <param name="height">The height of the opening.</param>
        /// <param name="width">The width of the opening.</param>
        public Opening(double distanceAlongWall, double baseHeight, double height, double width)
        {
            if(distanceAlongWall < 0.0)
            {
                throw new ArgumentOutOfRangeException("The distance along the curve must be greater than 0.0.");
            }

            if(baseHeight < 0.0)
            {
                throw new ArgumentOutOfRangeException("The base height must be greater than 0.0.");
            }

            if(height <= 0.0)
            {
                throw new ArgumentOutOfRangeException("An opening's height must be greater than 0.0.");
            }

            if(width <= 0.0)
            {
                throw new ArgumentOutOfRangeException("An opening's width must be greater than 0.0.");
            }
            this.DistanceAlongWall = distanceAlongWall;
            this.BaseHeight = baseHeight;
            this.Height = height;
            this.Width = width;
        }
    }
}