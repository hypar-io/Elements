using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        [ThreadStatic]
        private static Script<Polygon> _script;
        ScriptOptions _options;
        private readonly string _perimeterScript;
        private readonly List<string> _voidScripts = new List<string>();

        /// <summary>
        /// A collection of vector expressions.
        /// </summary>
        public List<VectorExpression> PerimeterVectorExpressions { get; }

        /// <summary>
        /// A collection of vector expressions.
        /// </summary>
        public List<List<VectorExpression>> VoidVectorExpressions { get; }

        /// <summary>
        /// Create a parametric profile.
        /// </summary>
        /// <param name="perimeterVectorExpressions"></param>
        /// <param name="voidVectorExpressions"></param>
        /// <param name="perimeter">The perimeter of the profile.</param>
        /// <param name="voids">The voids of the profile.</param>
        /// <param name="id">The unique identifier of the profile.</param>
        /// <param name="name">The name of the profile.</param>
        [JsonConstructor]
        public ParametricProfile(List<VectorExpression> perimeterVectorExpressions,
                                    List<List<VectorExpression>> voidVectorExpressions = null,
                                    Polygon @perimeter = null,
                                    IList<Polygon> @voids = null,
                                    Guid @id = default,
                                    string @name = null) : base(perimeter, voids, id, name)
        {
            if (perimeterVectorExpressions == null)
            {
                throw new ArgumentException("The profile could not be created. The perimeter and void expression lists must be defined.");
            }

            if (perimeterVectorExpressions.Count < 3)
            {
                throw new ArgumentException("The profile could not be created. There must be at least 3 perimeter vector expressions.");
            }

            PerimeterVectorExpressions = perimeterVectorExpressions;
            VoidVectorExpressions = voidVectorExpressions;

            _perimeterScript = CompilePolygonScriptFromExpressions(perimeterVectorExpressions);
            if (voidVectorExpressions != null)
            {
                foreach (var voidExpression in voidVectorExpressions)
                {
                    _voidScripts.Add(CompilePolygonScriptFromExpressions(voidExpression));
                }
            }
        }

        private string CompilePolygonScriptFromExpressions(List<VectorExpression> expressions)
        {
            var sb = new StringBuilder();
            sb.Append("new Polygon(true, new[]{");

            foreach (var expr in expressions)
            {
                sb.Append($"new Vector3({expr.X}, {expr.Y}),");
            }
            sb.Append("})");
            return sb.ToString();
        }

        /// <summary>
        /// Create the geometry of the parametric profile.
        /// </summary>
        public async Task SetGeometryAsync()
        {
            if (PerimeterVectorExpressions == null || PerimeterVectorExpressions.Count == 0)
            {
                throw new ArgumentException("The parametric profile could not be created. No translation expressions were provided.");
            }

            Perimeter = await CreatePolygonFromScriptAsync(_perimeterScript);

            Voids = new List<Polygon>();
            if (_voidScripts.Count > 0)
            {
                foreach (var voidScript in _voidScripts)
                {
                    var voidPoly = await CreatePolygonFromScriptAsync(voidScript);
                    Voids.Add(voidPoly);
                }
            }
        }

        /// <summary>
        /// Create a polygon by evaluating all vertex expressions
        /// in one script evaluation.
        /// </summary>
        private async Task<Polygon> CreatePolygonFromScriptAsync(string script)
        {
            if (_options == null)
            {
                _options = ScriptOptions.Default.WithReferences(GetType().Assembly).WithImports("Elements.Geometry");
            }

            // We cache the script wherever possible, but the script will fail
            // to run if the globals type does not match the globals object
            // provided. If this is the case, we need to re-compile the script
            // with the desired type.
            if (_script == null || _script.GlobalsType != GetType())
            {
                _script = CSharpScript.Create<Polygon>(script, _options, GetType());
                _script.Compile();
            }
            var scriptState = await _script.RunAsync(this);
            return scriptState.ReturnValue;
        }

        /// <summary>
        /// Set the properties or public member values of this profile instance
        /// to the values contained in the supplied dictionary.
        /// </summary>
        /// <param name="profileData">A dictionary of property values.</param>
        /// <param name="name">The name of the profile.</param>
        public void SetPropertiesFromProfileData(Dictionary<string, double> profileData, string name)
        {
            var t = GetType();
            foreach (var p in profileData)
            {
                var field = t.GetField(p.Key, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    if (field.FieldType != typeof(double))
                    {
                        continue;
                    }
                    field.SetValue(this, p.Value);
                }
                else
                {
                    var prop = t.GetProperty(p.Key, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        if (prop.PropertyType != typeof(double))
                        {
                            continue;
                        }
                        prop.SetValue(this, p.Value);
                    }
                    // else
                    // {
                    //     throw new Exception($"The profile type, {name}, has no field called {p.Key}.");
                    // }
                }
            }
        }
    }
}