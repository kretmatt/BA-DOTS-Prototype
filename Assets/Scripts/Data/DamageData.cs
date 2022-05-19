using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component for damage dealt to asteroids / players
/// </summary>
[GenerateAuthoringComponent]
public struct DamageData : IComponentData
{
    /// <summary>
    /// Amount of damage dealt to the entity
    /// </summary>
    public int damageDealt;
}
