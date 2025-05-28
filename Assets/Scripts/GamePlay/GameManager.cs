using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerProfile currentProfile;
    private int sessionXP = 0;      // temp var

    void Start()
    {

        currentProfile = ProfileManager.LoadOrCreateProfile(GameSettings.PlayerName);

        ProfileManager.SaveProfile(currentProfile);

        Debug.Log("Name: " + currentProfile.GetPlayerName());
    }

    public void OnTaskCompleted(int xpGained)
    {
        sessionXP += xpGained;       // ② XP in der Session sammeln
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

    // save if closed
    void OnApplicationQuit()
    {
        EndRun();
    }
}
