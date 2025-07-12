using UnityEngine;

public class AnswerTrigger : MonoBehaviour
{
    [Header("Game Manager")]
    public GameManager gameManager;

    [Header("Audio Settings")]
    [Tooltip("AudioSource, über die die Sounds abgespielt werden.")]
    public AudioSource audioSource;
    [Tooltip("Sound für eine richtige Antwort.")]
    public AudioClip correctClip;
    [Tooltip("Sound für eine falsche Antwort.")]
    public AudioClip wrongClip;

    private GameUI gameUI;

    public SplineManagerNew splineManagerNew;
    public float tPosition = 0;

    void Awake()
    {
        gameUI = FindFirstObjectByType<GameUI>();
    }

    void Start()
    {
        splineManagerNew = (SplineManagerNew)FindFirstObjectByType(typeof(SplineManagerNew));
        gameManager = (GameManager)FindFirstObjectByType(typeof(GameManager));
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                Debug.LogWarning("Keine AudioSource gesetzt und keine am selben GameObject gefunden!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        gameUI.HidePopup();
        if (!other.CompareTag("Player"))
            return;

        bool isCorrect = this.CompareTag("True");

        if (isCorrect)
        {
            Debug.Log("Spieler hat richtig geantwortet");
            gameManager.TaskCompleted();
            // Play correct sound
            if (correctClip != null && audioSource != null)
                audioSource.PlayOneShot(correctClip);
        }
        else
        {
            Debug.Log("Spieler hat falsch geantwortet");
            splineManagerNew.SpawnObstacles(tPosition, 1f);
            // Play wrong sound
            if (wrongClip != null && audioSource != null)
                audioSource.PlayOneShot(wrongClip);
        }
    }

    public void SetTPosition(float t)
    {
        tPosition = t;
    }


}
