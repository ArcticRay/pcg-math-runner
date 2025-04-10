using System.Collections.Generic;
using UnityEngine;

public class SplineGenerator : MonoBehaviour
{
    // Länge eines Chunks in Welt-Einheiten (Basislänge)
    public float chunkLength = 10f;
    // Anzahl der initial zu generierenden Chunks
    public int initialChunks = 3;
    // Anzahl der Punkte pro Chunk
    public int pointsPerChunk = 5;
    // Maximale seitliche Variation (für den zentralen Spline)
    public float lateralVariation = 2f;
    // Multiplikator, um die Abstände der Kontrollpunkte zu vergrößern
    public float controlPointDistanceMultiplier = 1.5f;
    // Breite, um die die linken/rechten Spuren vom zentralen Spline versetzt werden sollen
    public float laneOffset = 3f;
    // Sollen Catmull-Rom-Splines zur Interpolation genutzt werden (sonst lineare Interpolation)
    public bool useCatmullRom = true;
    // Anzahl der Unterteilungen pro Segment bei Catmull-Rom
    public int curveSegments = 20;
    
    // Liste, die alle Kontrollpunkte des zentralen Splines speichert
    private List<Vector3> controlPoints;

    void Start()
    {
        controlPoints = new List<Vector3>();
        // Startpunkt des Splines (Position des GameObjects)
        controlPoints.Add(transform.position);

        // Generiere die anfänglichen Chunks
        for (int i = 0; i < initialChunks; i++)
        {
            AppendChunk();
        }
    }

    // Fügt einen neuen Chunk an den existierenden Spline an.
    public void AppendChunk()
    {
        Vector3 lastPoint = controlPoints[controlPoints.Count - 1];

        // Erzeuge pro Chunk 'pointsPerChunk' neue Kontrollpunkte
        for (int i = 1; i <= pointsPerChunk; i++)
        {
            float t = (float)i / pointsPerChunk;
            // Hier wird der Basisabstand (chunkLength) mit dem Multiplikator skaliert,
            // sodass die Punkte weiter auseinander liegen und längere, sanftere Kurven ermöglichen.
            Vector3 newPoint = lastPoint + transform.forward * (chunkLength * t * controlPointDistanceMultiplier);
            // Füge eine zufällige seitliche Abweichung hinzu (für Variation)
            newPoint.x += Random.Range(-lateralVariation, lateralVariation);
            controlPoints.Add(newPoint);
        }
    }

    // Visualisierung der Splines via Gizmos
    void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Count < 2)
            return;

        // Zeichne den zentralen Spline in Weiß
        Gizmos.color = Color.white;
        if (useCatmullRom)
            DrawCatmullRomSpline(controlPoints);
        else
            DrawLinearSpline(controlPoints);

        // Erzeuge linken Klon: Offset um -transform.right * laneOffset
        List<Vector3> leftSpline = new List<Vector3>();
        foreach (Vector3 point in controlPoints)
            leftSpline.Add(point - transform.right * laneOffset);

        Gizmos.color = Color.blue;
        if (useCatmullRom)
            DrawCatmullRomSpline(leftSpline);
        else
            DrawLinearSpline(leftSpline);

        // Erzeuge rechten Klon: Offset um +transform.right * laneOffset
        List<Vector3> rightSpline = new List<Vector3>();
        foreach (Vector3 point in controlPoints)
            rightSpline.Add(point + transform.right * laneOffset);

        Gizmos.color = Color.red;
        if (useCatmullRom)
            DrawCatmullRomSpline(rightSpline);
        else
            DrawLinearSpline(rightSpline);
    }

    // Zeichnet den Spline mit linearer Interpolation (als Vergleich)
    void DrawLinearSpline(List<Vector3> points)
    {
        int interpolationSteps = 10;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];
            for (int j = 0; j < interpolationSteps; j++)
            {
                float t0 = (float)j / interpolationSteps;
                float t1 = (float)(j + 1) / interpolationSteps;
                Vector3 pStart = Vector3.Lerp(start, end, t0);
                Vector3 pEnd = Vector3.Lerp(start, end, t1);
                Gizmos.DrawLine(pStart, pEnd);
            }
        }
    }

    // Zeichnet den Spline mithilfe von Catmull-Rom-Splines
    void DrawCatmullRomSpline(List<Vector3> points)
    {
        // Dupliziere die Endpunkte für saubere Übergänge
        List<Vector3> pts = new List<Vector3>(points);
        if (pts.Count < 2)
            return;
        pts.Insert(0, pts[0]);
        pts.Add(pts[pts.Count - 1]);

        // Iteriere über die Punktgruppen
        for (int i = 0; i < pts.Count - 3; i++)
        {
            Vector3 p0 = pts[i];
            Vector3 p1 = pts[i + 1];
            Vector3 p2 = pts[i + 2];
            Vector3 p3 = pts[i + 3];

            // Unterteile das Segment in 'curveSegments' Unterabschnitte
            for (int j = 0; j < curveSegments; j++)
            {
                float t0 = j / (float)curveSegments;
                float t1 = (j + 1) / (float)curveSegments;
                Vector3 pos0 = GetCatmullRomPosition(t0, p0, p1, p2, p3);
                Vector3 pos1 = GetCatmullRomPosition(t1, p0, p1, p2, p3);
                Gizmos.DrawLine(pos0, pos1);
            }
        }
    }

    // Berechnet die Position auf einem Catmull-Rom-Spline für einen gegebenen Parameter t (0 <= t <= 1)
    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * ((2f * p1) +
                      (-p0 + p2) * t +
                      (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                      (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    // Zum Testen: Mit der Leertaste wird ein weiterer Chunk angehängt
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AppendChunk();
        }
    }
}
