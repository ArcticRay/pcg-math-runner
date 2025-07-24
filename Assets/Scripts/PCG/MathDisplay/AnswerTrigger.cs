using System.Net;
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

    public GameObject correctFracturedPrefab;
    public GameObject falseFracturedPrefab;


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
            splineManagerNew.SpawnObstacles(tPosition, tPosition + 0.1f);
            // Play wrong sound
            if (wrongClip != null && audioSource != null)
                audioSource.PlayOneShot(wrongClip);
        }
        BreakApart(isCorrect);

    }

    public void SetTPosition(float t)
    {
        tPosition = t;
    }

    public void BreakApart(bool correct)
    {
        GameObject fracturedInstance;
        if (correct)
        {
            fracturedInstance = Instantiate(correctFracturedPrefab, transform.position, transform.rotation);
        }
        else
        {
            fracturedInstance = Instantiate(falseFracturedPrefab, transform.position, transform.rotation);
        }

        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;

        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.Play();
            // Destroy(gameObject, audio.clip.length); // nach Sound zerstören
        }
        else
        {
            // Destroy(gameObject, 2f); // Fallback, falls kein Audio vorhanden
        }

        // Destroy(fracturedInstance, 3f);
        // Ersetzt durch Partikelsystem
    }



}
