using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the flocking simulation
/// </summary>
/// <author>Dan Singer</author>
public class GameManager : MonoBehaviour {

    private static GameManager instance;
    /// <summary>
    /// Singleton pattern
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    //Prefabs
    public GameObject obstaclePrefab;
    public GameObject flockerPrefab;

    //Min max info
    public Vector2 obstacleMinMax;
    public Vector2 flockerMinMax;

    //Terrain and boundaries
    public Terrain terrain;
    public GameObject innerBoundary;
    public Vector3 flockBoundSize;


    //Object lists
    public List<Transform> Obstacles { get; private set; }
    public List<Transform> Flockers { get; private set; }


    //Store average flock direction and location
    public Vector3 AverageFlockDirection { get; private set; }
    public Vector3 AverageFlockPosition { get; private set; }

    
    private DebugLineRenderer debugLineRenderer;

    /// <summary>
    /// Spawn obstacles and flockers
    /// </summary>
	void Start () {
        debugLineRenderer = GetComponent<DebugLineRenderer>();
        Obstacles = new List<Transform>();
        int obstacles = (int)Random.Range(obstacleMinMax.x, obstacleMinMax.y+1);
        for (int i = 0; i < obstacles; i++)
            Obstacles.Add(SpawnOnTerrain(obstaclePrefab, innerBoundary.GetComponent<Renderer>().bounds).transform);

        Bounds flockBounds = new Bounds(Vector3.zero, flockBoundSize);
        flockBounds.center = GetRandomPosOnTerrain(innerBoundary.GetComponent<Renderer>().bounds);

        Flockers = new List<Transform>();
        int flockers = (int)Random.Range(flockerMinMax.x, flockerMinMax.y+1);
        for (int i = 0; i < flockers; i++)
            Flockers.Add(SpawnOnTerrain(flockerPrefab, flockBounds).transform);

        DebugLineRenderer.Draw = true;
    }

    /// <summary>
    /// Spawn an object on the terrain, with an optional inner Boundary
    /// </summary>
    public GameObject SpawnOnTerrain(GameObject original, Bounds? innerBounds=null)
    {
        Vector3 loc = GetRandomPosOnTerrain(innerBounds) + new Vector3(0, original.GetComponent<Renderer>().bounds.extents.y, 0);
        GameObject newGo = Instantiate<GameObject>(original, loc, Quaternion.identity);
        return newGo;
    }

    /// <summary>
    /// Get a random position on the terrain, with an optional inner Boundary
    /// </summary>
    public Vector3 GetRandomPosOnTerrain(Bounds? innerBounds = null)
    {
        Bounds horizontalBounds = innerBounds.HasValue ? innerBounds.Value : terrain.terrainData.bounds;
        float minX = horizontalBounds.min.x;
        float maxX = horizontalBounds.max.x;
        float minZ = horizontalBounds.min.z;
        float maxZ = horizontalBounds.max.z;

        Vector3 loc = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minZ, maxZ));
        loc.y = terrain.SampleHeight(loc);
        return loc;
    }

    /// <summary>
    /// Each frame, calculate average flock location and direction, and draw debug lines and shapes.
    /// </summary>
    void Update () {

        //Calculate average flock direction and position
        Vector3 avgDir = Vector3.zero;
        Vector3 avgPos = Vector3.zero;
        foreach (Transform flocker in Flockers)
        {
            avgDir += flocker.forward;
            avgPos += flocker.position;
        }
        AverageFlockDirection = avgDir.normalized;
        if (Flockers.Count > 0)
            AverageFlockPosition = avgPos / Flockers.Count;

        //Draw debug lines
        if (debugLineRenderer)
        {
            debugLineRenderer.SetShapeLocation(AverageFlockPosition);
            //This is so the camera will match the flock's direction
            debugLineRenderer.SetShapeFwd(AverageFlockDirection); 
            debugLineRenderer.DrawLine(0, AverageFlockPosition, AverageFlockPosition + AverageFlockDirection * 5);
        }
    }
}
