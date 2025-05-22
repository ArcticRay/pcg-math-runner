using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class AnswerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tafelPrefab;

    public void SpawnAnswerTafel(Spline spline, float t, int answer)
    {
        float3 pos = spline.EvaluatePosition(t);
        float3 forward = spline.EvaluateTangent(t);
        float3 up = spline.EvaluateUpVector(t);
        Quaternion rot = Quaternion.LookRotation(forward, up);


        int randomizer = GetRandomBetweenMinus1And1();

        for (int i = -1; i <= 1; i++)
        {
            Vector3 offset = rot * new Vector3(20 * i, 15f, 0);
            Vector3 answerPos = (Vector3)pos + offset;

            GameObject answerTafel = Instantiate(tafelPrefab, answerPos, rot, transform);

            var display = answerTafel.GetComponent<TafelDisplay>();
            if (display != null)
            {
                if (i == randomizer)
                {
                    answerTafel.tag = "True";
                    display.SetText(answer.ToString());
                }
                else
                {
                    int newAnswer = UnityEngine.Random.Range(1, 20);
                    if (newAnswer == answer)
                    {
                        newAnswer++;
                    }
                    display.SetText(newAnswer.ToString());
                    answerTafel.tag = "False";
                }

            }

        }

    }

    private int GetRandomBetweenMinus1And1()
    {
        return UnityEngine.Random.Range(-1, 2);
    }
}
