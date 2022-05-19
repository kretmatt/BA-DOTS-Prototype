using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component for the projectile duration data. Determines when the projectile despawns automatically
/// </summary>
[GenerateAuthoringComponent]
public struct TimeToLiveData : IComponentData
{
    /// <summary>
    /// Time since the projectile was instantiated
    /// </summary>
    public float currentDuration;
    
    /// <summary>
    /// Maximum duration of the projectile
    /// </summary>
    public float maxDuration;

    /// <summary>
    /// Constructor of the TimeToLiveData component
    /// </summary>
    /// <param name="maxDuration">Maximum duration of the projectile</param>
    public TimeToLiveData(float maxDuration)
    {
        this.maxDuration = maxDuration;
        this.currentDuration = 0;
    }
}
