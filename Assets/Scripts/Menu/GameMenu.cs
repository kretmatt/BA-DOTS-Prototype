using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for toggling the menu of the game
/// </summary>
public class GameMenu : MonoBehaviour
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// GameObject of the menu
    /// </summary>
    [SerializeField] GameObject startMenu;

    /// <summary>
    /// GameObject of the UI during the game
    /// </summary>
    [SerializeField] GameObject gameUI;

    /// <summary>
    /// FPS counter 
    /// </summary>
    [SerializeField] GameObject fpsCounter;
    #endregion

    #region Unity Messages

    ////////////////////////////////////////////////////////////////////
    /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Show main menu when the game starts for the first time.
    /// </summary>
    private void Start()
    {
        startMenu.SetActive(true);
        gameUI.SetActive(false);
        fpsCounter.SetActive(false);
    }

    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. GAME_START event:
    ///    ToggleStartMenu - Toggles the game menu and the in-game UI 
    /// 
    /// 2. GAME_END event:
    ///    ToggleStartMenu - Toggles the game menu and the in-game UI
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, ToggleStartMenu);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, ToggleStartMenu);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. GAME_START event:
    ///    ToggleStartMenu
    /// 
    /// 2. GAME_END event:
    ///    ToggleStartMenu
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_START, ToggleStartMenu);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_END, ToggleStartMenu);
    }

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Toggles the game menu and its in-game UI
    /// </summary>
    /// <param name="message">Values used to trigger the event the method is subscribed to.</param>
    void ToggleStartMenu(Dictionary<string, object> message)
    {
        if (message!=null && message.ContainsKey("state"))
        {
            bool state = (bool)message["state"];

            startMenu.SetActive(state);
            gameUI.SetActive(!state);
            fpsCounter.SetActive(!state);
        }
        else
        {
            startMenu.SetActive(!startMenu.activeInHierarchy);
            gameUI.SetActive(!gameUI.activeInHierarchy);
            fpsCounter.SetActive(!fpsCounter.activeInHierarchy);
        }
    }

    #endregion
}
