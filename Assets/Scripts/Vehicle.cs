﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Base Vehicle class which contains methods to apply traditional forces and seeking forces for autonomous agents. 
/// Extend this to create a custom agent or player.
/// </summary>
/// <author>Dan Singer</author>
[RequireComponent(typeof(DebugLineRenderer))]
[RequireComponent(typeof(Collider))]
public abstract class Vehicle : MonoBehaviour {

    //Vectors for force-based movement
    public Vector3 Acceleration { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 Direction { get; private set; }

    private const float NORMAL_FORCE_MAGNITUDE = 1;

    protected Collider coll;


    protected DebugLineRenderer debugLineRenderer;

    //Floats for force-based movement
    public float mass;
    public float maxSpeed;
    public float maxForce;


    //Detailed force info
    public PursueEvadeInfo pursueInfo;
    public PursueEvadeInfo evadeInfo;
    public SForceInfo constrainInfo;
    public SForceRadiusInfo avoidInfo;
    public SForceRadiusInfo separationInfo;
    public WanderInfo wanderInfo;
    public SForceInfo alignInfo;
    public SForceInfo cohereInfo;

    public bool ignoreY = true;

    //Note this is OPTIONAL!
    public CharacterController controller;

    //For wander variation
    protected float wanderOffset;

    // Use this for initialization
    protected virtual void Start () {
        debugLineRenderer = GetComponent<DebugLineRenderer>();
        coll = GetComponent<Collider>();
        wanderOffset = Random.Range(0, 10.0f);
    }

    /// <summary>
    /// Return the closes object of type T to this Vehicle provided a list of objects to search through.
    /// </summary>
    protected T GetNearest<T>(List<T> objects) where T : MonoBehaviour
    {
        if (objects.Count == 0)
            return default(T);
        T minObj = objects[0];
        for (int i = 1; i < objects.Count; i++)
        {
            float minDistSqr = (transform.position - minObj.transform.position).sqrMagnitude;
            float distSqr = (transform.position - objects[i].transform.position).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                minObj = objects[i];
            }
        }
        return minObj;
    }



    /// <summary>
    /// Applies a force to this vehicle's acceleration.
    /// </summary>
    /// <param name="force">Force to apply</param>
    public void ApplyForce(Vector3 force)
    {
        if (ignoreY)
            force.y = 0;
        Acceleration += (force / mass);
    }

    /// <summary>
    /// Apply a friction force.
    /// </summary>
    protected void ApplyFriction(float frictionCoeff)
    {
        Vector3 friction = frictionCoeff * NORMAL_FORCE_MAGNITUDE * -Velocity.normalized;
        if (Velocity.sqrMagnitude > Math.Pow(0.01f, 2))
            ApplyForce(friction);
    }

    /// <summary>
    /// Get a force causing the vehicle to seek the target.
    /// </summary>
    /// <returns>Seek force vector</returns>
    protected Vector3 Seek(Vector3 target)
    {

        //Desired vel = target's position - my position
        Vector3 desiredVel = target - transform.position;
        //Scale desired to max speed
        desiredVel = desiredVel.normalized * maxSpeed;
        //Steering force = desired vel - current vel
        Vector3 steerForce = desiredVel - Velocity;
        //return steering force
        return steerForce;
    }

    /// <summary>
    /// Get a seeking force away from target.
    /// </summary>
    protected Vector3 Flee(Vector3 target)
    {
        Vector3 desiredVel = transform.position - target;
        desiredVel = desiredVel.normalized * maxSpeed;
        return (desiredVel - Velocity);
    }

    /// <summary>
    /// Get a force to Pursue the target.
    /// </summary>
    /// <param name="target">Target to pursue</param>
    /// <param name="secondsAhead">How many seconds in the future to look for where this target will be</param>
    protected Vector3 Pursue(Vehicle target, float secondsAhead)
    {
        Vector3 dest = target.transform.position + (target.Velocity*secondsAhead);

        float toTargetSqr = (target.transform.position - transform.position).sqrMagnitude;
        float targetToDestSqr = (dest - target.transform.position).sqrMagnitude;

        //Just seek the target's position when too close
        Vector3 seekLoc;
        if (toTargetSqr < targetToDestSqr)
            seekLoc = target.transform.position;
        else
            seekLoc = dest;
        return Seek(dest);
    }
    /// <summary>
    /// Get a force to Evade the target.
    /// </summary>
    /// <param name="target">Target to evade</param>
    /// <param name="secondsAhead">How many seconds in the future to look for where this target will be</param>
    protected Vector3 Evade(Vehicle target, float secondsAhead)
    {
        Vector3 dest = target.transform.position + (target.Velocity * secondsAhead);
        //If I'm in between dest and target...
        float posSqr = transform.position.sqrMagnitude;
        float targetMagSqr = target.transform.position.sqrMagnitude;
        float min, max;
        if (targetMagSqr < dest.sqrMagnitude) {
            min = targetMagSqr; max = dest.sqrMagnitude;
        }
        else{
            min = dest.sqrMagnitude; max = targetMagSqr;
        }

        //If I'm in-between the target and it's future position, just flee the target so I don't run into it!
        Vector3 fleeLoc;
        if (posSqr >= min && posSqr <= max)
            fleeLoc = target.transform.position;
        else
            fleeLoc = dest;
        //Debug
        return Flee(fleeLoc);
    }

