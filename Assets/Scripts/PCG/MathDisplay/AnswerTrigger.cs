using UnityEngine;

public class AnswerTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (this.CompareTag("True"))
            {
                Debug.Log("Spieler hat richtig geantwortet");
            }
            else
            {
                Debug.Log("Spieler hat falsch geantwortet");
            }

        }


    }

}
