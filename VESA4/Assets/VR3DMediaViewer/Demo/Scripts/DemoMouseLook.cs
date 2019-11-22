/*
 * DemoMouseLook.cs
 * 
 * This is just a simple mouse look script for the Stereo 369 Panorama demo.
 * 
 * You shouldn't use this for anything else. It has hardcoded values for the
 * demos purpose, and just isn't needed for VR. It's just needed to simulate
 * head movement to convey things on a 2D screen.
*/

using UnityEngine;
using System.Collections;

public class DemoMouseLook : MonoBehaviour
{
    private float xRotation = 0;
    private float yRotation = 0;
    private Quaternion defaultRotation;

	// Use this for initialization
	void Start () 
    {
        defaultRotation = transform.localRotation;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetMouseButton(0))
        {
            // We add the new rotation amount to the old total, with a increase to make it move faster.
            xRotation += Input.GetAxis("Mouse X") * 5.0f;
            yRotation += Input.GetAxis("Mouse Y") * 5.0f;

            // We wrap our values around in range of 360 degrees.
            if (xRotation > 360.0f) xRotation -= 360.0f;
            if (xRotation < -360.0f) xRotation += 360.0f;
            if (yRotation > 360.0f) yRotation -= 360.0f;
            if (yRotation < -360.0f) yRotation += 360.0f;

            // Make sure the rotation values don't exceed some reasonable limits.
            xRotation = Mathf.Clamp(xRotation, -360.0f, 360.0f);
            yRotation = Mathf.Clamp(yRotation, -90.0f, 90.0f);

            // Translate our floats to Quaternions.
            Quaternion xRot = Quaternion.AngleAxis(xRotation, Vector3.up);
            Quaternion yRot = Quaternion.AngleAxis(yRotation, -Vector3.right);

            // Set the new rotation value.
            transform.localRotation = defaultRotation * xRot * yRot;
        }	
	}
}
