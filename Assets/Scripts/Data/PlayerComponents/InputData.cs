using UnityEngine;
using Unity.Entities;

/// <summary>
/// Component for the player controls data
/// </summary>
[GenerateAuthoringComponent]
public struct InputData : IComponentData
{
    /// <summary>
    /// Key for shooting projectiles
    /// </summary>
    public KeyCode shootCode;

    /// <summary>
    /// Second key for shooting projectiles
    /// </summary>
    public KeyCode optionalShootCode;

    /// <summary>
    /// Key for moving forward
    /// </summary>
    public KeyCode forwardKey;

    /// <summary>
    /// Key for turning left
    /// </summary>
    public KeyCode turnLeftKey;

    /// <summary>
    /// Key for turning right
    /// </summary>
    public KeyCode turnRightKey;

    /// <summary>
    /// Key for rotating left
    /// </summary>
    public KeyCode rotateLeftKey;
    
    /// <summary>
    /// Key for rotating right
    /// </summary>
    public KeyCode rotateRightKey;
    
    /// <summary>
    /// Key for looking down
    /// </summary>
    public KeyCode lookUpKey;
    
    /// <summary>
    /// Key for looking up
    /// </summary>
    public KeyCode lookDownKey;

    /// <summary>
    /// Key for ending the current game
    /// </summary>
    public KeyCode endGameKey;

    /// <summary>
    /// Key for closing the game alltogether
    /// </summary>
    public KeyCode closeGameKey;
}
