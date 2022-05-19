using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Transforms;
using UnityEngine;
using Unity.Jobs;

/// <summary>
/// System for moving the player
/// </summary>
public partial class PlayerMovementSystem : SystemBase
{
    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed when the system is created. Used for subscribing methods to events:
    /// 1. GAME_START event:
    ///    ToggleShooting - Enables the controls for moving the player once the game starts
    /// 1. GAME_END event:
    ///    ToggleShooting - Enables the controls for moving the player once the game ends
    /// </summary>
    protected override void OnCreate()
    {
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, ToggleMoveable);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, ToggleMoveable);
    }

    /// <summary>
    /// Method for toggling the controls of the player characters
    /// </summary>
    /// <param name="message">Message from the EventManager class</param>
    void ToggleMoveable(Dictionary<string, object> message)
    {
        if (message != null && message.ContainsKey("state"))
        {
            bool canMove = (bool)message["state"];

            JobHandle toggleHandle = Entities.ForEach((ref PlayerMoveData playerMoveData) =>
            {
                playerMoveData.moveable = !canMove;
            }).ScheduleParallel(Dependency);
            toggleHandle.Complete();
        }
        else
        {
            JobHandle toggleHandle = Entities.ForEach((ref PlayerMoveData playerMoveData) =>
            {
                playerMoveData.moveable = !playerMoveData.moveable;
            }).ScheduleParallel(Dependency);
            toggleHandle.Complete();
        }


    }

    /// <summary>
    /// Method that gets executed every frame. Retrieves the movement data after it is set in PlayerInputSystem.
    /// Afterwards, the values are taken to manipulate the rotation and the position of the players.
    /// </summary>
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Translation position, ref Rotation rotation, in PlayerMoveData playerMoveData) =>
        {
            if (playerMoveData.moveable)
            {
                float3 temporaryTranslation = position.Value;
                Quaternion temporaryRotation = rotation.Value;

                float yaw = playerMoveData.turnSpeed * deltaTime * playerMoveData.rotation.z;
                float pitch = playerMoveData.turnSpeed * deltaTime * playerMoveData.rotation.y;
                float roll = playerMoveData.turnSpeed * deltaTime * playerMoveData.rotation.x;

                if (yaw != 0)
                    temporaryRotation = math.mul(temporaryRotation, quaternion.RotateY(yaw));
                if (pitch != 0)
                    temporaryRotation = math.mul(temporaryRotation, quaternion.RotateX(pitch));
                if (roll != 0)
                    temporaryRotation = math.mul(temporaryRotation, quaternion.RotateZ(roll));

                float3 playerForward = temporaryRotation * Vector3.forward;
                float3 targetUp = temporaryRotation * Vector3.up;

                temporaryTranslation += playerForward * deltaTime * playerMoveData.forwardMoveSpeed * playerMoveData.forwardMoveFactor;

                position.Value = temporaryTranslation;
                rotation.Value = temporaryRotation;
            }
        }).ScheduleParallel();
    }
    #endregion
}
