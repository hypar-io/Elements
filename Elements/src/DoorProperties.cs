using System;
using System.Collections.Generic;
using System.Text;

namespace Elements
{
    internal class DoorProperties
    {
        private readonly double _width;
        private readonly double _height;
        private readonly Material _material;
        private readonly DoorOpeningSide _openingSide;
        private readonly DoorOpeningType _openingType;

        public DoorProperties(Door door)
        {
            _width = door.ClearWidth;
            _height = door.ClearHeight;
            _material = door.Material;
            _openingSide = door.OpeningSide;
            _openingType = door.OpeningType;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DoorProperties doorProps))
            {
                return false;
            }

            if (!_width.ApproximatelyEquals(doorProps._width))
            {
                return false;
            }

            if (!_height.ApproximatelyEquals(doorProps._height))
            {
                return false;
            }

            if (!_material.Id.Equals(doorProps._material.Id))
            {
                return false;
            }

            if (!_openingSide.Equals(doorProps._openingSide))
            {
                return false;
            }

            if (!_openingType.Equals(doorProps._openingType))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + _width.GetHashCode();
            hash = hash * 31 + _height.GetHashCode();
            hash = hash * 31 + _material.Id.GetHashCode();
            hash = hash * 31 + _openingSide.GetHashCode();
            hash = hash * 31 + _openingType.GetHashCode();
            return hash;
        }
    }
}
