using System;
using System.Collections.Generic;
using UnityEngine;

public class IslandDecorator : Decorator
{
    [Header("Prefabs")]
    public GameObject[] palmPrefabs;
    public GameObject[] grassPrefabs;
    public GameObject[] bushPrefabs;
    public GameObject[] flowerPrefabs;
    public GameObject[] rockPrefabs;
    public GameObject[] cliffPrefabs;
    public GameObject waterPrefab;

    [Header("Underwater Prefabs")]
    public GameObject[] underwaterPlantPrefabs;
    [Range(0f, 1f)] public float underwaterPlantDensity = 0.05f;

    [Header("Density Settings (0–1)")]
    [Range(0f, 1f)] public float palmDensity = 0.01f;
    [Range(0f, 1f)] public float bushDensity = 0.01f;
    [Range(0f, 1f)] public float flowerDensity = 0.05f;
    [Range(0f, 1f)] public float rockDensity = 0.002f;
    [Range(0f, 1f)] public float grassPatchDensity = 0.1f;
    [Range(0f, 1f)] public float cliffDensity = 0.001f;
    public float cliffMinPathDistance = 500f;
    [Range(0f, 90f)] public float rockMaxSlope = 40f;
    [Range(0f, 90f)] public float cliffMaxSlope = 20f;

    [Header("Map Colors")]
    public Color deepWaterColor;
    public Color shallowWaterColor;
    public Color sandColor;
    public Color rockColor;
    public Color vegetationLowColor;
    public Color vegetationHighColor;
    public Color pathColor;

    public Material grassMaterial;
    public Material sandMaterial;

    public float sandThreshold = 20f;

    public override void ApplyShader(MeshRenderer meshRenderer, WorldGenerationParameters parameters)
    {
        meshRenderer.materials = new Material[]{
        sandMaterial,
        grassMaterial
    };

        // base.ApplyShader(meshRenderer, parameters);
        // if (meshRenderer.material.HasProperty("_Sea_Level"))
        //    meshRenderer.material.SetFloat("_Sea_Level", parameters.seaLevel);
        // if (meshRenderer.material.HasProperty("_Sand_Level"))
        //     meshRenderer.material.SetFloat("_Sand_Level", parameters.sandLevel);
    }

    public override void DecorateChunk(Mesh chunkMesh, TerrainChunkGenerator chunkGenerator, WorldGenerationParameters parameters)
    {
        Vector3[] vertices = chunkMesh.vertices;
        Vector3[] normals = chunkMesh.normals;
        Vector2[] pathProximity = chunkMesh.uv3;

        int seaLevel = parameters.seaLevel;
        int sandLevel = parameters.sandLevel;

        var random = new System.Random(
            parameters.seed
          + (int)chunkGenerator.GetOffset().x
          + (int)(chunkGenerator.GetOffset().y / parameters.ScaledChunkSize())
        );

        // --- Underwater Plants ---
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = chunkGenerator.transform.position + vertices[i];
            float height = worldPos.y;

            // nur unter Wasser und nicht auf Pfad
            if (height < seaLevel && !WithinDistanceToPath(pathProximity, i, parameters.onPathDistance))
            {
                if (random.NextDouble() < underwaterPlantDensity)
                {
                    var (plant, _) = RandomGameObject(
                        chunkGenerator, random, worldPos, Quaternion.identity, underwaterPlantPrefabs
                    );
                    RandomUpscale(plant, random, 10, 20);
                    RandomRotation(plant, random);
                }
            }
        }

        // Wasser-Seen / Pools
        if (parameters.lakes)
        {
            int worldSize = parameters.chunkSize * parameters.scale;
            const int step = 50;
            for (int x = 0; x < worldSize; x += step)
                for (int z = 0; z < worldSize; z += step)
                {
                    Vector3 pos = chunkGenerator.transform.position + new Vector3(x, seaLevel, z);
                    var (water, _) = RandomGameObject(chunkGenerator, random, pos, Quaternion.identity, waterPrefab);
                    RandomUpscale(water, random, 10, 10);
                }
        }

        // Dekoration pro Vertex
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = chunkGenerator.transform.position + vertices[i];
            float height = worldPos.y;
            float slope = Vector3.Angle(normals[i], Vector3.up);

            // Nur über Sand-Level
            if (height < sandLevel)
                continue;
            // Keine Deko auf dem Pfad oder zu nah
            if (WithinDistanceToPath(pathProximity, i, parameters.onPathDistance + 5f))
                continue;