    /// <summary>
    /// Return a force which avoids the most threatening obstacle in the provided list.
    /// </summary>
    protected Vector3 Avoid(List<Transform> obstacles, float avoidRadius)
    {
        Vector3 netForce = Vector3.zero;
        Vector3 desiredVel = Vector3.zero;
        float nearest = float.MaxValue;
        foreach (Transform obstacle in obstacles)
        {
            //Ignore obstacles behind the vehicle
            Vector3 obsLocalPos = obstacle.transform.position - transform.position;
            if (ignoreY)
                obsLocalPos.y = 0;
            float fwdProj = Vector3.Dot(transform.forward, obsLocalPos);
            if (fwdProj < 0)
                continue;

            //Ignore objects too far away
            float radSum = coll.Radius + obstacle.GetComponent<Collider>().InnerRadius;
            if (obsLocalPos.sqrMagnitude > Math.Pow(radSum, 2))
                continue;

            //Test for non-intersection
            float rightProj = Vector3.Dot(transform.right, obsLocalPos);
            if (Mathf.Abs(rightProj) > radSum)
                continue;

            if (fwdProj < nearest)
            {
                nearest = fwdProj;
                desiredVel = transform.right * -Mathf.Sign(rightProj);
            }
        }
        return Seek(transform.position + desiredVel);
    }

    /// <summary>
    /// Return a force causing this vehicle to seek a somewhat random location in front of it.
    /// </summary>
    /// <param name="ahead">Distance ahead to project a circle</param>
    /// <param name="radius">Radius of the projected circle</param>
    protected Vector3 Wander(float ahead, float radius)
    {
        float normalizedAngle = Mathf.PerlinNoise(Time.time + wanderOffset, 0);
        float angle = Mathf.Lerp(-90, 90, normalizedAngle);
        Vector3 rotatedRadius = transform.forward * radius;
        rotatedRadius = Quaternion.Euler(0, angle, 0) * rotatedRadius;
        Vector3 seekPt = transform.position + (transform.forward * (ahead + radius)) + rotatedRadius;
        return Seek(seekPt);
    }

    /// <summary>
    /// Get a force to constrain the vehicle to the provided Bounds.
    /// </summary>
    protected Vector3 ConstrainTo(Bounds bounds)
    {
        float x = transform.position.x; float z = transform.position.z;
        Vector3 min = bounds.center - bounds.extents;
        Vector3 max = bounds.center + bounds.extents;
        bool outside = x < min.x || x > max.x || z < min.z || z > max.z;
        if (outside)
            return Seek(new Vector3(bounds.center.x, transform.position.y, bounds.center.z));
        else
            return Vector3.zero;
    }

    /// <summary>
    /// Return a force which causes this object to separate from a list of vehicles.
    /// </summary>
    /// <typeparam name="T">Type of object to separate from</typeparam>
    /// <param name="vehicles">List of objects to separate from</param>
    /// <param name="radius">Radius around this object in which vehicles should be separated from</param>
    protected Vector3 Separate<T>(List<T> vehicles, float radius) where T:Component
    {
        Vector3 netForce = Vector3.zero;
        float radiusSqr = radius * radius;
        //Loop through each vehicle
        foreach (T vehicle in vehicles)
        {
            Vector3 vehicleToMe = transform.position - vehicle.transform.position;
            if (vehicleToMe.sqrMagnitude == 0)
                continue;
            //if it's in my radius
            if (vehicleToMe.sqrMagnitude < radiusSqr)
            {
                Vector3 sepForce = vehicleToMe.normalized;
                float weight = 1 / vehicleToMe.sqrMagnitude;
                netForce += (sepForce * weight);
            }
        }
        return Seek(transform.position + netForce);
    }

    /// <summary>
    /// Get a force which will align this vehicle to the specified direction.
    /// </summary>
    protected Vector3 Align(Vector3 direction)
    {
        Vector3 alignment = Seek(transform.position + direction);
        return alignment;
    }

    /// <summary>
    /// Cohere to the provided center point.
    /// </summary>
    protected Vector3 Cohere(Vector3 center)
    {
        return Seek(center);
    }


    /// <summary>
    /// Set the GameObject's forward to the current Direction.
    /// </summary>
    private void SetForward()
    {
        if (Direction != Vector3.zero)
            transform.forward = Direction;
    }

    /// <summary>
    /// Calculate the steering forces for this vehicle.
    /// </summary>
    protected abstract void CalcSteeringForces();

    /// <summary>
    /// Calculate velocity and then position from the acceleration derived from forces this frame.
    /// </summary>
    private void UpdatePosition()
    {
        //New "movement formula"
        Velocity += Acceleration * Time.deltaTime;
        if (controller)
            controller.Move(Velocity * Time.deltaTime);
        else
            transform.position += Velocity * Time.deltaTime;
        //Get normalized velocity as direction
        Direction = Velocity.normalized;
        //Reset acceleration
        Acceleration = Vector3.zero;
    }

    /// <summary>
    /// Draw debug lines for this object's forward and right axes in the game view.
    /// </summary>
    protected virtual void DrawDebugLines()
    {
        debugLineRenderer.DrawLine(0, transform.position, transform.position + transform.forward);
        debugLineRenderer.DrawLine(1, transform.position, transform.position + transform.right);

    }

    /// <summary>
    /// Calculate steering forces, update the position, rotate towards calculated direction, and draw debug lines.
    /// </summary>
    protected virtual void Update () {

        CalcSteeringForces();
        UpdatePosition();
        SetForward();
        DrawDebugLines();
	}
}
