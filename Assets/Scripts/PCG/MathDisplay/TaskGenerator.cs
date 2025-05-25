using System;
using UnityEditor;
using UnityEngine;

public class TaskGenerator : MonoBehaviour
{
    public static Task GetNewTask()
    {
        int one = UnityEngine.Random.Range(1, 11);
        int two = UnityEngine.Random.Range(1, 11);
        return new Task(TaskType.ADD, one, two);
    }

    public static Task GetNewTask(TaskType taskType, PlayerProfile playerProfile)
    {
        return new Task(TaskType.ADD, 1, 2);
    }

}

