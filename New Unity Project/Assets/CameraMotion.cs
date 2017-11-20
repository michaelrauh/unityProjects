using UnityEngine;
using System.Collections;

public class CameraMotion : MonoBehaviour 
{

    public Transform camera;
	public bool buttonDown = false;

    public float speed = 2.0F;
    
	void Update () 
    {
		if (Input.GetMouseButtonDown(0))
        {
            buttonDown = true;
        } 
        else if(Input.GetMouseButtonUp(0))
        {
            buttonDown = false;
        }
        
        if (buttonDown)
        {
            var x =  speed * Input.GetAxis("Mouse X");
            var y =  speed * Input.GetAxis("Mouse Y");
            
            transform.Rotate(y, x, 0);
        }
	}
}