using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Script for the timer in the game. Is also responsible for ending the game once the time runs out
/// </summary>
public class GameTimer : MonoBehaviour
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Text element of the timer
    /// </summary>
    [SerializeField] TextMeshProUGUI timerText;

    /// <summary>
    /// Max duration of the game
    /// </summary>
    [SerializeField] float gameTime;

    /// <summary>
    /// Time left of the game
    /// </summary>
    [SerializeField] float timeLeft;

    /// <summary>
    /// Flag for deciding whether to track time or not
    /// </summary>
    bool trackTime = false;

    #endregion

    #region Unity Messages

    ////////////////////////////////////////////////////////////////////
    /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. GAME_START event:
    ///    StartTimer - Starts the timer and sets the current time to the maximum value 
    /// 
    /// 2. GAME_END event:
    ///    StopTimer - Stops the timer
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, StartTimer);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, StopTimer);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. GAME_START event:
    ///    StartTimer
    /// 
    /// 2. GAME_END event:
    ///    StopTimer 
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_START, StartTimer);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_END, StopTimer);
    }

    /// <summary>
    /// Method that gets called every frame.
    /// If the timer keeps track of the time, the text element of the timer gets updated every frame. In addition, once the timer reaches 0 the game ends
    /// </summary>
    private void Update()
    {
        if (trackTime)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerDisplay();
            if (timeLeft <= 0)
                EventManager.TriggerEvent(EEventType.GAME_END, new Dictionary<string, object>() { { "state", true} });
        }
    }


    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method for starting the timer. Sets the current time to the maximum 
    /// </summary>
    /// <param name="message">Message sent from the EventManager class if the associated event gets triggered</param>
    void StartTimer(Dictionary<string, object> message)
    {
        timeLeft = gameTime;
        trackTime = true;
    }

    /// <summary>
    /// Method for stopping the timer
    /// </summary>
    /// <param name="message">Message sent from the EventManager class if the associated event gets triggered</param>
    void StopTimer(Dictionary<string, object> message)
    {
        trackTime = false;
    }

    /// <summary>
    /// Method for updating the timer text element
    /// </summary>
    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        float seconds = timeLeft % 60;

        timerText.text = $"{minutes}:{seconds:00}";
    }

    #endregion
}
