using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsMenuController : MonoBehaviour
{
    // Panels
    public GameObject mainMenuPanel;
    public GameObject namePanel;
    public GameObject mapPanel;
    public GameObject difficultyPanel;

    // UI
    public TMP_InputField playerNameInput;
    public TMP_Dropdown mapDropdown;
    public TMP_Dropdown difficultyDropdown;

    void Start()
    {
        ShowPanel(mainMenuPanel);
    }

    void ShowPanel(GameObject panel)
    {
        mainMenuPanel.SetActive(false);
        namePanel.SetActive(false);
        mapPanel.SetActive(false);
        difficultyPanel.SetActive(false);

        panel.SetActive(true);
    }


    public void OnPlayPressed()
    {
        ShowPanel(namePanel);
    }

    public void OnNameNext()
    {
        GameSettings.PlayerName = playerNameInput.text;
        ShowPanel(mapPanel);
    }


    public void OnMapNext()
    {
        GameSettings.SelectedMapIndex = mapDropdown.value;
        ShowPanel(difficultyPanel);
    }

    public void OnStartGame()
    {
        GameSettings.SelectedDifficultyIndex = difficultyDropdown.value;
        SceneManager.LoadScene("IslandScene");
    }
}
