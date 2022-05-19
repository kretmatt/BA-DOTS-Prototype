using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component for defining the boid / enemy prefab
/// </summary>
[GenerateAuthoringComponent]
public struct BoidAuthoringScript : IComponentData
{
    /// <summary>
    /// Prefab for the boid / enemy
    /// </summary>
    public Entity boidPrefab;
}