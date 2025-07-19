using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Tooltip("Referenz auf das Pause-Panel im Canvas")]
    public GameObject pausePanel;
    public GameUI gameUI;

    private bool isPaused = false;

    private void Start()
    {

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0f : 1f;

        pausePanel.SetActive(isPaused);

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        gameUI.SetVisible(!isPaused);
    }

    public void ResumeGame()
    {
        if (isPaused) TogglePause();
    }

    public void QuitGame()
    {
        Debug.Log("Spiel wird beendet");
        Application.Quit();
    }

    public void ToMainMenu()
    {
        Debug.Log("Zurück zum Hauptmenü");
        TogglePause();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene("MainMenu");
    }

    public void ToScoreboard()
    {
        SceneManager.LoadScene("Scoreboard");
    }
}
