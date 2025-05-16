using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using TMPro; 

public class TaskSpawner : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject tafelPrefab;
    [SerializeField] private int tafelnAnzahl = 5;


    private void Start()
    {
        SpawnTafeln();
    }

    private void SpawnTafeln()
    {
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < tafelnAnzahl; i++)
        {
            float t = i / (float)(tafelnAnzahl - 1);

            // Hole Position, Tangente und Up-Vektor
            float3 pos = spline.EvaluatePosition(t);
            float3 forward = spline.EvaluateTangent(t);
            float3 up = spline.EvaluateUpVector(t);
            Quaternion rot = Quaternion.LookRotation(forward, up);
            

            Vector3 offset = rot * new Vector3(-20, 15f, 0); // z. B. 1.5 Einheiten nach oben
            Vector3 newPos = (Vector3 )pos + offset;

            GameObject tafel = Instantiate(tafelPrefab, newPos, rot, transform);
            TafelDisplay display = tafel.GetComponent<TafelDisplay>();

            Task taskone = TaskGenerator.getNewTask();

            string text = $" {taskone.getDigitOne() + " " + taskone.getOperator() + " " + taskone.getDigitTwo() + " = ?" }";

            display.SetText(text); 



        }
    }
}
