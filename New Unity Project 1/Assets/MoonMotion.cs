using UnityEngine;
using System.Collections;

public class MoonMotion : MonoBehaviour 
{

		
	public Transform moon;
	
	void Update () 
    {
		var earth_position = GameObject.Find("Earth").transform.position;
		moon.transform.Rotate (new Vector3 (0f, 1f, .5f));
		moon.transform.RotateAround (earth_position, Vector3.up, 50 * Time.deltaTime);
	}
}