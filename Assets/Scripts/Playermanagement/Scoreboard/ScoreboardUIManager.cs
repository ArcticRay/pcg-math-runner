using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreboardUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform entryContainer;      // Content/EntryContainer
    [SerializeField] private GameObject entryPrefab;        // ScoreboardEntry Prefab

    private void Start()
    {
        PopulateScoreboard();
    }

    private void PopulateScoreboard()
    {
        foreach (Transform child in entryContainer)
            Destroy(child.gameObject);

        // load data
        var data = ScoreboardManager.LoadScoreboard();


        foreach (var e in data.entries)
        {
            var go = Instantiate(entryPrefab, entryContainer);
            var ui = go.GetComponent<ScoreboardEntryUI>();
            ui.Initialize(e.playerName, e.mmr, e.rank.ToString());
        }
    }
}
