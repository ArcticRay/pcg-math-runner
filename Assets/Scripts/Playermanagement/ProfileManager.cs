using UnityEngine;
using System.IO;

public static class ProfileManager
{
    private static string GetProfilePath(string playerName)
    {
        string fileName = playerName + ".json";
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public static PlayerProfile LoadOrCreateProfile(string playerName)
    {
        string path = GetProfilePath(playerName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerProfile>(json);
        }
        else
        {
            var newProfile = new PlayerProfile(playerName);
            SaveProfile(newProfile);
            return newProfile;
        }
    }

    public static void SaveProfile(PlayerProfile profile)
    {
        string json = JsonUtility.ToJson(profile, true);
        string path = GetProfilePath(profile.GetPlayerName());

        try
        {
            ScoreboardManager.AddOrUpdateEntry(
            profile.GetPlayerName(),
            profile.mmr,
            profile.GetRank()
            );
            File.WriteAllText(path, json);
            Debug.Log($"Profile saved to: {path}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Konnte Profil nicht speichern: {e.Message}");
        }
    }
}
