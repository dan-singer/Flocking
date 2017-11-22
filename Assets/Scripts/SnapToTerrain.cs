using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Causes a gameobject to always snap to the terrain
/// </summary>
/// <author>Dan Singer</author>
public class SnapToTerrain : MonoBehaviour {

    public Terrain terrain;
    public Renderer rend;

	/// <summary>
    /// Initialize terrain and renderer
    /// </summary>
	void Start () {
        if (!terrain)
            terrain = Terrain.activeTerrain;
        if (!rend)
            rend = GetComponent<Renderer>();
	}
	
	/// <summary>
    /// Each frame, snap the object's y-component to the terrain's height at the object's position
    /// </summary>
	void Update () {
        if (!terrain)
            return;
        transform.position = new Vector3(
            transform.position.x, terrain.SampleHeight(transform.position) + rend.bounds.extents.y, transform.position.z);
	}
}