            // Klippen
            if (slope <= cliffMaxSlope
             && !WithinDistanceToPath(pathProximity, i, cliffMinPathDistance)
             && random.NextDouble() < cliffDensity)
            {
                Vector3 spawnPos = worldPos + PositionNoise(random, 0f);
                var (cliff, _) = RandomGameObject(
                    chunkGenerator, random, spawnPos, Quaternion.identity, cliffPrefabs
                );
                RandomUpscale(cliff, random, 1, 10);
                RandomRotation(cliff, random);
                continue;
            }

            if (slope > rockMaxSlope)
                continue;

            Vector3 spawnNoisy = worldPos + PositionNoise(random, 0f);

            // Palmen
            if (random.NextDouble() < palmDensity)
            {
                var (palm, _) = RandomGameObject(chunkGenerator, random, spawnNoisy, Quaternion.identity, palmPrefabs);
                RandomUpscale(palm, random, 10, 20);
                RandomRotation(palm, random);
                continue;
            }

            // Büsche
            if (random.NextDouble() < bushDensity)
            {
                var (bush, _) = RandomGameObject(chunkGenerator, random, spawnNoisy, Quaternion.identity, bushPrefabs);
                RandomUpscale(bush, random, 5, 15);
                RandomRotation(bush, random);
            }

            // Blumen
            if (random.NextDouble() < flowerDensity)
            {
                var (flower, _) = RandomGameObject(chunkGenerator, random, spawnNoisy, Quaternion.identity, flowerPrefabs);
                RandomUpscale(flower, random, 3, 8);
            }

            // gewöhnliche Felsen
            if (random.NextDouble() < rockDensity)
            {
                var (rock, _) = RandomGameObject(chunkGenerator, random, spawnNoisy, Quaternion.identity, rockPrefabs);
                RandomUpscale(rock, random, 5, 15);
                RandomRotation(rock, random);
            }

            // Gras-Patches nahe Pfad
            if (WithinDistanceToPath(pathProximity, i, 300f)
             && random.NextDouble() < grassPatchDensity)
            {
                var (grass, _) = RandomGameObject(
                    chunkGenerator, random, spawnNoisy,
                    TiltWithTerrain(normals[i]), grassPrefabs
                );
                grass.transform.localScale = new Vector3(20f, 15f, 20f);
            }
        }

        SplitMeshByHeight(chunkMesh, sandThreshold);
    }

    private void SplitMeshByHeight(Mesh mesh, float threshold)
    {
        var verts = mesh.vertices;
        var tris = mesh.triangles;
        var grassTris = new List<int>();
        var sandTris = new List<int>();

        for (int i = 0; i < tris.Length; i += 3)
        {
            float avgY = (verts[tris[i + 0]].y
                        + verts[tris[i + 1]].y
                        + verts[tris[i + 2]].y) / 3f;

            if (avgY < threshold)
                grassTris.AddRange(new[] { tris[i + 0], tris[i + 1], tris[i + 2] });
            else
                sandTris.AddRange(new[] { tris[i + 0], tris[i + 1], tris[i + 2] });
        }

        mesh.subMeshCount = 2;
        mesh.SetTriangles(grassTris.ToArray(), 0);
        mesh.SetTriangles(sandTris.ToArray(), 1);
        mesh.RecalculateNormals();
    }

    public override Texture2D CreateMap(
        Mesh chunkMesh,
        Vector2 position,
        WorldGenerationParameters parameters,
        int isoInterval
    )
    {
        Vector3[] vertices = chunkMesh.vertices;
        Vector3[] normals = chunkMesh.normals;
        Vector2[] pathProximity = chunkMesh.uv3;
        Color[] colourMap = new Color[vertices.Length];

        int dim = (int)Math.Sqrt(vertices.Length);
        float maxIso = (parameters.MeshHeightMultiplier * 2.5f) / isoInterval;

        for (int i = 0; i < vertices.Length; i++)
        {
            float h = vertices[i].y + parameters.MeshHeightMultiplier;
            int isoVal = (int)(h / isoInterval);

            if (WithinDistanceToPath(pathProximity, i, parameters.onPathDistance))
                colourMap[i] = pathColor;
            else if (h < parameters.seaLevel)
            {
                float t = Mathf.InverseLerp(0, parameters.seaLevel, h);
                colourMap[i] = Color.Lerp(deepWaterColor, shallowWaterColor, t);
            }
            else if (h < parameters.sandLevel)
                colourMap[i] = sandColor;
            else if (Vector3.Angle(normals[i], Vector3.up) > rockMaxSlope)
                colourMap[i] = rockColor;
            else
            {
                float t = Mathf.InverseLerp(0, maxIso, isoVal);
                colourMap[i] = Color.Lerp(vegetationLowColor, vegetationHighColor, t);
            }
        }

        var texture = new Texture2D(dim, dim) { wrapMode = TextureWrapMode.Clamp };
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }
}
