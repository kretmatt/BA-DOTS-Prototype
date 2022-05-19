using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component for the projectile prefabs
/// </summary>
[GenerateAuthoringComponent]
public struct ProjectileAuthoringComponent : IComponentData
{
    /// <summary>
    /// Projectile prefab for the player
    /// </summary>
    public Entity projectilePrefab;
}
