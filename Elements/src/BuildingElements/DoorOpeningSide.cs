using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.BuildingElements
{
    public enum DoorOpeningSide
    {
        [System.Runtime.Serialization.EnumMember(Value = @"Undefined")]
        Undefined,
        [System.Runtime.Serialization.EnumMember(Value = @"Left Hand")]
        LeftHand,
        [System.Runtime.Serialization.EnumMember(Value = @"Right Hand")]
        RightHand,
        [System.Runtime.Serialization.EnumMember(Value = @"Double Door")]
        DoubleDoor
    }
}
