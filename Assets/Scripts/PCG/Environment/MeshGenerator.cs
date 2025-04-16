using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class MeshGenerator : MonoBehaviour
{

    [SerializeField]
    SplineContainer splineContainer;
    [SerializeField]
    private int splineIndex;
    [SerializeField]
    [Range(0f, 1f)]
    private float time;
    [SerializeField]
    private int width = 6;


    float3 position;
    float3 forward; // Tangent
    float3 upVector;

    float3 p1;
    float3 p2;

    public Mesh mesh;
    MeshFilter meshFilter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        
    }

    // Update is called once per frame
    void Update()
    {
        splineContainer.Evaluate(splineIndex, 0, out position, out forward, out upVector);
        float3 right = Vector3.Cross(forward, upVector).normalized;
        p1 = position + (right * width);
        p2 = position + (-right * width);

        GenerateMesh(p1, p2, p1 +1 , p2+ 1);

    }

    void GetPoints()
    {
        
    }

    void GenerateMesh(float3 p1, float3 p2, float3 p3, float3 p4)
    {
        Vector3[] vertices = new Vector3[]
        {
            p1,
            p2,
            p3,
            p4
        };

        int[] triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };

        // UVs für Texturierung (optional)
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals(); // für Beleuchtung wichtig

        // Mesh dem MeshFilter zuweisen
        meshFilter.mesh = mesh;
        
    }
}
