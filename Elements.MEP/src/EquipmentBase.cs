using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Fittings;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    public partial class EquipmentBase : GeometricElement
    {
        /// <summary>
        /// Create a copy of the equipment including AdditionalProperties.
        /// </summary>
        /// <returns></returns>
        public EquipmentBase(EquipmentBase other)
        {
            Name = other.Name;
            Id = Guid.NewGuid();
            Material = other.Material;
            AdditionalProperties = new Dictionary<string, object>(other.AdditionalProperties);
            Ports = new List<Port>();
            foreach (var port in other.Ports)
            {
                AddPort(port);
            }
        }

        /// <summary>
        /// Add the port, always making a clone
        /// </summary>
        /// <param name="port"></param>
        public void AddPort(Port port)
        {
            Ports.Add(port.Clone());
        }

        /// <summary>
        /// Create a clone of the equipment including AdditionalProperties.
        /// </summary>
        /// <returns></returns>
        public EquipmentBase Clone()
        {
            var newEquip = new EquipmentBase(this);
            return newEquip;
        }
    }
}