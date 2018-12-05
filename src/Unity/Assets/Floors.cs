using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hypar.Elements;
using Hypar.Geometry;
using System.Linq;
using Vector3 = Hypar.Geometry.Vector3;

public class Floors : MonoBehaviour {

	public double l1;

	public double l2;

	public double width;

	public double height;

	public double floorToFloor;

	public GameObject[] next;

	public GUIStyle style;
	
	private double totalArea;

	// Use this for initialization
	void Start () {
		StartCoroutine(CreateFloors());
	}

	void OnGUI()
    {
		GUI.Label(new Rect(10, 10, 200, 40), $"Floors: {height/floorToFloor}", style);
        GUI.Label(new Rect(10, 40, 200, 40), $"Total Floor Area: {totalArea}m\xB2", style);
    }
	
	// Update is called once per frame
	void LateUpdate () {
		if(Input.GetKeyUp(KeyCode.Keypad1))
		{
			l1 += 5;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
		else if(Input.GetKeyUp(KeyCode.Keypad2))
		{
			l1 -= 5;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
		else if(Input.GetKeyUp(KeyCode.Keypad4))
		{
			l2 += 5;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
		else if(Input.GetKeyUp(KeyCode.Keypad5))
		{
			l2 -= 5;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
		else if(Input.GetKeyUp(KeyCode.Keypad7))
		{
			height += floorToFloor;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
		else if(Input.GetKeyUp(KeyCode.Keypad8))
		{
			height -= floorToFloor;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
		else if(Input.GetKeyUp(KeyCode.Keypad3))
		{
			floorToFloor += 2;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
		else if(Input.GetKeyUp(KeyCode.Keypad6))
		{
			floorToFloor -= 2;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
		else if(Input.GetKeyUp(KeyCode.Keypad9))
		{
			width += 2;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
		else if(Input.GetKeyUp(KeyCode.KeypadDivide))
		{
			width -= 2;
			StopAllCoroutines();
			StartCoroutine(CreateFloors());
		}
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
		if(floorToFloor == 0)
		{
			floorToFloor = 4;
		}
		if(width == 0)
		{
			width = 5;
		}

		StopAllCoroutines();
		StartCoroutine(CreateFloors());
	}

	void UpdateModel(Model model)
	{
		Debug.Log("Building the floors...");
		StartCoroutine(CreateFloors());
	}

	private IEnumerator CreateFloors()
	{
		var floorModel = new Model();

		var poly = new Polygon(new[]{Vector3.Origin, new Vector3(l1,0,0), new Vector3(l1, width, 0), new Vector3(width, width, 0), new Vector3(width, l2, 0), new Vector3(0,l2,0)});
		var coreHole1 = Polygon.Rectangle(new Vector3(l1,width/2), 3, 4);
		var coreHole2 = Polygon.Rectangle(new Vector3(width/2, l2), 4, 3);
		var profile = new Profile(poly,new[]{coreHole1, coreHole2});
		
		var floorType  = new FloorType("100mm", 0.1);

		totalArea = 0.0;
		for(var i=0.0; i<=height; i+=floorToFloor)
		{
			var floor = new Floor(profile, floorType, i, BuiltInMaterials.Concrete);
			totalArea += floor.Area();
			floorModel.AddElement(floor);
			yield return null;
		}

		foreach(var go in next)
		{
			go.SendMessage("UpdateModel", floorModel);
		}

		yield return floorModel.ToGameObjects(this.gameObject);
	}
}
