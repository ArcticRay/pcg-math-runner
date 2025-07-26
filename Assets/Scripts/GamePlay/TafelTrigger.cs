using TMPro;
using UnityEngine;


public class TafelTrigger : MonoBehaviour
{
    public TMP_Text TaskText;

    public PauseManager pauseManager;

    private GameUI gameUI;

    private GameManager gameManager;

    private TimerDisplay timerDisplay;

    void Awake()
    {
        gameUI = FindFirstObjectByType<GameUI>();
        pauseManager = FindFirstObjectByType<PauseManager>();
        timerDisplay = FindFirstObjectByType<TimerDisplay>();
        gameManager = FindFirstObjectByType<GameManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (this.CompareTag("Finish"))
        {
            Debug.Log("Spieler hat das Ziel erreicht");
            gameManager.EndRun();
            timerDisplay.StopTimer();
            pauseManager.ToScoreboard();
            // SceneManager.LoadScene("MainMenu");
        }
        else if (other.CompareTag("Player"))
        {
            gameUI.ShowPopup(TaskText.text);
            Debug.Log("Spieler hat die Tafel durchquert!");
        }

    }


}