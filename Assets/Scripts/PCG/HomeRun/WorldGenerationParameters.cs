using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldGenerationParameters", menuName = "World/Generation Parameters")]
public class WorldGenerationParameters : ScriptableObject
{
    public Daytime daytime;
    public WorldType worldType;
    public string worldName;
    public int walkDuration;
    public int chunkSize;
    public int scale;
    public float noisefactor;
    public int octaves;
    public float persistence;
    public float lacunarity;
    public int seed;
    public float MeshHeightMultiplier;
    public int renderDistance;
    public float maximumPathTilt;
    public int pathSegmentLength;
    public int minPathSegmentLength;
    public int onPathDistance;
    public int nearPathDistance;
    public float maximumIncline;
    public int seaLevel;
    public int sandLevel;
    public int snowLevel;
    public float hillyness;
    public bool pineForestBiome;
    public bool deciduousForestBiome;
    public bool mixedForestBiome;
    public bool fieldBiome;
    public bool snowyMountain;
    public bool lakes;

    public WorldGenerationParameters()
    {
        daytime = Daytime.NOON;
        worldType = WorldType.ISLAND;
        worldName = "Neue Welt";
        walkDuration = 1800;
        chunkSize = 80;
        scale = 1;
        noisefactor = 2200;
        octaves = 7;
        persistence = 0.43f;
        lacunarity = 2;
        seed = 0;
        MeshHeightMultiplier = 400;
        renderDistance = 2;
        maximumPathTilt = 30;
        pathSegmentLength = 8;
        minPathSegmentLength = 5;
        onPathDistance = 30;
        nearPathDistance = 100;
        maximumIncline = 0.2f;
        seaLevel = -160;
        sandLevel = -150;
        snowLevel = 400;
        hillyness = 0.5f;
        pineForestBiome = true;
        deciduousForestBiome = true;
        mixedForestBiome = true;
        fieldBiome = true;
        snowyMountain = true;
        lakes = true;
    }

    public void RevalidateParameters()
    {
        if (worldType == WorldType.OCEAN)
        {
            noisefactor = Mathf.Lerp(4000, 3400, hillyness);
            MeshHeightMultiplier = Mathf.Lerp(150, 250, hillyness);
            renderDistance = 2;
            seaLevel = -120;
            sandLevel = -110;
            snowLevel = int.MaxValue;
        }
        else if (worldType == WorldType.ISLAND)
        {
            noisefactor = Mathf.Lerp(3200, 3400, hillyness);
            seaLevel = 2;
        }
    }

    public int ScaledChunkSize()
    {
        return chunkSize * scale;
    }

    public int GetNumerOfTiles()
    {
        const float velocity = 20;
        const float tileLength = 30;
        return (int)((walkDuration * velocity) / tileLength);
    }
}