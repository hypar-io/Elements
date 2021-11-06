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
            PerimeterVectorExpressions = perimeterVectorExpressions;
            VoidVectorExpressions = voidVectorExpressions;
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

            Perimeter = await CreatePolygonFromExpressionsAsync(PerimeterVectorExpressions);

            Voids = new List<Polygon>();
            if (VoidVectorExpressions != null)
            {
                foreach (var voidExpr in VoidVectorExpressions)
                {
                    var voidPoly = await CreatePolygonFromExpressionsAsync(voidExpr);
                    Voids.Add(voidPoly);
                }
            }
        }

        /// <summary>
        /// Create a polygon by evaluating all vertex expressions
        /// in one script evaluation.
        /// </summary>
        private async Task<Polygon> CreatePolygonFromExpressionsAsync(List<VectorExpression> expressions)
        {
            var sb = new StringBuilder();
            sb.Append("new Polygon(new[]{");

            foreach (var expr in expressions)
            {
                sb.Append($"new Vector3({expr.X}, {expr.Y}),");
            }
            sb.Append("})");
            var script = sb.ToString();
            return await CSharpScript.EvaluateAsync<Polygon>(script,
                                                             ScriptOptions.Default.WithReferences(GetType().Assembly).WithImports("Elements.Geometry"),
                                                             globals: this);
        }

        /// <summary>
        /// Set the properties or public member values of this profile instance
        /// to the values contained in the supplied dictionary.
        /// </summary>
        /// <param name="profileData">A dictionary of property values.</param>
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