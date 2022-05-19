using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// SpawnPreferences is responsible for taking the selected amount of enemies / asteroids and sending the value using the EventManager class
/// </summary>
public class SpawnPreferences : MonoBehaviour
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Dropdown menu for selecting the amount of asteroids
    /// </summary>
    [SerializeField]
    TMP_Dropdown asteroidsDropDown;

    /// <summary>
    /// Dropdown menu for selecting the amount of enemies
    /// </summary>
    [SerializeField]
    TMP_Dropdown enemySpawnRateDropDown;


    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    ///  Method bound to a dropdown menu (asteroidsDropDown). Once the selected value changes, the new amount of asteroids will be sent via the EventManager.
    /// </summary>
    /// <param name="selection">Index of the selected option</param>
    public void AsteroidsSelectionChanged(int selection)
    {
        int amount = Convert.ToInt32(asteroidsDropDown.options[selection].text);

        EventManager.TriggerEvent(EEventType.SET_ASTEROIDS, new Dictionary<string, object>() { { "amount", amount } });
    }

    /// <summary>
    /// Method bound to a dropdown menu (enemySpawnRateDropDown). Once the selected value changes, the new amount of enemies (boids) will be sent via the EventManager.
    /// </summary>
    /// <param name="selection">Index of the selected option</param>
    public void EnemySpawnRateSelectionChanged(int selection)
    {
        int amount = Convert.ToInt32(enemySpawnRateDropDown.options[selection].text);

        EventManager.TriggerEvent(EEventType.SET_ENEMIES, new Dictionary<string, object>() { { "enemies", amount } });
    }

    #endregion
}
