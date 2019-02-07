using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elements;
using System.Linq;
using Elements.Geometry;

public class Facade : MonoBehaviour {

	public GameObject[] next;

	public GUIStyle style;

	private int gridCount;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI()
    {
		GUI.Label(new Rect(10, 70, 200, 40), $"Facade Units: {gridCount}", style);
    }

	void UpdateModel(Model model)
	{
		Debug.Log("Building the facade...");
		StartCoroutine(CreateFacade(model));
	}

	private IEnumerator CreateFacade(Model model)
	{
		var facade = new Model();
		var floors = model.ElementsOfType<Floor>().ToList();

		var f1 = floors[0];
		var f2 = floors[floors.Count - 1];
		var height = f2.Elevation-f1.Elevation;
		var f2f = floors[1].Elevation - f1.Elevation;
		var et = new Elements.Geometry.Transform(new Elements.Geometry.Vector3(0,0,f1.Elevation));
		var perim = f1.Profile.Perimeter.Offset(0.001)[0];
		var mass = new Mass(new Profile(perim), height + f2f/3);
		gridCount = 0;
		foreach(var f in perim.Segments())
		{
			var top = new Line(new Elements.Geometry.Vector3(f.Start.X,f.Start.Y,f.Start.Z + height),
													new Elements.Geometry.Vector3(f.End.X,f.End.Y,f.End.Z + height));
			var g = new Elements.Grid(f, top, 4, f2f/3);
			var cells = g.Cells();
			gridCount += cells.Length;
			for(var i=0;i<cells.GetLength(0); i++)
			{
				var count = -1;
				for(var j=0; j<cells.GetLength(1); j++)
				{
					var c = cells[i,j];
					var m = BuiltInMaterials.Glass;
					count++;
					if(count == 0 || count == 2)
					{
						m = BuiltInMaterials.Default;
					}
					
					if(count == 2)
					{
						count = -1;
					}
					
					var p1 = new Panel(c.Reverse().ToArray().Shrink(0.025), m);
					facade.AddElement(p1);
				}
			}
			yield return null;
		}

		foreach(var go in next)
		{
			go.SendMessage("UpdateModel", facade);
		}

		yield return facade.ToGameObjects(this.gameObject);
	}
}
