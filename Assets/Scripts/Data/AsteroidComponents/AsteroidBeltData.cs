using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component for the asteroid belt data
/// </summary>
[GenerateAuthoringComponent]
public struct AsteroidBeltData : IComponentData
{
    /// <summary>
    /// Amount of asteroids to spawn
    /// </summary>
    public int numberOfAsteroids;

    /// <summary>
    /// Seed of the asteroid belt
    /// </summary>
    public uint asteroidBeltSeed;

    /// <summary>
    /// Inner radius of the asteroid belt
    /// </summary>
    public float beltInnerRadius;

    /// <summary>
    /// Outer radius of the asteroid belt
    /// </summary>
    public float beltOuterRadius;

    /// <summary>
    /// Height of the asteroid belt
    /// </summary>
    public float beltHeight;

    /// <summary>
    /// Movement speed of the asteroid belt objects
    /// </summary>
    public float beltObjectOrbitSpeed;

    /// <summary>
    /// Flag that determines the direction the belt objects are moving in. True is clockwise, false is counter-clockwise
    /// </summary>
    public bool beltRotationDirection;
}
