using System;
using UnityEditor;
using UnityEngine;

public class TaskGenerator : MonoBehaviour
{
    public static Task GetNewTask(Difficulty diff)
    {
        int one;
        int two;
        int difficulty = (int)diff;


        if (GameSettings.SelectedMapIndex == 0)
        {
            one = UnityEngine.Random.Range(1, 11 * difficulty + 10);
            two = UnityEngine.Random.Range(1, 11 * difficulty + 10);
            return new Task(TaskType.ADD, one, two, difficulty);
        }
        else if (GameSettings.SelectedMapIndex == 1)
        {
            one = UnityEngine.Random.Range(1, 11 * difficulty + 10);
            two = UnityEngine.Random.Range(1, 11 * difficulty + 10);

            if (one < two)
            {
                int temp = one;
                one = two;
                two = temp;
            }
            return new Task(TaskType.SUBTRACT, one, two, difficulty);
        }
        else if (GameSettings.SelectedMapIndex == 2)
        {
            one = UnityEngine.Random.Range(1, 11 + difficulty);
            two = UnityEngine.Random.Range(1, 11 + difficulty);
            return new Task(TaskType.MULTIPLY, one, two, difficulty);
        }
        else
        {
            one = UnityEngine.Random.Range(1, 11 * difficulty + 10);
            two = UnityEngine.Random.Range(1, 5 * difficulty + 5);
            while (one % two != 0 || one < two)
            {
                one++;
            }
            return new Task(TaskType.DIVIDE, one, two, difficulty);
        }
    }

    public static Task GetNewTask(TaskType taskType, PlayerProfile playerProfile)
    {
        return new Task(taskType, 1, 2, 1);
    }
}

