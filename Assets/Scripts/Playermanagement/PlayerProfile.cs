using System;
using UnityEngine;

[Serializable]
public class PlayerProfile
{

    public string playerName;
    public int playerLevel;
    public int xp;
    public int levelUPThreshold;

    public int mmr;

    public Difficulty rank;

    private static readonly int[] RankThresholds = {
        1000, // BEGINNER
        1100, // NOVICE
        1200, // APPRENTICE
        1300, // PROFICIENT
        1400, // EXPERIENCED
        1500, // ADVANCED
        1600, // CHALLENGING
        1700, // DIFFICULT
        1800, // EXPERT
        1900  // LEGENDARY
    };

    public PlayerProfile() { }

    public PlayerProfile(string name, int level = 1, int xp = 0, int levelUPThreshold = 100)
    {
        this.playerName = name;
        this.playerLevel = level;
        this.xp = xp;
        this.levelUPThreshold = levelUPThreshold;
        this.mmr = 1000;
        this.rank = Difficulty.BEGINNER;

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

    public void UpdateMMR(int delta)
    {
        mmr = Mathf.Max(0, mmr + delta);

        // Up-rank
        if ((int)rank < RankThresholds.Length - 1
            && mmr >= RankThresholds[(int)rank + 1])
        {
            rank += 1;
            Debug.Log($"↑ Up-ranked to {rank} ({mmr} MMR)");
        }
        // Down-rank
        else if ((int)rank > 0
                 && mmr < RankThresholds[(int)rank])
        {
            rank -= 1;
            Debug.Log($"↓ Down-ranked to {rank} ({mmr} MMR)");
        }
    }
}
