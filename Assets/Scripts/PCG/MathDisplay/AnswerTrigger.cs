using UnityEngine;

public class AnswerTrigger : MonoBehaviour
{
    public GameManager gameManager;

    void Start()
    {
        gameManager = (GameManager)FindFirstObjectByType(typeof(GameManager));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (this.CompareTag("True"))
            {
                Debug.Log("Spieler hat richtig geantwortet");
                gameManager.TaskCompleted();

            }
            else
            {
                Debug.Log("Spieler hat falsch geantwortet");
            }

        }


    }

}
