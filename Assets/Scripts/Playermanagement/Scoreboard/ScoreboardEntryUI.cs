using UnityEngine;
using TMPro;

public class ScoreboardEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textPlayerName;
    [SerializeField] private TMP_Text textMMR;
    [SerializeField] private TMP_Text textRank;

    public void Initialize(string playerName, int mmr, string rank)
    {
        textPlayerName.text = playerName;
        textMMR.text = mmr.ToString();
        textRank.text = rank;
    }
}
