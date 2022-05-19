using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component for the player shooting data
/// </summary>
[GenerateAuthoringComponent]
public struct PlayerShootData : IComponentData
{
    /// <summary>
    /// Flag that determines whether the player is currently shooting
    /// </summary>
    public bool isShooting;

    /// <summary>
    /// Flag that determines whether the player can currently shoot
    /// </summary>
    public bool canShoot;
}
