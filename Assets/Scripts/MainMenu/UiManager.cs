using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    public void ToMainMenu()
    {
        Debug.Log("Zurück zum Hauptmenü");
        SceneManager.LoadScene("MainMenu");
    }
}
