using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TafelTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (this.CompareTag("Finish"))
        {
            Debug.Log("Spieler hat das Ziel erreicht");
            SceneManager.LoadScene("MainMenu");
        }
        if (other.CompareTag("Player"))
        {
            Debug.Log("Spieler hat die Tafel durchquert!");
        }

    }


}