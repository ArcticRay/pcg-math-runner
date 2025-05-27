using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerProfile currentProfile;

    private string playerName;

    void Start()
    {

        currentProfile = new PlayerProfile("Maxi");

        ProfileManager.SaveProfile(currentProfile);

        // currentProfile = ProfileManager.LoadProfile(playerName);
    }
}
