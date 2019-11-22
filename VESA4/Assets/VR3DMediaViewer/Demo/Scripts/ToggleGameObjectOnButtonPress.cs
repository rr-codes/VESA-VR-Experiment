using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGameObjectOnButtonPress : MonoBehaviour
{
    public GameObject targetGameObject;
    public KeyCode button;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (targetGameObject != null && button != KeyCode.None)
        {
            if (Input.GetKeyDown(button))
                targetGameObject.SetActive(!targetGameObject.activeSelf);
        }
	}
}
