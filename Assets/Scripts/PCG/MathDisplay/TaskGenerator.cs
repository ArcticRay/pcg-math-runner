using System;
using UnityEngine;

public class TaskGenerator : MonoBehaviour
{
    public static Task getNewTask()
    {
        int one = UnityEngine.Random.Range(1, 11);
        int two = UnityEngine.Random.Range(1, 11);
        return new Task(TaskType.ADD, one, two);
    }

}

