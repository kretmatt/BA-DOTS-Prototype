using Unity.Entities;
using UnityEngine;


/// <summary>
/// Component for other general boid / enemy data
/// </summary>
[GenerateAuthoringComponent]
public struct GeneralBoidData : IComponentData
{
    /// <summary>
    /// Damage dealt to players on collision
    /// </summary>
    public int damage;
}
