using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component used for the health of an entity
/// </summary>
[GenerateAuthoringComponent]
public struct HealthData : IComponentData
{
    /// <summary>
    /// Current health value of the entity
    /// </summary>
    public int currentHealth;

    /// <summary>
    /// Maximum health value of the entity
    /// </summary>
    public int maxHealth;
}
