using System;
[Serializable]
public class PlayerProfile
{
    string playerName;
    int playerLevel;
    int playerXP;
    int xpThreshold = 20;

    public PlayerProfile(string playerName)
    {
        this.playerName = playerName;
        this.playerLevel = 0;
        this.playerXP = 0;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public int GetPlayerLevel()
    {
        return playerLevel;
    }

    public int GetPlayerXP()
    {
        return playerXP;
    }

    public void IncreasePlayerXP(int xpIncrease)
    {
        playerXP = playerXP + xpIncrease;
        if (playerXP > xpThreshold)
        {
            LevelUP();
        }
    }

    void LevelUP()
    {
        playerLevel = playerLevel + 1;
        xpThreshold = xpThreshold + xpThreshold + (playerLevel * 5);
    }
}

// implemented basic player management and leveling system