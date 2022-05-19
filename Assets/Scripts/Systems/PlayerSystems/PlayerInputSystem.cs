using Unity.Entities;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// System for reading the player input and converting it to movement data or closing / ending the game
/// </summary>
public partial class PlayerInputSystem : SystemBase
{
    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed every frame. Reads the input from the keys assigned to each player.
    /// The input is then converted to movement / shooting data.
    /// Aside from that, the game can also be closed / ended.
    /// </summary>
    protected override void OnUpdate()
    {
        NativeList<ESpecialInput> inputList = new NativeList<ESpecialInput>(Allocator.TempJob);

        Entities.ForEach((int nativeThreadIndex, ref PlayerShootData playerShootData, ref PlayerMoveData playerMoveData, ref HealthData health, in InputData inputData) =>
        {
            bool isShooting = Input.GetKeyDown(inputData.shootCode) || Input.GetKeyDown(inputData.optionalShootCode);
            bool forwardKeyPressed = Input.GetKey(inputData.forwardKey);
            bool turnRightKeyPressed = Input.GetKey(inputData.turnRightKey);
            bool turnLeftKeyPressed = Input.GetKey(inputData.turnLeftKey);
            bool lookUpKeyPressed = Input.GetKey(inputData.lookUpKey);
            bool lookDownKeyPressed = Input.GetKey(inputData.lookDownKey);
            bool rotateRightKeyPressed = Input.GetKey(inputData.rotateRightKey);
            bool rotateLeftKeyPressed = Input.GetKey(inputData.rotateLeftKey);

            // Set player shoot data

            playerShootData.isShooting = isShooting;

            // Set player move data

            playerMoveData.forwardMoveFactor = Convert.ToInt32(forwardKeyPressed);
            // Roll axis
            playerMoveData.rotation.x = Convert.ToInt32(rotateRightKeyPressed) - Convert.ToInt32(rotateLeftKeyPressed);
            // Pitch axis
            playerMoveData.rotation.y = Convert.ToInt32(lookDownKeyPressed) - Convert.ToInt32(lookUpKeyPressed);
            // Yaw axis
            playerMoveData.rotation.z = Convert.ToInt32(turnRightKeyPressed) - Convert.ToInt32(turnLeftKeyPressed);

            if (Input.GetKeyDown(inputData.endGameKey) && playerMoveData.moveable)
            {
                inputList.Add(ESpecialInput.GAME_END);
                health.currentHealth = health.maxHealth;
            }
            if (Input.GetKeyDown(inputData.closeGameKey))
            {
                inputList.Add(ESpecialInput.GAME_CLOSE);
            }

        }).Run();

        foreach (var specialInput in inputList)
        {
            HandleSpecialInput(specialInput);
        }

        inputList.Dispose();
    }

    /// <summary>
    /// Method for handling the special input inside the PlayerInputSystem, 
    /// that requires events / other functionality to be executed.
    /// </summary>
    /// <param name="input">Type of input</param>
    void HandleSpecialInput(ESpecialInput input)
    {
        if (input == ESpecialInput.GAME_CLOSE)
        {
            Unity.Entities.World.DisposeAllWorlds();
            Application.Quit();
        }
        else if (input == ESpecialInput.GAME_END)
        {
            EventManager.TriggerEvent(EEventType.HEALTH_DISPLAY, new System.Collections.Generic.Dictionary<string, object>() { { "percentage", 1f } });
            EventManager.TriggerEvent(EEventType.GAME_END, null);
        }
    }

    #endregion
}
/// <summary>
/// Enum used for registering events (GAME_END, GAME_CLOSE) inside a job in the system
/// </summary>
public enum ESpecialInput
{
    GAME_END,
    GAME_CLOSE
}
