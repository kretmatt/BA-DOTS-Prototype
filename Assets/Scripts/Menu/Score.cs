using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script for managing the Score in the UI and saving highscores
/// </summary>
public class Score : MonoBehaviour
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Text element for the highscore
    /// </summary>
    [SerializeField] TextMeshProUGUI highScoreText;

    /// <summary>
    /// Text element for the current score
    /// </summary>
    [SerializeField] TextMeshProUGUI scoreText;

    /// <summary>
    /// Current score of the game
    /// </summary>
    [SerializeField] int score;

    /// <summary>
    /// Current highscore
    /// </summary>
    [SerializeField] int highScore;

    /// <summary>
    /// Name of the point value sent by the event manager
    /// </summary>
    private static string amountKey = "amount";

    #endregion

    #region Unity Messages

    ////////////////////////////////////////////////////////////////////
    /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. SCORE_INCREASE event:
    ///    IncreaseScore - Increases the score by the amount sent from the event manager and displays the new score. Also checks for the new highscore.  
    /// 
    /// 2. GAME_START event:
    ///    PrepareForGame - Prepares the score script for the new game.
    /// 
    /// 3. GAME_END event:
    ///    GameEndRoutine - Cleans up the score and checks whether there is a new highscore.
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.SCORE_INCREASE, IncreaseScore);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, PrepareForGame);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, GameEndRoutine);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. SCORE_INCREASE event:
    ///    IncreaseScore 
    /// 
    /// 2. GAME_START event:
    ///    PrepareForGame
    /// 
    /// 3. GAME_END event:
    ///    GameEndRoutine
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.SCORE_INCREASE, IncreaseScore);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_START, PrepareForGame);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_END, GameEndRoutine);
    }

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that checks if there is a new highscore. Further functionality, that should be executed once the game ends, can be added anytime.
    /// </summary>
    /// <param name="message">Message from the event manager.</param>
    void GameEndRoutine(Dictionary<string, object> message)
    {
        CheckNewHighScore();
    }

    /// <summary>
    /// Method that prepares the script for a new game by resetting and loading values.
    /// </summary>
    /// <param name="message">Message from the event manager</param>
    void PrepareForGame(Dictionary<string, object> message)
    {
        ResetScore();
        LoadHighScore();
        DisplayScore();
        DisplayHighScore();
    }

    /// <summary>
    /// Method that resets the score
    /// </summary>
    void ResetScore()
    {
        score = 0;
    }

    /// <summary>
    /// Method that loads the current highscore from the player preferences.
    /// </summary>
    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("highScore", 100);
    }

    /// <summary>
    /// Method that checks whether the current score is greater than the highscore. If so, it saves the current score as the new highscore.
    /// </summary>
    void CheckNewHighScore()
    {
        if (score > highScore)
        {
            PlayerPrefs.SetInt("highScore", score);
            highScore = score;
            DisplayHighScore();
        }
    }

    /// <summary>
    /// Method that increases the score by the amount of points sent by the event manager.
    /// </summary>
    /// <param name="message">Message from the event manager. Contains the amount of points to be added to the score.</param>
    void IncreaseScore(Dictionary<string, object> message)
    {
        if (message.ContainsKey(amountKey))
        {
            int amount = (int)message[amountKey];
            score += amount;
            DisplayScore();
            CheckNewHighScore();
        }
    }

    /// <summary>
    /// Sets the text of the highscore counter in the UI to the current highscore.
    /// </summary>
    void DisplayHighScore()
    {
        highScoreText.text = highScore.ToString();
    }

    /// <summary>
    /// Sets the text of the score counter in the UI to the current score.
    /// </summary>
    void DisplayScore()
    {
        scoreText.text = score.ToString();
    }
    #endregion
}
