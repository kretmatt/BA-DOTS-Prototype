using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component for the projectile impact prefabs
/// </summary>
[GenerateAuthoringComponent]
public struct ProjectileImpactAuthoringScript : IComponentData
{
    /// <summary>
    /// Projectile impact prefab of the players' projectiles
    /// </summary>
    public Entity impactPrefab;
}
