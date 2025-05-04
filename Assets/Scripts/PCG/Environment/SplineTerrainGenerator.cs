using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Terrain))]
public class SplineTerrainGenerator : MonoBehaviour
{
    [Header("Referenzen")]
    public SplineManager splineManager;

    [Header("Tile-Einstellungen")]
    public int tileResolution = 256;
    public Vector3 tileSize = new Vector3(50, 10, 50);

    [Header("Formradius")]
    public float flatRadius = 2f;
    public float islandRadius = 10f;
    public float beachRadius = 15f;

    [Header("Noise‑Parameter")]
    public float noiseScale = 0.1f;
    public float noiseAmplitude = 2f;

    [Header("Textur‑Schwellen (Welt‑Höhe)")]
    public float beachHeight = 0.5f;
    public float grassHeight = 2f;

    private Dictionary<Vector2Int, Terrain> tiles = new();
    private Vector3 origin;
    private Terrain baseTerrain;
    private TerrainData baseData;

    void Start()
    {
        if (!Application.isPlaying || splineManager == null)
        {
            enabled = false;
            return;
        }

        baseTerrain = GetComponent<Terrain>();
        baseData = baseTerrain.terrainData;

        origin = transform.position - new Vector3(tileSize.x * 0.5f, 0, tileSize.z * 0.5f);

        baseData.heightmapResolution = tileResolution;
        baseData.alphamapResolution = tileResolution;
        baseData.size = tileSize;
        baseTerrain.terrainData = baseData;
        baseTerrain.transform.position = origin;
        tiles.Add(Vector2Int.zero, baseTerrain);

        splineManager.OnSplineExtended += HandleSplineExtended;
        HandleSplineExtended(splineManager.GetSplinePoints());
    }

    void HandleSplineExtended(List<Vector3> splinePoints)
    {
        var newTiles = EnsureTilesCoverSpline(splinePoints);
        foreach (var coord in newTiles)
            ApplyHeightsAndTextures(tiles[coord], coord, splinePoints);
    }

    List<Vector2Int> EnsureTilesCoverSpline(List<Vector3> pts)
    {
        var created = new List<Vector2Int>();
        foreach (var p in pts)
        {
            var idx = WorldToTileCoord(p);
            if (!tiles.ContainsKey(idx))
            {
                var clone = Instantiate(baseData);
                clone.heightmapResolution = tileResolution;
                clone.alphamapResolution = tileResolution;
                clone.size = tileSize;

                var go = Terrain.CreateTerrainGameObject(clone);
                go.transform.position = origin + new Vector3(idx.x * tileSize.x, 0, idx.y * tileSize.z);
                go.name = $"Tile_{idx.x}_{idx.y}";

                var t = go.GetComponent<Terrain>();
                tiles.Add(idx, t);
                created.Add(idx);
            }
        }
        return created;
    }

