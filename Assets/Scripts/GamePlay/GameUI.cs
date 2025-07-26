using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("UI-Container")]
    [SerializeField] private CanvasGroup uiGroup;

    [Header("Popup-Container")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private RectTransform popupPanelRect;

    [Header("Popup-Text")]
    [SerializeField] private TextMeshProUGUI popupText;

    [Header("Animationseinstellungen")]
    [SerializeField] private float popDuration = 0.5f;
    [SerializeField] private float moveDuration = 0.8f;

    [Header("Padding für automatische Größe")]
    [SerializeField] private Vector2 padding = new Vector2(20, 10);

    private RectTransform canvasRect;
    private float bottomMargin = 20f;

    private void Awake()
    {
        var rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
            Debug.LogError("GameUI: Kein Canvas in Parent-Hierarchie gefunden!");
        canvasRect = rootCanvas.GetComponent<RectTransform>();
    }

    private void Start()
    {
        SetVisible(true);
        popupPanel.SetActive(false);
    }

    public void SetVisible(bool visible)
    {
        uiGroup.alpha = visible ? 1f : 0f;
        uiGroup.interactable = visible;
        uiGroup.blocksRaycasts = visible;
    }

    public void ShowPopup(string newText)
    {
        StopAllCoroutines();


        popupPanel.SetActive(true);
        popupText.text = newText;


        LayoutRebuilder.ForceRebuildLayoutImmediate(popupPanelRect);

        // Panel-Größe an Text anpassen (+ Padding)
        float w = popupText.preferredWidth + padding.x * 2;
        float h = popupText.preferredHeight + padding.y * 2;
        popupPanelRect.sizeDelta = new Vector2(w, h);


        popupPanelRect.anchorMin = popupPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupPanelRect.anchoredPosition = Vector2.zero;
        popupPanelRect.localScale = Vector3.zero;

        StartCoroutine(PopupAndSlide());
    }

    public void HidePopup()
    {
        StopAllCoroutines();
        popupPanel.SetActive(false);
    }

    private IEnumerator PopupAndSlide()
    {

        float t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / popDuration);
            popupPanelRect.localScale = Vector3.one * s;
            yield return null;
        }
        popupPanelRect.localScale = Vector3.one;


        yield return new WaitForSecondsRealtime(0.5f);

        float halfCanvasH = canvasRect.rect.height * 0.5f;
        float halfPopupH = popupPanelRect.sizeDelta.y * 0.5f;
        float targetY = -halfCanvasH + halfPopupH + bottomMargin;

        Vector2 startPos = popupPanelRect.anchoredPosition;
        Vector2 endPos = new Vector2(0f, targetY);

        t = 0f;
        while (t < moveDuration)
        {
            t += Time.unscaledDeltaTime;
            popupPanelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t / moveDuration);
            yield return null;
        }
        popupPanelRect.anchoredPosition = endPos;
    }
}
