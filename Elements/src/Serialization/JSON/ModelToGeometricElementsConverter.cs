using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// Convert a model to a collection of geometric elements.
    /// </summary>
    public class ModelToGeometricElementsConverter : JsonConverter<Model>
    {
        public override Model Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var resolver = options.ReferenceHandler.CreateResolver() as ElementReferenceResolver;

            // Read materials, profiles, geometric elements
            var elements = new Dictionary<Guid, Element>();
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var elementsElement = root.GetProperty("Elements");
                var transform = JsonSerializer.Deserialize<Transform>(root.GetProperty("Transform"));

                foreach (var element in elementsElement.EnumerateObject())
                {
                    var discriminator = element.Value.GetProperty("discriminator").GetString();

                    // TODO: This try/catch is only here to protect against
                    // situations like null property values when the serializer
                    // expects a value, or validation errors. Unlike json.net, system.text.json doesn't
                    // have null value handling on read. 
                    // try
                    // {
                    var id = Guid.Parse(element.Name);
                    Element e = null;
                    element.Value.TryGetProperty("Name", out var nameProp);

                    string name;
                    {
                        name = nameProp.GetString();
                    }

                    switch (discriminator)
                    {
                        // TODO: Big assumption here - that things are in the right order.
                        case "Elements.Material":
                            e = JsonSerializer.Deserialize<Material>(element.Value);
                            break;
                        case "Elements.Geometry.Profile":
                            e = JsonSerializer.Deserialize<Profile>(element.Value);
                            break;
                        case "Elements.ElementInstance":
                            var baseDefinition = (GeometricElement)resolver.ResolveReference(element.Value.GetProperty("BaseDefinition").GetString());
                            var elementTransform = JsonSerializer.Deserialize<Transform>(element.Value.GetProperty("Transform"));
                            e = new ElementInstance(baseDefinition, elementTransform, name, id);
                            break;
                        default:

                            if (element.Value.TryGetProperty("Perimeter", out _) && element.Value.TryGetProperty("Voids", out _))
                            {
                                // TODO: We're handling profile-like things in this way.
                                e = JsonSerializer.Deserialize<Profile>(element.Value);
                                break;
                            }

                            // Qualify element as a geometric element by seeing 
                            // whether it has a representation.
                            if (element.Value.TryGetProperty("Representation", out var repProperty))
                            {
                                if (discriminator == "Elements.ModelCurve")
                                {
                                    // TODO: Handle model curves.
                                    continue;
                                }
                                else if (discriminator == "Elements.GridLine")
                                {
                                    // TODO: Handle grid lines.
                                    continue;
                                }

                                var solidOps = new List<SolidOperation>();
                                foreach (var solidOp in repProperty.GetProperty("SolidOperations").EnumerateArray())
                                {
                                    SolidOperation op = null;
                                    var isVoid = false;
                                    if (solidOp.TryGetProperty("IsVoid", out var isVoidElement))
                                    {
                                        isVoid = isVoidElement.GetBoolean();
                                    }

                                    switch (solidOp.GetProperty("discriminator").GetString())
                                    {
                                        case "Elements.Geometry.Solids.Extrude":
                                            var profile = (Profile)resolver.ResolveReference(solidOp.GetProperty("Profile").GetString());
                                            var height = solidOp.GetProperty("Height").GetDouble();
                                            var direction = JsonSerializer.Deserialize<Vector3>(solidOp.GetProperty("Direction"));
                                            op = new Extrude(profile, height, direction, isVoid);
                                            break;
                                        case "Elements.Geometry.Solids.Sweep":
                                            profile = (Profile)resolver.ResolveReference(solidOp.GetProperty("Profile").GetString());
                                            var curve = DeserializeCurve(solidOp.GetProperty("Curve"));
                                            var startSetback = solidOp.GetProperty("StartSetback").GetDouble();
                                            var endSetback = solidOp.GetProperty("EndSetback").GetDouble();
                                            var profileRotation = 0.0;
                                            if (solidOp.TryGetProperty("ProfileRotation", out var rotation))
                                            {
                                                profileRotation = rotation.GetDouble();
                                            }
                                            op = new Sweep(profile, curve, startSetback, endSetback, profileRotation, isVoid);
                                            break;
                                        case "Elements.Geometry.Solids.Lamina":
                                            var perimeter = JsonSerializer.Deserialize<Polygon>(solidOp.GetProperty("Perimeter"));
                                            op = new Lamina(perimeter, isVoid);
                                            break;
                                    }
                                    solidOps.Add(op);
                                }
                                var rep = new Representation(solidOps);
                                elementTransform = JsonSerializer.Deserialize<Transform>(element.Value.GetProperty("Transform"));
                                var material = (Material)resolver.ResolveReference(element.Value.GetProperty("Material").GetString());
                                var elementId = element.Value.GetProperty("Id").GetGuid();
                                var isElementDefinition = element.Value.GetProperty("IsElementDefinition").GetBoolean();
                                e = new GeometricElement(elementTransform, material, rep, isElementDefinition, elementId, name);
                            }
                            break;
                    }
                    if (e != null)
                    {
                        elements.Add(id, e);
                        resolver.AddReference(id.ToString(), e);
                    }
                    // }
                    // catch (Exception ex)
                    // {
                    //     Console.WriteLine(ex.Message);
                    //     continue;
                    // }
                }

                var model = new Model(transform, elements);
                return model;
            }
        }

        private Curve DeserializeCurve(JsonElement jsonCurve)
        {
            var discriminator = jsonCurve.GetProperty("discriminator").GetString();
            switch (discriminator)
            {
                case "Elements.Geometry.Line":
                    return JsonSerializer.Deserialize<Line>(jsonCurve);
                case "Elements.Geometry.Polygon":
                    return JsonSerializer.Deserialize<Polygon>(jsonCurve);
                case "Elements.Geometry.Polyline":
                    return JsonSerializer.Deserialize<Polyline>(jsonCurve);
                case "Elements.Geometry.Arc":
                    return JsonSerializer.Deserialize<Arc>(jsonCurve);
                default:
                    throw new JsonException($"The curve type, {discriminator}, could not be deserialized.");
            }
        }

        public override void Write(Utf8JsonWriter writer, Model value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}