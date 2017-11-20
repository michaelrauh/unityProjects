using UnityEngine;
using System.Collections;

public class EarthMotion: MonoBehaviour 
{

	public Transform earth;
	
	void Update () 
    {
		earth.transform.Rotate (new Vector3 (0f, 1f, 0f));
		earth.transform.RotateAround (Vector3.zero, Vector3.up, 20 * Time.deltaTime);
	}
}
