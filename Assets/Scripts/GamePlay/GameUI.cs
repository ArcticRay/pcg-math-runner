using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("UI-Container")]
    public CanvasGroup uiGroup;

    [Header("Popup-Text")]
    public TextMeshProUGUI popupText;
    public RectTransform popupRect;

    [Header("Animationseinstellungen")]
    public float popDuration = 0.5f;
    public float moveDuration = 0.8f;

    [Header("Padding für automatische Größe")]
    public Vector2 padding = new Vector2(20, 10);

    private RectTransform canvasRect;
    private float bottomMargin = 20f;

    void Awake()
    {
        var rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
            Debug.LogError("GameUI: Kein Canvas in Parent-Hierarchie gefunden!");
        canvasRect = rootCanvas.GetComponent<RectTransform>();
    }

    void Start()
    {
        SetVisible(true);
        popupText.gameObject.SetActive(false);
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
        popupText.text = newText;

        LayoutRebuilder.ForceRebuildLayoutImmediate(popupText.rectTransform);

        float w = popupText.preferredWidth + padding.x * 2;
        float h = popupText.preferredHeight + padding.y * 2;
        popupRect.sizeDelta = new Vector2(w, h);

        popupRect.anchorMin = popupRect.anchorMax = new Vector2(0.5f, 0.5f);

        StartCoroutine(PopupAndSlide());
    }

    public void HidePopup()
    {
        StopAllCoroutines();
        popupText.gameObject.SetActive(false);
    }

    private IEnumerator PopupAndSlide()
    {
        popupText.gameObject.SetActive(true);

        popupRect.anchoredPosition = Vector2.zero;
        popupRect.localScale = Vector3.zero;

        // Pop-In Animation
        float t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / popDuration);
            popupRect.localScale = Vector3.one * s;
            yield return null;
        }
        popupRect.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(0.5f);

        // Berechne Ziel-Position: ganz unten mit Margin
        float halfCanvasH = canvasRect.rect.height * 0.5f;
        float halfPopupH = popupRect.sizeDelta.y * 0.5f;
        float targetY = -halfCanvasH + halfPopupH + bottomMargin;
        Vector2 startPos = popupRect.anchoredPosition;
        Vector2 endPos = new Vector2(0, targetY);

        // Slide-Animation
        t = 0f;
        while (t < moveDuration)
        {
            t += Time.unscaledDeltaTime;
            popupRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t / moveDuration);
            yield return null;
        }
        popupRect.anchoredPosition = endPos;
    }
}
