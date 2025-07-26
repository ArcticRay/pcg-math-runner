using UnityEngine;
using UnityEngine.UI;  // für Button
using UnityEngine.SceneManagement;  // für EventSystem

public class KeyboardButtonHandler : MonoBehaviour
{
    [SerializeField] private Button confirmButton; // z.B. Start‑Game
    [SerializeField] private Button cancelButton;  // z.B. Zurück‑Button

    void Update()
    {
        // Enter / Return → Bestätigen
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (confirmButton != null && confirmButton.interactable)
                confirmButton.onClick.Invoke();
        }

        // Escape → Abbrechen / Zurück
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
