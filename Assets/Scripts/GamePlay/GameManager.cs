using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerProfile currentProfile;
    private int sessionXP = 0;      // temp var

    private int sessionXPGain = 0;

    void Start()
    {

        currentProfile = ProfileManager.LoadOrCreateProfile(GameSettings.PlayerName);

        ProfileManager.SaveProfile(currentProfile);

        Debug.Log("Name: " + currentProfile.GetPlayerName());
        Debug.Log("Difficulty: " + GameSettings.SelectedDifficultyIndex);
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
        }
        else if (GameSettings.SelectedDifficultyIndex == 1)
        {
            IncreaseXP(15);
        }
        else
        {
            IncreaseXP(20);
        }
    }


    // save if closed
    void OnApplicationQuit()
    {
        EndRun();
    }
}
