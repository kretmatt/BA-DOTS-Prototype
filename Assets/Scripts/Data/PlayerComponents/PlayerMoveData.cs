using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Component for the player movement data
/// </summary>
[GenerateAuthoringComponent]
public struct PlayerMoveData : IComponentData
{

    /// <summary>
    /// Value that determines how much the player wants to move forward
    /// </summary>
    public float forwardMoveFactor;
    
    /// <summary>
    /// rotation contains data for the rotation on the yaw, pitch, and roll axes
    /// </summary>
    public float3 rotation;

    /// <summary>
    /// Flag that determines whether the player can actually move at the moment
    /// </summary>
    public bool moveable;
    
    /// <summary>
    /// Forwared movement speed of the player
    /// </summary>
    public float forwardMoveSpeed;
    
    /// <summary>
    /// Rotation speed of the player
    /// </summary>
    public float turnSpeed;
}
