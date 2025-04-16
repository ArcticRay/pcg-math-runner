using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    public SplineManager splineManager;
    public int width = 100;
    public float pathWidth = 10f;
    public float noiseScale = 0.5f;
    public float noiseAmplitude = 1f;
    public float islandFalloffStart = 30f;
    public float islandFalloffStrength = 1f;

    public float flatRange = 8f;
    public float smoothFalloffStart = 20f;

    private Mesh mesh;
    private List<Vector3> terrainVertices = new List<Vector3>();
    private List<int> terrainTriangles = new List<int>();
    private List<Vector2> terrainUVs = new List<Vector2>();

    private int lastGeneratedIndex = 0;

    private void Start()
    {
        if (splineManager == null)
        {
            Debug.LogError("SplineManager muss zugewiesen sein!");
            return;
        }

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        splineManager.OnSplineExtended += GenerateTerrain;
        GenerateTerrain(splineManager.GetSplinePoints());
    }

    void GenerateTerrain(List<Vector3> splinePoints)
    {
        int vertsPerRow = (width * 2) + 1;

        for (int i = lastGeneratedIndex; i < splinePoints.Count; i++)
        {
            Vector3 center = splinePoints[i];
            Vector3 forward = Vector3.right;

            if (i < splinePoints.Count - 1)
                forward = (splinePoints[i + 1] - center).normalized;
            else if (i > 0)
                forward = (center - splinePoints[i - 1]).normalized;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            for (int x = -width; x <= width; x++)
            {
                Vector3 offset = right * x;
                Vector3 pos = center + offset;

                float distanceFromPath = Mathf.Abs(x);
                float height = 0f;

                if (distanceFromPath > flatRange * 0.5f)
                {
                    float falloff = Mathf.InverseLerp(flatRange * 0.5f, smoothFalloffStart, distanceFromPath);
                    float noise = Mathf.PerlinNoise(pos.x * noiseScale, pos.z * noiseScale) * noiseAmplitude;
                    height = Mathf.Lerp(noise, -2f, falloff * islandFalloffStrength);
                }

                terrainVertices.Add(new Vector3(pos.x, center.y + height, pos.z));
                terrainUVs.Add(new Vector2((float)x / width, (float)i / splinePoints.Count));
            }
        }

        int startRow = Mathf.Max(0, lastGeneratedIndex - 1);
        for (int z = startRow; z < splinePoints.Count - 1; z++)
        {
            for (int x = 0; x < (width * 2); x++)
            {
                int rowOffset = z * ((width * 2) + 1);
                int start = rowOffset + x;

                terrainTriangles.Add(start);
                terrainTriangles.Add(start + vertsPerRow);
                terrainTriangles.Add(start + 1);

                terrainTriangles.Add(start + 1);
                terrainTriangles.Add(start + vertsPerRow);
                terrainTriangles.Add(start + vertsPerRow + 1);
            }
        }

        lastGeneratedIndex = splinePoints.Count;
        UpdateMesh();
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.SetVertices(terrainVertices);
        mesh.SetTriangles(terrainTriangles, 0);
        mesh.SetUVs(0, terrainUVs);
        mesh.RecalculateNormals();
    }
}
