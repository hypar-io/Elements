using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hypar.Elements;
using Hypar.Geometry;
using Vector3 = Hypar.Geometry.Vector3;
using System.Linq;
using UnityEngine.UI;

public class CreateHyparElements : MonoBehaviour {

	public double height;

	public double l1;

	public double l2;

	public double width;

	public double floorToFloor;

	public double columnSpacing;

	public double columnSetback;

	// Use this for initialization
	void Start () {
		StartCoroutine(CreateLModel());
	}

	void OnValidate()
	{
		if(l1 == 0 || l1 > 100)
		{
			l1 = 20;
		}
		if(l2 == 0)
		{
			l2 = 10;
		}
		if(height == 0)
		{
			height = 30;
		}
		if(width == 0)
		{
			width = 5;
		}
		if(floorToFloor == 0)
		{
			floorToFloor = 4;
		}
		if(columnSpacing == 0)
		{
			columnSpacing = 4;
		}
		if(columnSetback == 0)
		{
			columnSetback = 1.0;
		}
		StopAllCoroutines();
		StartCoroutine(CreateLModel());
	}

	private IEnumerator CreateLModel()
	{
		var poly = new Polygon(new[]{Vector3.Origin, new Vector3(l1,0,0), new Vector3(l1, width, 0), new Vector3(width, width, 0), new Vector3(width, l2, 0), new Vector3(0,l2,0)});
		var hole = Polygon.Rectangle(new Vector3(2,2), new Vector3(4,4));
		var profile = new Profile(poly,hole);
		
		var floorType  = new FloorType("100mm", 0.1);

		var inset = poly.Offset(-columnSetback);

		var model = new Model();
		// model.AddElement(mass);
		
		for(var i=0.0; i<=height; i+=floorToFloor)
		{
			var floor = new Floor(profile, floorType, i, BuiltInMaterials.Concrete);
			model.AddElement(floor);
			yield return null;

			if(i+floorToFloor > height)
			{
				continue;
			}

			var mass = new Mass(new Profile(poly.Offset(0.001)[0]), i, floorToFloor);
			foreach(var f in mass.Faces())
			{
				var g = new Hypar.Elements.Grid(f, 10, 3);
				for(var c=0;c<g.ColumnCount; c++)
				{	
					var colCells =g.CellsInColumn(c);
					var c1 = colCells[0];
					var c2 = colCells[1];
					var c3 = colCells[2];
					var p1 = new Panel(c1.Reverse().ToList(), BuiltInMaterials.Default);
					var p2 = new Panel(c2.Reverse().ToList(), BuiltInMaterials.Glass);
					var p3 = new Panel(c3.Reverse().ToList(), BuiltInMaterials.Default);
					model.AddElements(new[]{p1,p2,p3});
				}
			}

			foreach(var l in inset.First().Segments())
			{
				var divs = Mathf.Ceil((float)(l.Length/columnSpacing));
				for(var t=0.0; t<=1.0; t+=1.0/divs)
				{
					var pt = l.PointAt(t);
					var column = new Column(new Vector3(pt.X,pt.Y,i), floorToFloor, Polygon.Rectangle(Vector3.Origin, 0.25,0.25), BuiltInMaterials.Concrete);
					model.AddElement(column);
				}
				yield return null;
			}
		}
		yield return CreateGameObjectsFromModel(model);
	}

