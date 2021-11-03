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
        private readonly List<MetadataReference> _refs;

        /// <summary>
        /// The data which defines the profile.
        /// </summary>
        public ParametericProfileData Data { get; set; }

        /// <summary>
        /// Create a parametric profile.
        /// </summary>
        /// <param name="data">The profile </param>
        /// <param name="id">The unique identifier of the profile.</param>
        /// <param name="name">The name of the profile.</param>
        [JsonConstructor]
        public ParametricProfile(ParametericProfileData data, Guid @id = default, string @name = null) : base(null, null, id, name)
        {
            if (data.PerimeterVectorExpressions == null || data.PerimeterVectorExpressions.Count == 0)
            {
                throw new ArgumentException("The parametric profile could not be created. No translation expressions were provided.");
            }

            Data = data;

            // These references are required to get dynamic to work.
            _refs = new List<MetadataReference>{
                                    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).GetTypeInfo().Assembly.Location),
                                    MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location)};
            Perimeter = CreatePolygonFromExpressions(data.PerimeterVectorExpressions, data.PropertyValues, _refs).Result;

            if (data.VoidVectorExpressions != null)
            {
                foreach (var voidExpr in data.VoidVectorExpressions)
                {
                    var voidPoly = CreatePolygonFromExpressions(voidExpr, data.PropertyValues, _refs).Result;
                    Voids.Add(voidPoly);
                }
            }
        }

        /// <summary>
        /// Create the profile.
        /// </summary>
        public static async Task<Polygon> CreatePolygonFromExpressions(List<VectorExpression> expressions,
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