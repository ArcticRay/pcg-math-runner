using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerProfile currentProfile;
    private int sessionXP = 0;
    [SerializeField]
    TaskSpawner taskSpawner;

    private Difficulty rank;

    void Start()
    {

        currentProfile = ProfileManager.LoadOrCreateProfile(GameSettings.PlayerName);
        this.rank = currentProfile.GetRank();

        ProfileManager.SaveProfile(currentProfile);

        Debug.Log("Name: " + currentProfile.GetPlayerName());
        Debug.Log("Difficulty: " + GameSettings.SelectedDifficultyIndex);
        Debug.Log("Rank: " + rank);


    }

    public void GenerateTask()
    {
        taskSpawner.StartTafelSpawn(rank);
    }

    public void IncreaseXP(int xpGained)
    {
        sessionXP += xpGained;
        Debug.Log($"+{xpGained} XP (Session total: {sessionXP})");
    }

    public void EndRun()
    {
        if (sessionXP <= 0) return;

        currentProfile.IncreasePlayerXP(sessionXP);

        ProfileManager.SaveProfile(currentProfile);


        Debug.Log($"Run beendet: {sessionXP} XP hinzugefügt. Neuer Stand: Lvl {currentProfile.GetPlayerLevel()}, XP {currentProfile.GetXP()}");

        sessionXP = 0;
    }

    public void TaskCompleted()
    {
        if (GameSettings.SelectedDifficultyIndex == 0)
        {
            IncreaseXP(10);
            UpdateMMR(1);
        }
        else if (GameSettings.SelectedDifficultyIndex == 1)
        {
            IncreaseXP(15);
            UpdateMMR(2);
        }
        else
        {
            IncreaseXP(20);
            UpdateMMR(4);
        }
    }

    public void TaskUncompleted()
    {
        UpdateMMR(-2);
    }

    public void UpdateMMR(int change)
    {
        currentProfile.UpdateMMR(change);
    }


    // save if closed
    void OnApplicationQuit()
    {
        EndRun();
    }
}
