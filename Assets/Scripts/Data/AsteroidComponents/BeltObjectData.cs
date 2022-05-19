using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Component data for entities that are part of an asteroid belt
/// </summary>
public struct BeltObjectData : IComponentData
{
    /// <summary>
    /// The speed of the entity inside the asteroid belt
    /// </summary>
    public float orbitSpeed;

    /// <summary>
    /// Flag that determines whether the entity rotates around the center of the asteroid belt clockwise or not
    /// </summary>
    public bool rotationClockwise;

    /// <summary>
    /// Center of the asteroid belt
    /// </summary>
    public float3 parentPosition;

    /// <summary>
    /// Up vector of the asteroid belt center
    /// </summary>
    public float3 parentUp;
}
