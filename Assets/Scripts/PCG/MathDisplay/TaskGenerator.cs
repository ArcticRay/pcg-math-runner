using System;
using UnityEngine;

public class TaskGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static Task getNewTask()
    {
        int one = UnityEngine.Random.Range(1, 11);
        int two = UnityEngine.Random.Range(1, 11);
        return new Task(TaskType.ADD, one, two);
    }

}

