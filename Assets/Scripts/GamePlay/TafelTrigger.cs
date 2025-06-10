using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TafelTrigger : MonoBehaviour
{
    public TMP_Text TaskText;

    private GameUI gameUI;

    void Awake()
    {
        gameUI = FindFirstObjectByType<GameUI>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (this.CompareTag("Finish"))
        {
            Debug.Log("Spieler hat das Ziel erreicht");
            SceneManager.LoadScene("MainMenu");
        }
        if (other.CompareTag("Player"))
        {
            gameUI.ShowPopup(TaskText.text);
            Debug.Log("Spieler hat die Tafel durchquert!");
        }

    }


}