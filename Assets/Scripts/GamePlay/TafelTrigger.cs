using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TafelTrigger : MonoBehaviour
{
    public TMP_Text TaskText;

    public PauseManager pauseManager;

    private GameUI gameUI;

    void Awake()
    {
        gameUI = FindFirstObjectByType<GameUI>();
        pauseManager = FindFirstObjectByType<PauseManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (this.CompareTag("Finish"))
        {
            Debug.Log("Spieler hat das Ziel erreicht");
            pauseManager.ToMainMenu();
            // SceneManager.LoadScene("MainMenu");
        }
        if (other.CompareTag("Player"))
        {
            gameUI.ShowPopup(TaskText.text);
            Debug.Log("Spieler hat die Tafel durchquert!");
        }

    }


}