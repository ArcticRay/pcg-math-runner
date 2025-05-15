using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PathTerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int chunkSize = 64;
    public float scale = 1f;
    public float heightMultiplier = 10f;
    public AnimationCurve heightCurve;

    [Header("Path Settings")]
    public int pathWidth = 3;
    public AnimationCurve pathBlendCurve;

    [Header("Visual Settings")]
    public Material terrainMaterial;

    private float[,] heightMap;

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        heightMap = GenerateHeightMap();
        List<Vector2Int> path = GenerateStraightPath();
        AdjustHeightMapToPath(path);
        Mesh mesh = GenerateMesh(heightMap);

        // Apply mesh and material
        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = terrainMaterial;
    }

    float[,] GenerateHeightMap()
    {
        float[,] map = new float[chunkSize + 1, chunkSize + 1];
        for (int z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float nx = (float)x / chunkSize;
                float nz = (float)z / chunkSize;
                float noise = Mathf.PerlinNoise(nx * 5f, nz * 5f);
                map[x, z] = heightCurve.Evaluate(noise);
            }
        }
        return map;
    }

    List<Vector2Int> GenerateStraightPath()
    {
        List<Vector2Int> path = new();
        int centerX = chunkSize / 2;
        for (int z = 0; z <= chunkSize; z++)
        {
            path.Add(new Vector2Int(centerX, z));
        }
        return path;
    }

    void AdjustHeightMapToPath(List<Vector2Int> path)
    {
        for (int z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float minDist = float.MaxValue;
                foreach (Vector2Int p in path)
                {
                    float dist = Vector2.Distance(new Vector2(x, z), new Vector2(p.x, p.y));
                    if (dist < minDist) minDist = dist;
                }

                if (minDist < pathWidth)
                {
                    float t = pathBlendCurve.Evaluate(1 - minDist / pathWidth);
                    heightMap[x, z] = Mathf.Lerp(heightMap[x, z], 0.05f, t);
                }
            }
        }
    }

    Mesh GenerateMesh(float[,] heightMap)
    {
        int width = chunkSize + 1;
        int length = chunkSize + 1;
        Vector3[] vertices = new Vector3[width * length];
        Vector2[] uvs = new Vector2[width * length];
        int[] triangles = new int[chunkSize * chunkSize * 6];

        int index = 0;
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float y = heightMap[x, z] * heightMultiplier;
                vertices[index] = new Vector3(x * scale, y, z * scale);
                uvs[index] = new Vector2((float)x / chunkSize, (float)z / chunkSize);
                index++;
            }
        }

        int triIndex = 0;
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int start = z * width + x;
                triangles[triIndex++] = start;
                triangles[triIndex++] = start + width;
                triangles[triIndex++] = start + 1;
                triangles[triIndex++] = start + 1;
                triangles[triIndex++] = start + width;
                triangles[triIndex++] = start + width + 1;
            }
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();
        return mesh;
    }
}
