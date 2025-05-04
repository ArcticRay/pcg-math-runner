using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineChunkGenerator : MonoBehaviour
{
    public AnimationCurve terrainCurve;
    public AnimationCurve convergenceToSpline;
    public float meshHeightMultiplier = 50f;
    public float scale = 1f;
    public int chunkSize = 64;
    List<Vector3> splinePoints;
    
    private float[,] heightMap;
    private float[,] distanceToSpline;

    void Start()
    {
        // GenerateChunk();
    }

    public void GenerateChunk(List<Vector3> splinePoints)
    {
        this.splinePoints = splinePoints;
        heightMap = GenerateHeightMap();
        AdjustHeightmapToSpline();
        Mesh mesh = GenerateMesh();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        Debug.Log("Chunk generiert");
    }

    float[,] GenerateHeightMap()
    {
        float[,] map = new float[chunkSize + 1, chunkSize + 1];

        for (int z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float sampleX = (transform.position.x + x * scale) * 0.01f;
                float sampleZ = (transform.position.z + z * scale) * 0.01f;
                float noise = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                map[x, z] = terrainCurve.Evaluate(noise);
            }
        }

        return map;
    }

    void AdjustHeightmapToSpline()
    {
        distanceToSpline = new float[chunkSize + 1, chunkSize + 1];
        Vector2 chunkOffset = new Vector2(transform.position.x, transform.position.z);
        

        for (int z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                Vector2 worldPos = chunkOffset + new Vector2(x * scale, z * scale);
                float minDistance = float.MaxValue;
                float splineHeight = 0;

                for (int i = 0; i < splinePoints.Count - 1; i++)
                {
                    Vector2 a = new Vector2(splinePoints[i].x, splinePoints[i].z);
                    Vector2 b = new Vector2(splinePoints[i + 1].x, splinePoints[i + 1].z);
                    Vector2 closest = ClosestPointOnSegment(a, b, worldPos);
                    float dist = Vector2.Distance(worldPos, closest);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        splineHeight = Mathf.Lerp(splinePoints[i].y, splinePoints[i + 1].y, 
                            Mathf.InverseLerp(0, Vector2.Distance(a, b), Vector2.Distance(a, closest)));
                    }
                }

                distanceToSpline[x, z] = minDistance;

                float influence = Mathf.Clamp01(1 - (minDistance / 10f));
                heightMap[x, z] = Mathf.Lerp(heightMap[x, z], splineHeight / meshHeightMultiplier, convergenceToSpline.Evaluate(influence));
            }
        }
    }

    Mesh GenerateMesh()
    {
        int width = heightMap.GetLength(0);
        int length = heightMap.GetLength(1);
        Vector3[] vertices = new Vector3[width * length];
        Vector2[] uvs = new Vector2[width * length];
        int[] triangles = new int[(width - 1) * (length - 1) * 6];

        int vertIndex = 0;
        int triIndex = 0;
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float y = heightMap[x, z] * meshHeightMultiplier;
                vertices[vertIndex] = new Vector3(x * scale + transform.position.x, y, z * scale + transform.position.z);
                uvs[vertIndex] = new Vector2(x / (float)width, z / (float)length);

                if (x < width - 1 && z < length - 1)
                {
                    int a = vertIndex;
                    int b = a + width;
                    int c = b + 1;
                    int d = a + 1;

                    triangles[triIndex++] = a;
                    triangles[triIndex++] = b;
                    triangles[triIndex++] = c;
                    triangles[triIndex++] = a;
                    triangles[triIndex++] = c;
                    triangles[triIndex++] = d;
                }

                vertIndex++;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

    Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }
}
