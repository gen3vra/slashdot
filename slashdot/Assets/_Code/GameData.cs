using System;

[Serializable]
public class GameData
{
    /// <summary>
    /// Not saved, per game
    /// </summary>
    public int score;
    public int totalScore;
    public int highScore;
    public int combo;
    /// <summary>
    /// per game
    /// </summary>
    public int gameHighestCombo;
    public int highestCombo;

    public GameData(GameData migrateData = null)
    {
        if (migrateData != null)
        {
            //score = migrateData.score;
            totalScore = migrateData.totalScore;
            highScore = migrateData.highScore;
            highestCombo = migrateData.highestCombo;
        }
        else
        // Initialize default values
        {
            score = 0;
            totalScore = 0;
            highScore = 0;
            combo = 0;
            highestCombo = 0;
            gameHighestCombo = 0;
        }
    }
}
