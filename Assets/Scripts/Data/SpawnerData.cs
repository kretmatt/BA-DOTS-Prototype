using UnityEngine;
using Unity.Entities;

/// <summary>
/// Component consisting of spawning related data
/// </summary>
[GenerateAuthoringComponent]
public struct SpawnerData : IComponentData
{
    /// <summary>
    /// Flag that determines whether entities need to be spawned
    /// </summary>
    public bool isSpawning;
}
