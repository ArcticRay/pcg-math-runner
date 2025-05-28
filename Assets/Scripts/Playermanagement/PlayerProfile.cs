using System;
using UnityEngine;

[Serializable]
public class PlayerProfile
{

    public string playerName;
    public int playerLevel;
    public int xp;

    public int levelUPThreshold;

    public PlayerProfile() { }

    public PlayerProfile(string name, int level = 1, int xp = 0, int levelUPThreshold = 100)
    {
        this.playerName = name;
        this.playerLevel = level;
        this.xp = xp;
        this.levelUPThreshold = levelUPThreshold;
    }

    public string GetPlayerName() => playerName;
    public int GetPlayerLevel() => playerLevel;
    public int GetXP() => xp;

    public void SetPlayerName(string name) => playerName = name;
    public void SetPlayerLevel(int level) => playerLevel = level;
    public void SetXP(int xpAmount) => xp = xpAmount;

    public void IncreasePlayerXP(int amount)
    {
        xp += amount;
        if (xp > levelUPThreshold)
        {
            LevelUP();
            levelUPThreshold += (100 * playerLevel);
        }
    }

    public void LevelUP()
    {
        playerLevel = playerLevel + 1;
    }
}
