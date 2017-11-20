using UnityEngine;
using System.Collections;

public class camera_motion : MonoBehaviour {

	bool pressed=false;
	Vector2 pos;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonDown (0)) {
						pressed = true;
				}
		if (Input.GetMouseButtonUp (0))
						pressed = false;

		if (pressed) {
			float h = 1 * Input.GetAxis("Mouse X");
			float v = 1 * Input.GetAxis("Mouse Y");
			transform.RotateAround(new Vector3(0, 0, 0), Vector3.right, v);		
			transform.RotateAround(new Vector3(0, 0, 0), Vector3.up, h);		
		}
	
	}
}
