using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public int mapWidth = 10;
    public int mapHeight = 10;

    public float noiseScale = 0.3f;

    public bool autoUpdate;


    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

        MapDisplay display = FindFirstObjectByType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }
}
