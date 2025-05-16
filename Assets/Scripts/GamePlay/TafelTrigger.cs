using UnityEngine;

public class TafelTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Spieler hat die Tafel durchquert!");
        }
    }
}