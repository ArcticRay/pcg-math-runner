using UnityEngine;

public class TaskSpawner : MonoBehaviour
{

    [SerializeField] private GameObject tafelPrefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject tafelInstance = Instantiate(tafelPrefab, new Vector3(0, 10, 10), Quaternion.identity);
        TafelDisplay display = tafelInstance.GetComponent<TafelDisplay>();
        display.SetText("7 + 4 = ?"); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
