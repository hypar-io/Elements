using System;
using System.Collections.Generic;
using Elements.Flow;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    public abstract partial class Fitting
    {
        [Obsolete("Use GetPorts")]
        public Port[] GetConnectors()
        {
            return GetPorts();
        }

        public virtual string GetRepresentationHash()
        {
            return this.GetHashCode().ToString();
        }

        abstract public Port[] GetPorts();

        public abstract Transform GetRotatedTransform();

        public override bool PropagateAdditionalTransform(Transform transform, TransformDirection transformDirection)
        {
            AdditionalTransform.Concatenate(transform);
            return true;
        }

        public override Transform GetPropagatedTransform(TransformDirection transformDirection)
        {
            return AdditionalTransform;
        }

        public override void ClearAdditionalTransform()
        {
            var inverted = new Transform(AdditionalTransform);
            inverted.Invert();
            AdditionalTransform.Concatenate(inverted);
        }

        public override void ApplyAdditionalTransform()
        {
            Transform.Concatenate(AdditionalTransform);
            var connectors = GetPorts();
            foreach (var connector in connectors)
            {
                connector.Position = AdditionalTransform.OfPoint(connector.Position);
            }

            ClearAdditionalTransform();
        }

        public void AssignReferenceBasedOnSection(Section section)
        {
            if (section != null)
            {
                var sectionLocator = new FittingLocator(section);
                ComponentLocator.MatchNetworkSection(sectionLocator);
                if (this is Assembly assembly)
                {
                    assembly.AssignSectionReferenceInternalToAssembly(sectionLocator);
                }
            }
        }

        protected List<SolidOperation> GetExtensions()
        {
            List<SolidOperation> extrudes = new List<SolidOperation>();

            foreach (var port in GetPorts())
            {
                if (port.Dimensions == null || port.Dimensions.Extension.ApproximatelyEquals(0))
                {
                    continue;
                }

                var extensionDiameter = port.Dimensions.BodyDiameter;
                if (extensionDiameter.ApproximatelyEquals(0) || extensionDiameter.ApproximatelyEquals(port.Diameter))
                {
                    extensionDiameter = port.Diameter * 1.2;
                }

                var portTransform = new Transform(port.Position, port.Direction);
                portTransform = portTransform.Concatenated(Transform.Inverted());
                double bigDiameter = extensionDiameter;
                double smallDiameter = port.Diameter;
                if (bigDiameter < smallDiameter)
                {
                    (bigDiameter, smallDiameter) = (smallDiameter, bigDiameter);
                }

                var bigCircle = new Circle(portTransform, bigDiameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
                var smallCircle = new Circle(portTransform, smallDiameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
                Profile profile = new Profile(bigCircle, smallCircle);
                var extrude = new Extrude(profile, port.Dimensions.Extension, portTransform.ZAxis.Unitized());
                extrudes.Add(extrude);
            }

            return extrudes;
        }
    }
}