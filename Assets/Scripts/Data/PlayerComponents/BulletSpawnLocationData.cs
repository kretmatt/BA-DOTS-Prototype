using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Component consisting of the spawn locations of the projectiles
/// </summary>
public struct BulletSpawnLocationData : IComponentData
{
    /// <summary>
    /// Spawn position of the first projectile
    /// </summary>
    public float3 firstCannonPosition;
    
    /// <summary>
    /// Spawn position of the second projectile
    /// </summary>
    public float3 secondCannonPosition;
}