	private IEnumerator CreateDomeModel(double radius, int divisions)
	{
		var model = new Model(); 

		var dome = CreateDome(radius,divisions);

		for(var i=0; i<dome.Count; i++)
		{	
			// Draw latitudinal beams.
			List<Vector3> a,b;

			if(i == dome.Count - 1)
			{
				a = dome[i];
				b = dome[0];
			}
			else
			{
				a = dome[i];
				b = dome[i+1];
			}

			for(var j=2; j<a.Count; j++)
			{
				var line = new Line(a[j], b[j]);
				var beam = new Beam(line, WideFlangeProfileServer.Instance.GetProfileByName("W27x178"), BuiltInMaterials.Steel, Vector3.ZAxis);
				model.AddElement(beam);
			}

			// Draw longitudinal beams.
			for(var k=0; k<a.Count-1; k++)
			{
				var line = new Line(a[k], a[k+1]);
				var beam = new Beam(line, WideFlangeProfileServer.Instance.GetProfileByName("W27x178"), BuiltInMaterials.Steel, Vector3.ZAxis);
				model.AddElement(beam);
			}

			// Draw panels
			for(var k=2; k<a.Count-1; k++)
			{
				var panel = new Panel(new[]{a[k+1], b[k+1], b[k], a[k]}, BuiltInMaterials.Glass);
				model.AddElement(panel);
			}
		}

		CreateGameObjectsFromModel(model);

		yield return null;
	}
	
	private IEnumerator CreateGameObjectsFromModel(Model model)
	{
		var materials = new Dictionary<string, UnityEngine.Material>();

		foreach(var m in model.Materials)
		{
			materials.Add(m.Value.Id, m.Value.ToUnityMaterial());
		}

		var existingElementsCount = this.transform.childCount;
		var newElements = model.Elements.Values.ToList();
		// Debug.Log($"There are {existingElementsCount} existing elements and {newElements.Count} new elements.");

		// If there are more existing objects than new ones.
		if(existingElementsCount > newElements.Count)
		{
			// Debug.Log("Using the existing elements block.");
			for(var i=0; i<existingElementsCount; i++)
			{
				var go = this.transform.GetChild(i).gameObject;
				if(newElements.Count > i)
				{
					// Update the game object.
					go.Update(newElements[i], materials[newElements[i].Material.Id]);
				}
				else
				{
					// Destroy the game object.
					Destroy(go);
				}
			}
			yield return null;
		}
		// if there are more new objects than old ones.
		else if(newElements.Count >= existingElementsCount)
		{
			// Debug.Log("Using the new elements block.");
			for(var i=0; i< newElements.Count; i++)
			{
				if(existingElementsCount > i)
				{
					// Debug.Log("Updating game object...");
					var go = this.transform.GetChild(i).gameObject;
					go.Update(newElements[i], materials[newElements[i].Material.Id]);
				}
				else
				{
					// Debug.Log("Creating new game object...");
					// Create a new game object.
					var e = newElements[i];
					var material = materials[e.Material.Id];
					e.ToGameObject(this.gameObject, material);
				}
			}
			yield return null;
		}
	}

	// Update is called once per frame
	void Update () {
	}

	//Invoked when a submit button is clicked.
    public void SubmitSliderSetting()
    {
		StopAllCoroutines();
		StartCoroutine(CreateLModel());
    }

	private List<List<Vector3>> CreateDome(double r, int div)
	{
		var pts = new List<List<Vector3>>();
		var PI2 = Mathf.PI * 2;
		var udiv = div;
		var vdiv = div;
		for(var s=0.0f; s<PI2; s += PI2/udiv)
		{
			var slice = new List<Vector3>();
			var halfPi = Mathf.PI/2;
			for(var t=0.0f; t<=halfPi; t+=halfPi/vdiv)
			{
				var v = new Vector3(r * Mathf.Cos(s) * Mathf.Sin(t), r * Mathf.Sin(s) * Mathf.Sin(t), r * Mathf.Cos(t));
				slice.Add(v);
			}
			pts.Add(slice);
		}
		return pts;
	}
}

public static class HyparExtensions
{
	public static UnityEngine.Vector3 ToUnityVector3(this Vector3 v)
	{
		return new UnityEngine.Vector3((float)v.X, (float)v.Y, (float)v.Z);
	}

