using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandCreator : MonoBehaviour
{
    public int terrainSize = 128; // Size of the terrain in units
    public float noiseScale = 20f; // Perlin noise scale for height generation
    public float maxHeight = 10f; // Maximum height of the terrain
    public float centerHumpScale = 0.3f; // Scale for a gentle central hump

    public Material grassMaterial;
    public Material sandMaterial;
    public Material waterMaterial;

    public GameObject[] treePrefabs; // Tree prefabs
    public GameObject[] rockPrefabs; // Rock prefabs
    public GameObject[] fallStuffPrefabs; // Leaves etc. prefabs
    public GameObject[] grassPrefabs; // Grass prefabs

    public float treeDensity = 0.02f; // Tree density (0-1)
    public float rockDensity = 0.01f; // Rock density (0-1)
    public float grassDensity = 0.05f; // Grass density (0-1)
    public float fallStuffDensity = 0.05f; // Grass density (0-1)


    public GameObject waterPlane; // A plane representing water surrounding the island

    void Start()
    {
        GenerateIsland();
    }

    void GenerateIsland()
    {
        GameObject terrain = new GameObject("IslandTerrain");
        terrain.transform.parent = transform;

        MeshFilter filter = terrain.AddComponent<MeshFilter>();
        MeshRenderer renderer = terrain.AddComponent<MeshRenderer>();
        MeshCollider collider = terrain.AddComponent<MeshCollider>();

        Mesh islandMesh = GenerateTerrainMesh();
        filter.mesh = islandMesh;
        collider.sharedMesh = islandMesh;
        renderer.material = grassMaterial;

        PlaceObjectsOnIsland(islandMesh);

        CreateWaterPlane();
    }

    Mesh GenerateTerrainMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int z = 0; z <= terrainSize; z++)
        {
            for (int x = 0; x <= terrainSize; x++)
            {
                float worldX = (float)x / terrainSize;
                float worldZ = (float)z / terrainSize;

                float height = CalculateHeight(worldX, worldZ);
                vertices.Add(new Vector3(x, height, z));
                uvs.Add(new Vector2(worldX, worldZ));
            }
        }

        for (int z = 0; z < terrainSize; z++)
        {
            for (int x = 0; x < terrainSize; x++)
            {
                int startIndex = z * (terrainSize + 1) + x;

                triangles.Add(startIndex);
                triangles.Add(startIndex + terrainSize + 1);
                triangles.Add(startIndex + 1);

                triangles.Add(startIndex + 1);
                triangles.Add(startIndex + terrainSize + 1);
                triangles.Add(startIndex + terrainSize + 2);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    float CalculateHeight(float worldX, float worldZ)
    {
        float baseHeight = Mathf.PerlinNoise(worldX * noiseScale, worldZ * noiseScale) * maxHeight * 0.2f; // Reduce small hill height

        // Circular island effect with falloff map
        float centerX = 0.5f; // Center of the island in normalized coordinates
        float centerZ = 0.5f;
        float distanceToCenter = Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(centerX, centerZ));
        float maxDistance = 0.5f; // Maximum radius of the island

        float falloff = Mathf.Clamp01(distanceToCenter / maxDistance);
        baseHeight *= (1 - falloff);

        // Gentle central hump effect
        float centerHump = Mathf.Exp(-distanceToCenter * distanceToCenter * centerHumpScale) * (maxHeight * 0.4f);
        baseHeight += centerHump;

        // Sand transition near water
        if (distanceToCenter >= maxDistance * 0.8f)
        {
            baseHeight = Mathf.Lerp(0, baseHeight, (maxDistance - distanceToCenter) / (maxDistance * 0.2f));
        }

        return baseHeight;
    }

    void PlaceObjectsOnIsland(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;

        foreach (Vector3 vertex in vertices)
        {
            if (vertex.y > 1f && vertex.y < maxHeight - 2f)
            {
                // Place trees
                if (Random.value < treeDensity)
                {
                    PlaceObject(treePrefabs, vertex);
                }

                // Place rocks
                if (Random.value < rockDensity)
                {
                    PlaceObject(rockPrefabs, vertex);
                }

                // Place grass
                if (Random.value < grassDensity)
                {
                    PlaceObject(grassPrefabs, vertex);
                }

                // Place leaves etc.
                if (Random.value < fallStuffDensity)
                {
                    PlaceObject(fallStuffPrefabs, vertex);
                }
            }
        }
    }

    void PlaceObject(GameObject[] prefabs, Vector3 position)
    {
        if (prefabs.Length == 0) return;

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    void CreateWaterPlane()
    {
        GameObject water = Instantiate(waterPlane, transform);
        water.transform.localScale = new Vector3(terrainSize / 5f, 1, terrainSize / 5f);
        water.transform.position = new Vector3(terrainSize / 2, 0.1f, terrainSize / 2);
        water.GetComponent<Renderer>().material = waterMaterial;
    }
}