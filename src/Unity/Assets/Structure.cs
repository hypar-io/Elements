using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hypar.Elements;
using Hypar.Geometry;
using System.Linq;
using Vector3 = Hypar.Geometry.Vector3;

public class Structure : MonoBehaviour {

	public double columnSpacing;

	public double columnSetback;

	public GameObject[] next;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnValidate()
	{
		if(columnSpacing == 0)
		{
			columnSpacing = 4;
		}
		if(columnSetback == 0)
		{
			columnSetback = 1.0;
		}
	}

	void UpdateModel(Model model)
	{
		Debug.Log("Building the structure...");
		StartCoroutine(CreateStructure(model));
	}

	private IEnumerator CreateStructure(Model model)
	{
		var structure = new Model();

		var floors = model.ElementsOfType<Floor>().ToList();
		// for(var i=0; i<floors.Count-1; i++)
		// {
			var f = floors[0];
			var f1 = floors[floors.Count - 1];
			var floorToFloor = f1.Elevation-f.Elevation;
			var inset = f.Profile.Perimeter.Offset(-columnSetback);
			foreach(var l in inset.First().Segments())
			{
				var divs = Mathf.Ceil((float)(l.Length/columnSpacing));
				for(var t=0.0; t<=1.0; t+=1.0/divs)
				{
					var pt = l.PointAt(t);
					var column = new Column(new Vector3(pt.X,pt.Y,f.Elevation), f1.Elevation, Polygon.Rectangle(Vector3.Origin, 0.25,0.25), BuiltInMaterials.Concrete);
					structure.AddElement(column);
				}
				yield return null;
			}
		// }
		
		foreach(var go in next)
		{
			go.SendMessage("UpdateModel", structure);
		}

		yield return structure.ToGameObjects(this.gameObject);
	}
}

