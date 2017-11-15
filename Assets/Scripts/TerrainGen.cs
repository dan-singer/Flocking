using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component which generates a random terrain based on Perlin noise
/// </summary>
/// <author>Dan Singer</author>
public class TerrainGen : MonoBehaviour {

    public float step = 0.1f;
    public int resolution = 128;

    private Terrain terrain;

	// Use this for initialization
	void Start () {
        terrain = GetComponent<Terrain>();
        terrain.terrainData.heightmapResolution = resolution;

        Vector2 input = new Vector2(0, 0);
        float[,] heights = new float[resolution, resolution];
        for (int y=0; y<heights.GetLength(0); y++)
        {
            for (int x=0; x<heights.GetLength(1); x++)
            {
                heights[y, x] = Mathf.PerlinNoise(input.x, input.y);
                input.x += step;
            }
            input.y += step;
            input.x = 0;
        }

        terrain.terrainData.SetHeights(0, 0, heights);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
