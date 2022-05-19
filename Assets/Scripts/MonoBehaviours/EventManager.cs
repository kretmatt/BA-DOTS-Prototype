using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EventManager is responsible for providing functionality to trigger, subscribe to and unsubscribe from game related events
/// </summary>
public class EventManager : MonoBehaviour
{
    #region Variables & Accessors

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Used for managing all events and the methods subsribed to them
    /// </summary>
    private Dictionary<EEventType, Action<Dictionary<string, object>>> eventRegistry;

    /// <summary>
    /// Static instance of the EventManager class to prevent multiple EventManager instances in one scene
    /// </summary>
    private static EventManager eventManager;

    /// <summary>
    /// Public accessor for the EventManager class to retrieve the static instance of EventManager
    /// </summary>
    public static EventManager Instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    Debug.LogError("There is no EventManager instance in the entire scene!");
                }
                else
                {
                    eventManager.Init();
                    DontDestroyOnLoad(eventManager);
                }
            }

            return eventManager;
        }
    }

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method for initializing the event registry variable
    /// </summary>
    private void Init()
    {
        if (eventRegistry == null)
        {
            eventRegistry = new Dictionary<EEventType, Action<Dictionary<string, object>>>();
        }
    }


    /// <summary>
    /// Method subscribing to an event with a specific key
    /// </summary>
    /// <param name="eventType">Key of the event</param>
    /// <param name="listener">New method to be added to the event</param>
    public static void SubscribeMethodToEvent(EEventType eventType, Action<Dictionary<string, object>> listener)
    {
        Action<Dictionary<string, object>> thisEvent;

        if (Instance.eventRegistry.TryGetValue(eventType, out thisEvent))
        {
            thisEvent += listener;
            Instance.eventRegistry[eventType] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            Instance.eventRegistry.Add(eventType, thisEvent);
        }
    }


    /// <summary>
    /// Method for unsubscribing a method from the event
    /// </summary>
    /// <param name="eventType">Key of the event</param>
    /// <param name="listener">Method to be removed from the event</param>
    public static void UnsubscribeMethodFromEvent(EEventType eventType, Action<Dictionary<string, object>> listener)
    {
        if (eventManager == null) return;
        Action<Dictionary<string, object>> thisEvent;
        if (Instance.eventRegistry.TryGetValue(eventType, out thisEvent))
        {
            thisEvent -= listener;
            Instance.eventRegistry[eventType] = thisEvent;
        }
    }

    /// <summary>
    /// Method for triggering an event
    /// </summary>
    /// <param name="eventType">Key of the event</param>
    /// <param name="message">Message to be sent to all subscribed methods</param>
    public static void TriggerEvent(EEventType eventType, Dictionary<string, object> message)
    {
        Action<Dictionary<string, object>> thisEvent = null;
        if (Instance.eventRegistry.TryGetValue(eventType, out thisEvent))
        {
            thisEvent?.Invoke(message);
        }
    }

    #endregion
}

/// <summary>
/// Enum used in the event manager class, which allows the EEventType values to be used as events.
/// </summary>
public enum EEventType
{
    SCORE_INCREASE,
    SCORE_DISPLAY,
    HEALTH_DECREASE,
    HEALTH_DISPLAY,
    GAME_END,
    GAME_START,
    SET_ASTEROIDS,
    SET_ENEMIES,
    ENEMIES_SPAWNED,
    ENEMY_DEATH
}
