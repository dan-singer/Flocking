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

    protected override void CalcSteeringForces()
    {
        Vector3 netForce = Vector3.zero;
        netForce += Separate(GameManager.Instance.Flockers, separationInfo.radius) * separationInfo.weight;
        netForce = Vector3.ClampMagnitude(netForce, maxForce);
        ApplyForce(netForce);
    }
}
