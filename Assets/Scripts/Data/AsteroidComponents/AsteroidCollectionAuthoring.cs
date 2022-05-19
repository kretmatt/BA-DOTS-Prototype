using Unity.Entities;
using UnityEngine;

/// <summary>
/// Component for the asteroid collection data used by the asteroid belt spawner
/// </summary>
[GenerateAuthoringComponent]
public struct AsteroidCollectionAuthoring : IComponentData
{
    /// <summary>
    /// The light asteroid with the least health
    /// </summary>
    public Entity lightAsteroid;
    
    /// <summary>
    /// The medium asteroid with medium health
    /// </summary>
    public Entity mediumAsteroid;
    
    /// <summary>
    /// The heavy asteroid with the most health
    /// </summary>
    public Entity heavyAsteroid;
}
