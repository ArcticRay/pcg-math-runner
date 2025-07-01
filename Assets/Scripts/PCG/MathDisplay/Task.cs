using Unity.VisualScripting;
using UnityEngine;

public class Task
{
    TaskType type;
    int digit1;
    int digit2;

    int difficulty;
    public Task(TaskType taskType, int digit1, int digit2, int difficulty)
    {
        this.digit1 = digit1;
        this.digit2 = digit2;
        this.type = taskType;
    }

    public int getDigitOne()
    {
        return digit1;
    }

    public int getDigitTwo()
    {
        return digit2;
    }

    public TaskType GetTaskType()
    {
        return type;
    }

    public string getOperator()
    {
        string op = "Hello";

        if (type == TaskType.ADD)
            op = "+";
        else if (type == TaskType.SUBTRACT)
            op = "-";
        else if (type == TaskType.MULTIPLY)
            op = "x";
        else if (type == TaskType.DIVIDE)
            op = "/";

        return op;
    }

    public int getResult()
    {
        int result = 0;

        switch (type)
        {
            case TaskType.ADD:
                result = digit1 + digit2;
                break;

            case TaskType.SUBTRACT:
                result = digit1 - digit2;
                break;

            case TaskType.MULTIPLY:
                result = digit1 * digit2;
                break;

            case TaskType.DIVIDE:
                if (digit1 != 0 && digit2 != 0)
                    result = digit1 / digit2;
                else
                    Debug.LogWarning("Division durch 0 ist nicht erlaubt.");
                break;

            default:
                Debug.LogWarning("Unknown TaskType");
                break;
        }

        return result;
    }
}
