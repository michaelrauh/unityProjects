using UnityEngine;
using System.Collections;

public class sphere_motion : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey("a"))
			transform.Translate(new Vector3(0.1f, 0, 0));
		if(Input.GetKey("d"))
			transform.Translate(new Vector3(-0.1f, 0, 0));
		if(Input.GetKey("s"))
			transform.Translate(new Vector3(0, 0, 0.1f));
		if(Input.GetKey("w"))
			transform.Translate(new Vector3(0, 0, -0.1f));
	}
}
