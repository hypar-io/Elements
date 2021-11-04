using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A profile whose vertex locations are defined by a parametric expression.
    /// </summary>
    public class ParametricProfile : Profile
    {
        private static List<MetadataReference> _refs;

        /// <summary>
        /// The data which defines the profile.
        /// </summary>
        public ParametericProfileData Data { get; set; }

        /// <summary>
        /// Create a parametric profile.
        /// </summary>
        /// <param name="data">The data used to generate the profile.</param>
        /// <param name="perimeter">The perimeter of the profile.</param>
        /// <param name="voids">The voids of the profile.</param>
        /// <param name="id">The unique identifier of the profile.</param>
        /// <param name="name">The name of the profile.</param>
        [JsonConstructor]
        private ParametricProfile(ParametericProfileData data,
                                  Polygon @perimeter,
                                  IList<Polygon> @voids,
                                  Guid @id = default,
                                  string @name = null) : base(perimeter, voids, id, name)
        {
            Data = data;
        }

        /// <summary>
        /// Create a parametric profile.
        /// </summary>
        /// <param name="data">The data used to generate the profile.</param>
        /// <param name="id">The unique identifier of the profile.</param>
        /// <param name="name">The name of the profile.</param>
        public static async Task<ParametricProfile> CreateAsync(ParametericProfileData data, Guid id = default, string name = null)
        {
            if (_refs == null)
            {
                // These references are required to get dynamic to work.
                _refs = new List<MetadataReference>{
                                    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).GetTypeInfo().Assembly.Location),
                                    MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location)};
            }

            if (data.PerimeterVectorExpressions == null || data.PerimeterVectorExpressions.Count == 0)
            {
                throw new ArgumentException("The parametric profile could not be created. No translation expressions were provided.");
            }

            var perimeter = await CreatePolygonFromExpressionsAsync(data.PerimeterVectorExpressions, data.PropertyValues, _refs);

            var voids = new List<Polygon>();
            if (data.VoidVectorExpressions != null)
            {
                foreach (var voidExpr in data.VoidVectorExpressions)
                {
                    var voidPoly = await CreatePolygonFromExpressionsAsync(voidExpr, data.PropertyValues, _refs);
                    voids.Add(voidPoly);
                }
            }

            return new ParametricProfile(data, perimeter, voids, id, name);
        }

        /// <summary>
        /// Create the profile.
        /// </summary>
        private static async Task<Polygon> CreatePolygonFromExpressionsAsync(List<VectorExpression> expressions,
                                                                       Dictionary<string, double> propertyValues,
                                                                       List<MetadataReference> refs)
        {
            var vertices = new List<Vector3>();

            // https://github.com/dotnet/roslyn/issues/3194
            // We use an exando object so we can load it up with
            // the properties at runtime.
            dynamic expando = new ExpandoObject();
            foreach (var p in propertyValues)
            {
                ((IDictionary<string, object>)expando).Add(p.Key, p.Value);
            }
            var g = new ProfileScriptGlobals() { data = expando };

            var options = ScriptOptions.Default.AddReferences(refs);

            foreach (var expr in expressions)
            {
                var script = CSharpScript.Create<double>(expr.X,
                                                         options: options,
                                                         globalsType: typeof(ProfileScriptGlobals));
                var xResult = (await script.RunAsync(g)).ReturnValue;

                script = CSharpScript.Create<double>(expr.Y,
                                                     options: options,
                                                     globalsType: typeof(ProfileScriptGlobals));
                var yResult = (await script.RunAsync(g)).ReturnValue;

                var newPosition = new Vector3(xResult, yResult, 0);
                vertices.Add(newPosition);
            }

            return new Polygon(vertices);
        }

        /// <summary>
        /// The globals type for the profile script.
        /// </summary>
        public class ProfileScriptGlobals
        {
            /// <summary>
            /// The 'data' property available to the script.
            /// </summary>
            public dynamic data;
        }
    }
}