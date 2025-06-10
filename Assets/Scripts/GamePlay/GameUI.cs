using UnityEngine;
using TMPro;
using System.Collections;

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
    public Vector2 targetCorner = new Vector2(-50, 0);

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

        t = 0f;
        Vector2 startPos = popupRect.anchoredPosition;
        while (t < moveDuration)
        {
            t += Time.unscaledDeltaTime;
            popupRect.anchoredPosition = Vector2.Lerp(startPos, targetCorner, t / moveDuration);
            yield return null;
        }
        popupRect.anchoredPosition = targetCorner;
    }
}