    void ApplyHeightsAndTextures(Terrain terrain, Vector2Int coord, List<Vector3> splinePoints)
    {
        var data = terrain.terrainData;
        int res = data.heightmapResolution;
        int aRes = data.alphamapResolution;
        int layers = data.terrainLayers.Length;
        if (layers < 3) return;

        // Alte Höhen für unberührte Bereiche übernehmen
        float[,] heights = data.GetHeights(0, 0, res, res);
        float[,,] alphas = new float[aRes, aRes, layers];

        float normBeachH = beachHeight / data.size.y;
        float normGrassH = grassHeight / data.size.y;
        Vector3 tileOrigin = terrain.transform.position;

        for (int x = 0; x < res; x++)
        for (int z = 0; z < res; z++)
        {
            float wx = tileOrigin.x + x / (float)(res - 1) * data.size.x;
            float wz = tileOrigin.z + z / (float)(res - 1) * data.size.z;

            var result = MinDistanceToSpline(new Vector2(wx, wz), splinePoints);
            float dist = result.dist;
            bool beforeStart = (result.segIndex == 0 && result.rawT < 0f);
            bool afterEnd = (result.segIndex == splinePoints.Count - 2 && result.rawT > 1f);
            bool openZone = beforeStart || afterEnd;

            if (!openZone && dist <= beachRadius)
            {
                // Höhe der Spline interpoliert
                int i = result.segIndex;
                float tRaw = Mathf.Clamp01(result.rawT);
                float baseH = Mathf.Lerp(splinePoints[i].y, splinePoints[i + 1].y, tRaw);

                float newH = baseH;
                float noise = Mathf.PerlinNoise(wx * noiseScale, wz * noiseScale) * noiseAmplitude;

                if (dist > flatRadius)
                {
                    if (dist <= islandRadius)
                    {
                        float t = Mathf.InverseLerp(flatRadius, islandRadius, dist);
                        float blend = Mathf.SmoothStep(0f, 1f, t);
                        newH = baseH + noise * blend;
                    }
                    else
                    {
                        float t2 = Mathf.InverseLerp(islandRadius, beachRadius, dist);
                        float blend2 = 1f - t2;
                        newH = baseH + noise * blend2;
                    }
                }

                heights[z, x] = Mathf.Clamp01(newH / data.size.y);
            }
        }

        // Kantenanpassung für nahtlose Übergänge
        Vector2Int[] neigh = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        foreach (var off in neigh)
        {
            Vector2Int nIdx = coord + off;
            if (!tiles.ContainsKey(nIdx)) continue;
            var nT = tiles[nIdx];
            for (int k = 0; k < res; k++)
            {
                float worldX, worldZ;
                if (off == Vector2Int.left || off == Vector2Int.right)
                {
                    worldX = tileOrigin.x + (off == Vector2Int.left ? 0 : data.size.x);
                    worldZ = tileOrigin.z + k / (float)(res - 1) * data.size.z;
                    heights[k, off == Vector2Int.left ? 0 : res - 1] = Mathf.Clamp01(
                        nT.SampleHeight(new Vector3(worldX, 0, worldZ)) / data.size.y);
                }
                else
                {
                    worldX = tileOrigin.x + k / (float)(res - 1) * data.size.x;
                    worldZ = tileOrigin.z + (off == Vector2Int.down ? 0 : data.size.z);
                    heights[off == Vector2Int.down ? 0 : res - 1, k] = Mathf.Clamp01(
                        nT.SampleHeight(new Vector3(worldX, 0, worldZ)) / data.size.y);
                }
            }
        }

        data.SetHeights(0, 0, heights);

        // Texture Painting
        for (int x = 0; x < aRes; x++)
        for (int z = 0; z < aRes; z++)
        {
            int hmX = Mathf.Clamp(Mathf.RoundToInt(x * (res - 1f) / (aRes - 1f)), 0, res - 1);
            int hmZ = Mathf.Clamp(Mathf.RoundToInt(z * (res - 1f) / (aRes - 1f)), 0, res - 1);
            float hVal = heights[hmZ, hmX];

            alphas[z, x, 0] = hVal <= normBeachH ? 1f : 0f;
            alphas[z, x, 1] = (hVal > normBeachH && hVal <= normGrassH) ? 1f : 0f;
            alphas[z, x, 2] = hVal > normGrassH ? 1f : 0f;
        }
        data.SetAlphamaps(0, 0, alphas);
    }

    struct DistanceResult { public float dist; public int segIndex; public float rawT; }

    DistanceResult MinDistanceToSpline(Vector2 p, List<Vector3> pts)
    {
        var result = new DistanceResult { dist = float.MaxValue, segIndex = 0, rawT = 0f };
        for (int i = 0; i < pts.Count - 1; i++)
        {
            Vector2 a = new Vector2(pts[i].x, pts[i].z);
            Vector2 b = new Vector2(pts[i + 1].x, pts[i + 1].z);
            Vector2 ab = b - a;
            float rawT = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
            float t = Mathf.Clamp01(rawT);
            Vector2 proj = a + t * ab;
            float d = Vector2.Distance(p, proj);
            if (d < result.dist)
            {
                result.dist = d;
                result.segIndex = i;
                result.rawT = rawT;
            }
        }
        return result;
    }

    Vector2Int WorldToTileCoord(Vector3 p)
    {
        int ix = Mathf.FloorToInt((p.x - origin.x) / tileSize.x);
        int iz = Mathf.FloorToInt((p.z - origin.z) / tileSize.z);
        return new Vector2Int(ix, iz);
    }
}

// Ergänzung im SplineManager:
// public event Action<List<Vector3>> OnSplineExtended;
// und in ExtendSpline(): OnSplineExtended?.Invoke(splinePoints);
