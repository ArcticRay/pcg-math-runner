using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerProfile currentProfile;

    private string name;

    void Start()
    {

        currentProfile = new PlayerProfile("Maxi");

        ProfileManager.SaveProfile(currentProfile);

        // currentProfile = ProfileManager.LoadProfile(name);
    }
}
