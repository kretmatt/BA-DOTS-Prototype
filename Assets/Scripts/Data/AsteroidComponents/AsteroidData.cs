using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component for asteroid data
/// </summary>
[GenerateAuthoringComponent]
public struct AsteroidData : IComponentData
{
    /// <summary>
    /// Damage dealt by the asteroid
    /// </summary>
    public int damage;
}
