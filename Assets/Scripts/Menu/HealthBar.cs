using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script for managing the health bar in the in-game UI
/// </summary>
public class HealthBar : MonoBehaviour
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Healthbar element in the in-game UI
    /// </summary>
    [SerializeField] RectTransform healthBar;

    /// <summary>
    /// Text inside the healthbar
    /// </summary>
    [SerializeField] TextMeshProUGUI healthText;

    /// <summary>
    /// Key of the value inside the messages sent by the EventManager class
    /// </summary>
    private static readonly string percentageKey = "percentage";

    /// <summary>
    /// Maximum width of the healthbar
    /// </summary>
    float maxWidth;

    #endregion

    #region Unity Messages


    ////////////////////////////////////////////////////////////////////
    /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets executed once during the initialization process. Saves the maximum width of the healtbar element to the maxWidth variable
    /// </summary>
    private void Start()
    {
        maxWidth = healthBar.rect.width;
    }

    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. HEALTH_DISPLAY event:
    ///    UpdateHealthUI - Updates the healthbar according to the current health of the player
    /// 
    /// 2. GAME_END event:
    ///    Resets the healthbar for the next game
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.HEALTH_DISPLAY, UpdateHealthUI);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, PrepareHealthBar);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. HEALTH_DISPLAY event:
    ///    UpdateHealthUI
    /// 
    /// 2. GAME_END event:
    ///    PrepareHealthBar
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.HEALTH_DISPLAY, UpdateHealthUI);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_END, PrepareHealthBar);
    }

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method for preparing the healthbar UI elements for the next game
    /// </summary>
    /// <param name="message">Message sent from the EventManager class</param>
    private void PrepareHealthBar(Dictionary<string, object> message)
    {
        healthBar.sizeDelta = new Vector2(maxWidth, healthBar.rect.height);
        healthText.text = "Full Health";
    }

    /// <summary>
    /// Method for updating the healtbar UI elements
    /// </summary>
    /// <param name="message">Message sent from the EventManager class. The message should contain the current health percentage of the player to update the healthbar element</param>
    private void UpdateHealthUI(Dictionary<string, object> message)
    {
        if (message.ContainsKey(percentageKey))
        {
            float percentage = (float)message[percentageKey];
            healthBar.sizeDelta = new Vector2(maxWidth * percentage, healthBar.rect.height);
            healthText.text = $"Health: {percentage * 100}%";
        }
    }

    #endregion
}
