using Elements;
using Elements.Geometry.Profiles;
using Elements.Geometry;
using Elements.Serialization.JSON;

var x = 0.0;
var z = 0.0;
var factory = new HSSPipeProfileFactory();
var hssProfiles = factory.AllProfiles().ToList();
var model = new Model();
foreach (var profile in hssProfiles)
{
    var color = new Color((float)(x / 20.0), (float)(z / hssProfiles.Count), 0.0f, 1.0f);
    var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
    var beam = new Beam(line, profile);
    model.AddElement(beam);
    x += 2.0;
    if (x > 20.0)
    {
        z += 2.0;
        x = 0.0;
    }
}

var json = model.ToJson();
var newModel = Model.FromJson(json, out List<string> errors);
foreach (var e in errors)
{
    Console.WriteLine(e);
}