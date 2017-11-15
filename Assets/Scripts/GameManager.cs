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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
