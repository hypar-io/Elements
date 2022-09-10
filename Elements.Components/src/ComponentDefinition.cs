
using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;
using System.Text.Json.Serialization;

namespace Elements.Components
{
    /// <summary>
    /// An element representing the definition of a component, with a collection of rules and a set of reference anchors.
    /// </summary>
    public class ComponentDefinition : Element
    {
        /// <summary>
        /// Construct a ComponentDefinition from rules and anchors.
        /// </summary>
        /// <param name="rules">The rules for this component.</param>
        /// <param name="referenceAnchors">The reference anchors for this component definition.</param>
        public ComponentDefinition(IList<IComponentPlacementRule> rules, IList<Vector3> referenceAnchors) : base(Guid.NewGuid(), null)
        {
            Rules = rules;
            ReferenceAnchors = referenceAnchors;
        }
        internal IList<IComponentPlacementRule> Rules { get; set; }

        internal IList<Vector3> ReferenceAnchors { get; set; }
        internal IList<Vector3> AnchorDisplacements { get; set; }
        internal Transform OrientationGuide { get; set; }
        /// <summary>
        /// Create an instance of this component.
        /// </summary>
        /// <param name="anchors">The anchor points used to set the component geometry.</param>
        /// <param name="orientationGuide">An optional transform to help guide the orientation of the elements.</param>
        public ComponentInstance Instantiate(IList<Vector3> anchors, Transform orientationGuide = null)
        {
            var instance = new ComponentInstance();

            // Map the reference anchors from the definition to the supplied boundary anchors,
            // and find the displacement vectors from one to the other 
            AnchorDisplacements = new List<Vector3>();
            for (int i = 0; i < ReferenceAnchors.Count; i++)
            {
                var referenceAnchor = ReferenceAnchors[i];
                var matchingAnchor = anchors[i % anchors.Count]; // TODO: use smarter logic for associating anchors
                AnchorDisplacements.Add(matchingAnchor - referenceAnchor);
            }
            OrientationGuide = orientationGuide;

            if (orientationGuide == null)
            {
                if (ReferenceAnchors.Count < 2 || anchors.Count < 2)
                {
                    OrientationGuide = new Transform();
                }
                else
                {
                    var referenceVector = ReferenceAnchors[1] - ReferenceAnchors[0];
                    var targetVector = anchors[1] - anchors[0];

                    var angle = Angle_2D(referenceVector, targetVector) * 180 / Math.PI;
                    OrientationGuide = new Transform(Vector3.Origin, angle);
                }
            }

            // for each rule, instantiate its instances, and add them
            foreach (var rule in Rules)
            {
                instance.Instances.AddRange(rule.Instantiate(this));
            }

            return instance;
        }

        // TODO: remove and use Vector3.PlaneAngleTo
        private static double Angle_2D(Vector3 A, Vector3 B)
        {
            // reject very small vectors
            if (A.Length() < Vector3.EPSILON || B.Length() < Vector3.EPSILON)
            {
                return double.NaN;
            }

            // project to XY Plane
            Vector3 aProjected = new Vector3(A.X, A.Y, 0).Unitized();
            Vector3 bProjected = new Vector3(B.X, B.Y, 0).Unitized();
            // Cos^-1(a dot b), a dot b clamped to [-1, 1]
            var num = Math.Acos(Math.Max(Math.Min(aProjected.Dot(bProjected), 1.0), -1.0));
            // Round close to 0 to 0
            if (Math.Abs(num) < Vector3.EPSILON)
            {
                return 0.0;
            }
            // Round close to pi to pi
            if (Math.Abs(num - Math.PI) < Vector3.EPSILON)
            {
                return Math.PI;
            }
            // check if should be reflex angle
            Vector3 aCrossB = aProjected.Cross(bProjected).Unitized();
            if (Vector3.ZAxis.Dot(aCrossB) > 0.999)
            {
                return num;
            }
            else
            {
                return Math.PI * 2 - num;
            }
        }
    }
}