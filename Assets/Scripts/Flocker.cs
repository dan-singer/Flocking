using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A agent that flocks and stays in bounds
/// </summary>
/// <author>Dan Singer</author>
public class Flocker : Vehicle
{
    /// <summary>
    /// Calculate and apply flocking forces
    /// </summary>
    protected override void CalcSteeringForces()
    {
        Vector3 netForce = Vector3.zero;
        netForce += Separate(GameManager.Instance.Flockers, separationInfo.radius) * separationInfo.weight;
        netForce += Align(GameManager.Instance.AverageFlockDirection) * alignInfo.weight;
        netForce += Cohere(GameManager.Instance.AverageFlockPosition) * cohereInfo.weight;
        netForce += Wander(wanderInfo.unitsAhead, wanderInfo.radius) * wanderInfo.weight;
        netForce += ConstrainTo(GameManager.Instance.innerBoundary.GetComponent<Renderer>().bounds) * constrainInfo.weight;
        netForce += Avoid(GameManager.Instance.Obstacles, avoidInfo.radius) * avoidInfo.weight;
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }
}
