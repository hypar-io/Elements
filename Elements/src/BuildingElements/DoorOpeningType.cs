using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.BuildingElements
{
    public enum DoorOpeningType
    {
        [System.Runtime.Serialization.EnumMember(Value = @"Undefined")]
        Undefined,
        [System.Runtime.Serialization.EnumMember(Value = @"Single Swing")]
        SingleSwing,
        [System.Runtime.Serialization.EnumMember(Value = @"Double Swing")]
        DoubleSwing
    }
}
