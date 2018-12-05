using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hypar.Elements;
using Hypar.Geometry;
using System.Linq;
using Vector3 = Hypar.Geometry.Vector3;

public class Cores : MonoBehaviour {

	public GameObject[] next;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void UpdateModel(Model model)
	{
		Debug.Log("Building the structure...");
		StartCoroutine(CreateCores(model));
	}

	private IEnumerator CreateCores(Model model)
	{
		var cores = new Model();

		var floors = model.ElementsOfType<Floor>().ToList();
		
		var f = floors[0];
		var f1 = floors[floors.Count - 1];
		var height = f1.Elevation - f.Elevation + 2;

		var corePerim1 = f.ProfileTransformed.Voids[0];
		var corePerim2 = f.ProfileTransformed.Voids[1];
		var wallType = new WallType("200mm", 0.2);
		foreach(var l in corePerim1.Segments())
		{
			var w = new Wall(l, wallType, height);
			cores.AddElement(w);
		}
		foreach(var l in corePerim2.Segments())
		{
			var w = new Wall(l, wallType, height);
			cores.AddElement(w);
		}
		
		if(next != null)
		{
			foreach(var go in next)
			{
				go.SendMessage("UpdateModel", cores);
			}
		}

		yield return cores.ToGameObjects(this.gameObject);
	}
}


