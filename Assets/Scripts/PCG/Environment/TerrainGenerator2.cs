using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator2 : MonoBehaviour
{
    public SplineManager splineManager;

    [Header("Terrain Materials")]
    public Material pathMaterial;       // Material für den Pfad (Gras)
    public Material islandMaterial;     // Material für die Insel (Sand)
    public Material waterMaterial;      // Material für Wasser (optional)

    [Header("Terrain Parameters")]
    public float pathWidth = 4f;        // Breite des Pfades
    public float islandWidth = 15f;     // Breite der Insel vom Pfad aus
    public float noiseScale = 0.2f;     // Skalierung des Noise für Ränder
    public float edgeVariation = 2.5f;  // Variation der Randbreite
    public float pathHeight = 0.05f;    // Höhe des Pfades über der Basis
    public float islandDepth = 0.3f;    // Tiefe der Insel unter dem Pfad
    public float waterHeight = -0.5f;   // Höhe des Wassers
    public float uvScale = 3f;          // Skalierung der UV-Koordinaten

    private List<GameObject> generatedChunks = new List<GameObject>();
    private int lastProcessedPoint = 0;
    private GameObject waterPlane;

    void Start()
    {
        if (splineManager != null)
        {
            // Vorhandene Spline-Punkte verarbeiten
            GenerateTerrainForExistingSpline();

            // Wasser erstellen, falls gewünscht
            if (waterMaterial != null)
                CreateWaterPlane();

            // Event-Listener für neue Spline-Punkte
            splineManager.OnSplineExtended += OnSplineExtended;
        }
        
    }

    void CreateWaterPlane()
    {
        waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterPlane.name = "WaterPlane";
        waterPlane.transform.parent = transform;
        waterPlane.transform.position = new Vector3(0, waterHeight, 0);
        waterPlane.transform.localScale = new Vector3(100, 1, 100);
        waterPlane.GetComponent<MeshRenderer>().material = waterMaterial;
    }

    void GenerateTerrainForExistingSpline()
    {
        List<Vector3> points = splineManager.GetSplinePoints();
        if (points.Count > 1)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                GenerateTerrainSegment(points[i], points[i + 1], i);
            }
            lastProcessedPoint = points.Count - 2;
        }
    }

    void OnSplineExtended(List<Vector3> allPoints)
    {
        // Verarbeite nur neue Punkte
        if (allPoints.Count > lastProcessedPoint + 2)
        {
            for (int i = lastProcessedPoint + 1; i < allPoints.Count - 1; i++)
            {
                GenerateTerrainSegment(allPoints[i], allPoints[i + 1], i);
            }
            lastProcessedPoint = allPoints.Count - 2;

            // Aktualisiere Wasser
            if (waterPlane != null && allPoints.Count > 0)
            {
                UpdateWaterPlane(allPoints[allPoints.Count - 1]);
            }
        }
    }

    void GenerateTerrainSegment(Vector3 start, Vector3 end, int index)
    {
        // Container für diesen Chunk
        GameObject chunk = new GameObject($"TerrainChunk_{index}");
        chunk.transform.parent = this.transform;

        // WICHTIG: Zuerst die Insel (Sand) generieren
        GameObject islandObj = new GameObject("Island");
        islandObj.transform.parent = chunk.transform;

        MeshFilter islandMeshFilter = islandObj.AddComponent<MeshFilter>();
        MeshRenderer islandMeshRenderer = islandObj.AddComponent<MeshRenderer>();
        islandMeshRenderer.material = islandMaterial;

        // Insel muss unter dem Pfad sein, aber mit Überlappung um Lücken zu vermeiden
        Mesh islandMesh = CreateTerrainMesh(start, end, index, islandWidth, -islandDepth, true);
        islandMeshFilter.mesh = islandMesh;

        MeshCollider islandCollider = islandObj.AddComponent<MeshCollider>();
        islandCollider.sharedMesh = islandMesh;

        // DANN den Pfad generieren (kleinere Breite, höhere Position)
        GameObject pathObj = new GameObject("Path");
        pathObj.transform.parent = chunk.transform;

        MeshFilter pathMeshFilter = pathObj.AddComponent<MeshFilter>();
        MeshRenderer pathMeshRenderer = pathObj.AddComponent<MeshRenderer>();
        pathMeshRenderer.material = pathMaterial;

        // Der Pfad sollte leicht über der Insel liegen
        Mesh pathMesh = CreateTerrainMesh(start, end, index, pathWidth, pathHeight, false);
        pathMeshFilter.mesh = pathMesh;

        MeshCollider pathCollider = pathObj.AddComponent<MeshCollider>();
        pathCollider.sharedMesh = pathMesh;

        generatedChunks.Add(chunk);
    }

    Mesh CreateTerrainMesh(Vector3 start, Vector3 end, int index, float width, float heightOffset, bool applyNoise)
    {
        Mesh mesh = new Mesh();

        // Richtungsvektor zwischen Start und Ende
        Vector3 direction = (end - start).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;

        float segmentLength = Vector3.Distance(start, end);

        // Mehr Segmente für glattere Übergänge
        int lengthSegments = Mathf.Max(4, Mathf.CeilToInt(segmentLength * 2));
        int widthSegments = 10;

        Vector3[] vertices = new Vector3[(lengthSegments + 1) * (widthSegments + 1)];
        Vector2[] uvs = new Vector2[(lengthSegments + 1) * (widthSegments + 1)];

        // WICHTIG: Spline-Position genau verfolgen
        for (int l = 0; l <= lengthSegments; l++)
        {
            float lPercent = l / (float)lengthSegments;

            // Lineare Interpolation zwischen Start- und Endpunkt
            Vector3 pathPoint = Vector3.Lerp(start, end, lPercent);

            for (int w = 0; w <= widthSegments; w++)
            {
                float wPercent = w / (float)widthSegments;
                float wOffset = Mathf.Lerp(-width / 2f, width / 2f, wPercent);

                float finalOffset = wOffset;
                float verticalChange = 0.001f;

                // Nur bei der Insel Noise anwenden
                if (applyNoise)
                {
                    // Weniger Noise nahe am Pfad für bessere Übergänge
                    float edgeFactor = Mathf.SmoothStep(0f, 1f, Mathf.Abs(wPercent - 0.5f) * 2f);

                    // Noise für die Ränder - Sameninput für zusammenhängende Segmente
                    float noiseInput = (pathPoint.x * noiseScale) + (pathPoint.z * noiseScale);
                    float edgeNoise = Mathf.PerlinNoise(noiseInput, wPercent * 2.5f) * edgeVariation;

                    finalOffset = wOffset * (1 + edgeNoise * edgeFactor);

                    // Höhenvariation - steiler am Rand
                    float depthNoise = Mathf.PerlinNoise(noiseInput * 2, wPercent * 5) * 0.5f;
                    verticalChange -= depthNoise * Mathf.SmoothStep(0f, 1f, edgeFactor);
                }

                int idx = l * (widthSegments + 1) + w;

                // Finale Position mit korrekter Höhe und Breite
                vertices[idx] = new Vector3(
                    pathPoint.x + right.x * finalOffset,
                    pathPoint.y + verticalChange,
                    pathPoint.z + right.z * finalOffset
                );

                // Verbesserte UV-Koordinaten für besseres Texturing
                uvs[idx] = new Vector2(lPercent * uvScale, wPercent * uvScale);
                Debug.DrawLine(
                pathPoint + Vector3.up * 2,
                vertices[idx],
                Color.red,
                5f
);

            }
            
        }

        // Generiere Triangles
        int[] triangles = new int[lengthSegments * widthSegments * 6];
        int triIndex = 0;

        for (int l = 0; l < lengthSegments; l++)
        {
            for (int w = 0; w < widthSegments; w++)
            {
                int currentRow = l * (widthSegments + 1);
                int nextRow = (l + 1) * (widthSegments + 1);

                triangles[triIndex++] = currentRow + w;
                triangles[triIndex++] = nextRow + w;
                triangles[triIndex++] = currentRow + w + 1;

                triangles[triIndex++] = currentRow + w + 1;
                triangles[triIndex++] = nextRow + w;
                triangles[triIndex++] = nextRow + w + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }

    void UpdateWaterPlane(Vector3 farPoint)
    {
        if (waterPlane == null) return;

        float distance = Vector3.Distance(Vector3.zero, new Vector3(farPoint.x, 0, farPoint.z));
        waterPlane.transform.localScale = new Vector3(
            Mathf.Max(100, distance / 5f * 1.5f),
            1,
            Mathf.Max(100, distance / 5f * 1.5f)
        );

        waterPlane.transform.position = new Vector3(
            farPoint.x * 0.5f,
            waterHeight,
            farPoint.z * 0.5f
        );
    }
    
}