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

	public Model model;
	
	public GameObject facade;

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

		for(var i=0.0; i<=height; i+=floorToFloor)
		{
			var floor = new Floor(profile, floorType, i, BuiltInMaterials.Concrete);
			model.AddElement(floor);
			yield return null;

			if(i+floorToFloor > height)
			{
				continue;
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
		this.model = null;
		this.model = model;

		facade.SendMessage("UpdateModel", model);
		yield return model.ToGameObjects(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	}
}

public static class HyparExtensions
{
	public static UnityEngine.Vector3 ToUnityVector3(this Vector3 v)
	{
		return new UnityEngine.Vector3((float)v.X, (float)v.Y, (float)v.Z);
	}

	public static UnityEngine.Vector3[] ToUnityVertices(this IEnumerable<double> arr, Hypar.Geometry.Transform t)
	{	
		var list = arr.ToList();
		var count = arr.Count();
		var verts = new UnityEngine.Vector3[count/3];
		var index = 0;
		for(var i=0; i<count; i+=3)
		{	
			var vt = t.OfPoint(new Vector3(list[i], list[i+1], list[i+2]));
			// var v = new UnityEngine.Vector3((float)list[i], (float)list[i+1], (float)list[i+2]);
			var v = vt.ToUnityVector3();
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
		uMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
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

		go.transform.SetParent(parent.transform,false);
	
		var verts = new List<UnityEngine.Vector3>();
		var tris = new List<int>();
		if(element is ITessellateMesh)
		{
			var tess = (ITessellateMesh)element;
			var mesh = tess.Mesh();
			verts.AddRange(mesh.Vertices.ToUnityVertices(element.Transform));
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
		// var o = element.Transform.Origin.ToUnityVector3();
		var verts = new List<UnityEngine.Vector3>();
		var tris = new List<int>();
		if(element is ITessellateMesh)
		{
			var tess = (ITessellateMesh)element;
			var mesh = tess.Mesh();
			verts.AddRange(mesh.Vertices.ToUnityVertices(element.Transform));
			tris.AddRange(mesh.Indices.Select(idx=>(int)idx));
		}

		goMesh.vertices = verts.ToArray();
		goMesh.triangles = tris.ToArray();
		goMesh.RecalculateNormals();

		return go;
	}

	public static IEnumerator ToGameObjects(this Model model, GameObject parent)
	{
		var materials = new Dictionary<string, UnityEngine.Material>();

		foreach(var m in model.Materials)
		{
			materials.Add(m.Value.Id, m.Value.ToUnityMaterial());
		}

		var existingElementsCount = parent.transform.childCount;
		var newElements = model.Elements.Values.ToList();
		// Debug.Log($"There are {existingElementsCount} existing elements and {newElements.Count} new elements.");

		// If there are more existing objects than new ones.
		if(existingElementsCount > newElements.Count)
		{
			// Debug.Log("Using the existing elements block.");
			for(var i=0; i<existingElementsCount; i++)
			{
				var go = parent.transform.GetChild(i).gameObject;
				if(newElements.Count > i)
				{
					// Update the game object.
					go.Update(newElements[i], materials[newElements[i].Material.Id]);
				}
				else
				{
					// Destroy the game object.
					GameObject.Destroy(go);
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
					var go = parent.transform.GetChild(i).gameObject;
					go.Update(newElements[i], materials[newElements[i].Material.Id]);
				}
				else
				{
					// Debug.Log("Creating new game object...");
					// Create a new game object.
					var e = newElements[i];
					var material = materials[e.Material.Id];
					e.ToGameObject(parent.gameObject, material);
				}
			}
			yield return null;
		}
	}
}
