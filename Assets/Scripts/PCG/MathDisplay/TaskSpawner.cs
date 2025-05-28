using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using TMPro;

public class TaskSpawner : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject tafelPrefab;
    [SerializeField] private int tafelAnzahl = 20;
    [SerializeField] private AnswerSpawner answerSpawner;



    private void Start()
    {
        if (GameSettings.SelectedDifficultyIndex == 0)
        {
            tafelAnzahl = 10;
        }
        else if (GameSettings.SelectedDifficultyIndex == 1)
        {
            tafelAnzahl = 10;
        }
        else
        {
            tafelAnzahl = 20;
        }
        SpawnTafeln();
    }

    private void SpawnTafeln()
    {
        if (tafelAnzahl <= 2)
        {
            return;
        }

        Spline spline = splineContainer.Spline;

        float answerOffset = (1f / (tafelAnzahl * 2));


        for (int i = 0; i < tafelAnzahl; i++)
        {
            float t = i / (float)(tafelAnzahl - 1);

            float3 pos = spline.EvaluatePosition(t);
            float3 forward = spline.EvaluateTangent(t);
            float3 up = spline.EvaluateUpVector(t);
            Quaternion rot = Quaternion.LookRotation(forward, up);

            Vector3 offset = rot * new Vector3(-20, 15f, 0);
            Vector3 newPos = (Vector3)pos + offset;

            GameObject tafel = Instantiate(tafelPrefab, newPos, rot, transform);
            TafelDisplay display = tafel.GetComponent<TafelDisplay>();

            Task task = TaskGenerator.GetNewTask();
            string taskText = $"{task.getDigitOne()} {task.getOperator()} {task.getDigitTwo()} = ?";
            display.SetText(taskText);

            if (answerSpawner != null)
            {
                answerSpawner.SpawnAnswerTafel(spline, (t + answerOffset), task.getResult());
            }
        }
    }
}
