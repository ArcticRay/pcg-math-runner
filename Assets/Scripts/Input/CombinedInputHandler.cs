using UnityEngine;

public class CombinedInputHandler : MonoBehaviour
{
    bool usingBoard;
    void Start()
    {
        var names = Input.GetJoystickNames();
        for (int i = 0; i < names.Length; i++)
        {
            Debug.Log($"Joystick {i + 1}: '{names[i]}'");
        }

        foreach (var name in Input.GetJoystickNames())
        {
            if (!string.IsNullOrEmpty(name) && name.Contains("RVL-WBC-01") || name.Contains("Nintendo RVL-CNT-01"))
            {
                usingBoard = true;
                Debug.Log("Wii Balance Board used");
                break;
            }
        }
        if (!usingBoard) Debug.Log("No Balance Board found. Using Keyboard instead");
    }

    void Update()
    {

    }

    void HandleBalanceBoardInput()
    {
        // TODO
    }
}
