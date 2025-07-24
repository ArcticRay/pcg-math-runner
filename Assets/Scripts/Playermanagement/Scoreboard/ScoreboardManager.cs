using System.IO;
using UnityEngine;

public static class ScoreboardManager
{
    private static string FileName => "scoreboard.json";
    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static ScoreboardData LoadScoreboard()
    {
        if (!File.Exists(FilePath))
            return new ScoreboardData();

        string json = File.ReadAllText(FilePath);
        return JsonUtility.FromJson<ScoreboardData>(json) ?? new ScoreboardData();
    }

    public static void SaveScoreboard(ScoreboardData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"Scoreboard saved to: {FilePath}");
    }

    public static void AddOrUpdateEntry(string playerName, int mmr, Difficulty rank)
    {
        var data = LoadScoreboard();
        var entry = data.entries.Find(e => e.playerName == playerName);

        if (entry != null)
        {
            entry.mmr = mmr;
            entry.rank = rank;
        }
        else
        {
            data.entries.Add(new ScoreboardEntry
            {
                playerName = playerName,
                mmr = mmr,
                rank = rank
            });
        }

        data.entries.Sort((a, b) => b.mmr.CompareTo(a.mmr));
        SaveScoreboard(data);
    }
}