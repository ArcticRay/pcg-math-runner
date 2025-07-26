using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class KeyboardButtonHandler : MonoBehaviour
{
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (confirmButton != null && confirmButton.interactable)
                confirmButton.onClick.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cancelButton != null && cancelButton.interactable)
            {
                cancelButton.onClick.Invoke();
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }

        }
    }
}
