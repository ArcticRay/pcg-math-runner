using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class AnswerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tafelPrefab;

    private static readonly System.Random _rnd = new System.Random();

    public void SpawnAnswerTafel(Spline spline, float t, int answer, int one, int two, TaskType taskType)
    {
        float3 pos = spline.EvaluatePosition(t);
        float3 forward = spline.EvaluateTangent(t);
        float3 up = spline.EvaluateUpVector(t);
        Quaternion rot = Quaternion.LookRotation(forward, up);


        int randomizer = GetRandomBetweenMinus1And1();
        int oldAnswer = 0;

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
                    int newAnswer = GenerateOne(one, two, taskType);
                    if (newAnswer == oldAnswer)
                    {
                        newAnswer++;
                    }
                    if (newAnswer == answer)
                    {
                        newAnswer += 1;
                    }

                    display.SetText(newAnswer.ToString());
                    answerTafel.tag = "False";
                    oldAnswer = newAnswer;
                }

            }

        }

    }

    public static int GenerateOne(int a, int b, TaskType taskType)
    {
        int correct = Calculate(a, b, taskType);
        var candidates = new HashSet<int>();

        switch (taskType)
        {
            case TaskType.ADD:
                candidates.Add(a);
                candidates.Add(b);
                candidates.Add(correct + (_rnd.Next(0, 2) == 0 ? 1 : -1));
                break;

            case TaskType.SUBTRACT:
                candidates.Add(b - a);
                candidates.Add(a - b);
                candidates.Add(correct + (_rnd.Next(0, 2) == 0 ? 1 : -1));
                break;

            case TaskType.MULTIPLY:
                candidates.Add(a * (b + 1));
                candidates.Add((a + 1) * b);
                candidates.Add(a * b + a);
                candidates.Add(a * b + UnityEngine.Random.Range(1, 11));
                break;

            case TaskType.DIVIDE:
                if (b != 0)
                {

                    candidates.Add(a / Math.Max(1, b) + 1);
                    candidates.Add(a / Math.Max(1, b) - 1);

                    candidates.Add(b != 0 ? b / Math.Max(1, a) : 0);
                }
                break;
        }

        // Filtere das korrekte Ergebnis und negative Werte raus
        candidates.Remove(correct);
        candidates.RemoveWhere(x => x < 0);

        // Falls kein Kandidat mehr übrig ist, generiere einen Offset-Fauxpas
        if (!candidates.Any())
        {
            int offset;
            do
            {
                offset = _rnd.Next(-5, 6);
            } while (offset == 0);
            return correct + offset;
        }

        // Wähle zufällig einen verbleibenden Wert aus
        return candidates.ElementAt(_rnd.Next(candidates.Count));
    }

    private static int Calculate(int a, int b, TaskType t) =>
        t switch
        {
            TaskType.ADD => a + b,
            TaskType.SUBTRACT => a - b,
            TaskType.MULTIPLY => a * b,
            TaskType.DIVIDE => a / b,
            _ => throw new ArgumentException("Unbekannter TaskType")
        };

    private int GetRandomBetweenMinus1And1()
    {
        return UnityEngine.Random.Range(-1, 2);
    }
}