	public static UnityEngine.Vector3[] ToUnityVertices(this IEnumerable<double> arr)
	{	
		var list = arr.ToList();
		var count = arr.Count();
		var verts = new UnityEngine.Vector3[count/3];
		var index = 0;
		for(var i=0; i<count; i+=3)
		{
			var v = new UnityEngine.Vector3((float)list[i], (float)list[i+1], (float)list[i+2]);
			verts[index] = v;
			index ++;
		}
		return verts;
	}

	private static UnityEngine.Mesh CreateGameObject(string name, UnityEngine.Material material, UnityEngine.Transform transform)
	{
		var go = new GameObject(name);
		var goMeshFilter = go.AddComponent<MeshFilter>();
		var goRenderer = go.AddComponent<MeshRenderer>();
		goRenderer.material = material;
		var goMesh = new UnityEngine.Mesh();
		goMeshFilter.mesh = goMesh;
		go.transform.SetParent(transform, false);
		return goMesh;
	}

	public static UnityEngine.Color ToUnityColor(this Hypar.Geometry.Color color)
	{
		return new UnityEngine.Color(color.Red, color.Green, color.Blue, color.Alpha);
	}

	public static UnityEngine.Material ToUnityMaterial(this Hypar.Elements.Material material)
	{
		var uMaterial = new UnityEngine.Material(Shader.Find("Standard"));
		uMaterial.name = material.Name;
		uMaterial.color = material.Color.ToUnityColor();
		if(material.Color.Alpha < 1.0f)
		{
			uMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			uMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			uMaterial.SetInt("_ZWrite", 0);
			uMaterial.DisableKeyword("_ALPHATEST_ON");
			uMaterial.DisableKeyword("_ALPHABLEND_ON");
			uMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
			uMaterial.SetFloat("_Glossiness", material.GlossinessFactor);
			// uMaterial.SetFloat("_Metallic", material.SpecularFactor);
			uMaterial.renderQueue = 3000;
		}
		return uMaterial;
	}

	public static GameObject ToGameObject(this Element element, GameObject parent, UnityEngine.Material material)
	{
		var go = new GameObject(element.Id);
		var goMeshFilter = go.AddComponent<MeshFilter>();
		var goRenderer = go.AddComponent<MeshRenderer>();
		goRenderer.material = material;
		var goMesh = new UnityEngine.Mesh();
		goMeshFilter.mesh = goMesh;
		go.transform.SetParent(parent.transform, false);
		var o = element.Transform.Origin.ToUnityVector3();
		go.transform.position = new UnityEngine.Vector3(o.x, o.z, o.y);
		var verts = new List<UnityEngine.Vector3>();
		var tris = new List<int>();
		if(element is ITessellateMesh)
		{
			var tess = (ITessellateMesh)element;
			var mesh = tess.Mesh();
			verts.AddRange(mesh.Vertices.ToUnityVertices());
			tris.AddRange(mesh.Indices.Select(idx=>(int)idx));
		}

		goMesh.vertices = verts.ToArray();
		goMesh.triangles = tris.ToArray();
		goMesh.RecalculateNormals();

		return go;
	}

	public static GameObject Update(this GameObject go, Element element, UnityEngine.Material material)
	{
		var goMeshFilter = go.GetComponent<MeshFilter>();
		var goRenderer = go.GetComponent<MeshRenderer>();
		goRenderer.material = material;
		var goMesh = goMeshFilter.mesh;
		goMesh.Clear();
		var o = element.Transform.Origin.ToUnityVector3();
		go.transform.position = new UnityEngine.Vector3(o.x, o.z, o.y);
		var verts = new List<UnityEngine.Vector3>();
		var tris = new List<int>();
		if(element is ITessellateMesh)
		{
			var tess = (ITessellateMesh)element;
			var mesh = tess.Mesh();
			verts.AddRange(mesh.Vertices.ToUnityVertices());
			tris.AddRange(mesh.Indices.Select(idx=>(int)idx));
		}

		goMesh.vertices = verts.ToArray();
		goMesh.triangles = tris.ToArray();
		goMesh.RecalculateNormals();

		return go;
	}
}
