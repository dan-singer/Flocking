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
    public GameObject innerBounds;
    public Vector3 flockBoundSize;



    public List<Transform> Obstacles { get; private set; }
    public List<Transform> Flockers { get; private set; }

    /// <summary>
    /// Spawn obstacles and flockers
    /// </summary>
	void Start () {
        Obstacles = new List<Transform>();
        int obstacles = (int)Random.Range(obstacleMinMax.x, obstacleMinMax.y+1);
        for (int i = 0; i < obstacles; i++)
            Obstacles.Add(SpawnOnTerrain(obstaclePrefab).transform);

        Bounds flockBounds = new Bounds(Vector3.zero, flockBoundSize);
        flockBounds.center = GetRandomPosOnTerrain();

        Flockers = new List<Transform>();
        int flockers = (int)Random.Range(flockerMinMax.x, flockerMinMax.y+1);
        for (int i = 0; i < flockers; i++)
            Flockers.Add(SpawnOnTerrain(flockerPrefab, flockBounds).transform);
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

    // Update is called once per frame
    void Update () {
		
	}
}
