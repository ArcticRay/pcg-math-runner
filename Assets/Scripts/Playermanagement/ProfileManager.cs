using UnityEngine;
using System.IO;

public static class ProfileManager
{
    // saves profile in JSON
    public static void SaveProfile(PlayerProfile profile)
    {

        string json = JsonUtility.ToJson(profile, true);


        string fileName = profile.GetPlayerName() + ".json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(path, json);

        Debug.Log($"Profile saved to: {path}");
    }

    // Loads profile
    public static PlayerProfile LoadProfile(string playerName)
    {
        string fileName = playerName + ".json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Profile {fileName} not found at {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<PlayerProfile>(json);
    }
}
