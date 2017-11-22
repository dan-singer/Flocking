using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages camera toggling for the flocking demo
/// </summary>
/// <author>Dan Singer</author>
public class CameraManager : MonoBehaviour {

    private Camera[] cameras;
    public int camIndex = 0;

	// Use this for initialization
	void Start () {
        cameras = GetComponentsInChildren<Camera>();
        //Only activate the camera at camIndex
        for (int i = 0; i < cameras.Length; i++)
            cameras[i].enabled = (i == camIndex);
	}
	
	// Update is called once per frame
	void Update () {
        //Cycle through cameras
		if (Input.GetKeyDown(KeyCode.C))
        {
            cameras[camIndex].enabled = false;
            camIndex++;
            camIndex %= cameras.Length;
            cameras[camIndex].enabled = true;
        }

	}
}
