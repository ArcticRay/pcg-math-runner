using TMPro;
using UnityEngine;

public class TafelDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tafelText;

    public void SetText(string text)
    {
        tafelText.text = text;
    }

    public void SetNumber(int number)
    {
        tafelText.text = number.ToString();
    }
}
