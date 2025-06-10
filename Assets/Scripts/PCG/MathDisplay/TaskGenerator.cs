using System;
using UnityEditor;
using UnityEngine;

public class TaskGenerator : MonoBehaviour
{
    public static Task GetNewTask()
    {
        int one = UnityEngine.Random.Range(1, 11);
        int two = UnityEngine.Random.Range(1, 11);

        if (GameSettings.SelectedMapIndex == 0)
        {
            return new Task(TaskType.ADD, one, two);
        }
        else if (GameSettings.SelectedMapIndex == 1)
        {
            if (one < two)
            {
                int temp = one;
                one = two;
                two = temp;
            }
            return new Task(TaskType.SUBTRACT, one, two);
        }
        else if (GameSettings.SelectedMapIndex == 2)
        {
            return new Task(TaskType.MULTIPLY, one, two);
        }
        else
        {
            one = UnityEngine.Random.Range(1, 30);
            while (one % two != 0 || one < two)
            {
                one++;
            }
            return new Task(TaskType.DIVIDE, one, two);
        }



    }

    public static Task GetNewTask(TaskType taskType, PlayerProfile playerProfile)
    {
        return new Task(TaskType.ADD, 1, 2);
    }

}

