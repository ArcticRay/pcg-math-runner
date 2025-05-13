using System;
using UnityEngine;
using System.IO;

public class WorldGenerationParameterSerialization
{
    private static readonly string cachePath = Path.Combine(Application.dataPath, "Cache");
    private static readonly string fileName = Path.Combine(cachePath, "worldgenconfig.json");

    public static void CreateDefaultConfigFile()
    {
        if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);
        var parameters = ScriptableObject.CreateInstance<WorldGenerationParameters>();
        SaveWorldGenerationParameters(parameters);
    }

    public static WorldGenerationParameters GetWorldGenerationParameters()
    {
        try
        {
            var json = File.ReadAllText(fileName);
            //can i load it as worldGenerationParameters? and then load it agains as a specific type?
            var parameters = ScriptableObject.CreateInstance<WorldGenerationParameters>();
            JsonUtility.FromJsonOverwrite(json, parameters);
            // Ignore Seed for random worlds
            parameters.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            return parameters;
        }
        catch (FileNotFoundException)
        {
            Debug.LogError("The file does not exist.");
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred: " + e.Message);
        }

        return null;
    }

    public static void SaveWorldGenerationParameters(WorldGenerationParameters parameters)
    {
        var json = JsonUtility.ToJson(parameters);
        File.WriteAllText(fileName, json);
        var path = Path.Combine(cachePath, "SavedGames");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, parameters.worldName + ".json"), json);
    }
}