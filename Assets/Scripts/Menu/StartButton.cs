using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple script for starting the game
/// </summary>
public class StartButton : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Amount of enemies to spawn
    /// </summary>
    public int boidAmount = 10;

    /// <summary>
    /// Amount of asteroids to spawn
    /// </summary>
    public int asteroidAmount = 100;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets called once in the entire lifetime of the script. Subscribes methods for setting asteroids and enemies to the respective events
    /// </summary>
    private void Awake()
    {
        EventManager.SubscribeMethodToEvent(EEventType.SET_ASTEROIDS, SetAsteroids);
        EventManager.SubscribeMethodToEvent(EEventType.SET_ENEMIES, SetEnemies);
    }

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method for receiving the amount of asteroids from the respective dropdown menu
    /// </summary>
    /// <param name="message">Message from the EventManager class. Contains the amount of asteroids to spawn</param>
    void SetAsteroids(Dictionary<string, object> message)
    {
        if (message.ContainsKey("amount"))
        {
            int amount = (int)message["amount"];
            asteroidAmount = amount;
        }
    }

    /// <summary>
    /// Method for receiving the amount of enemies from the respective dropdown menu
    /// </summary>
    /// <param name="message">Message from the EventManager class. Contains the amount of enemies to spawn</param>
    void SetEnemies(Dictionary<string, object> message)
    {
        if (message.ContainsKey("enemies"))
        {
            int amount = (int)message["enemies"];
            boidAmount = amount;
        }
    }

    /// <summary>
    /// Method bound to a button. Once the button is clicked, the GAME_START event will be triggered.
    /// </summary>
    public void Click()
    {
        EventManager.TriggerEvent(EEventType.GAME_START, new Dictionary<string, object>() { { "asteroids", asteroidAmount }, { "enemies", boidAmount } });
    }

    #endregion
}
