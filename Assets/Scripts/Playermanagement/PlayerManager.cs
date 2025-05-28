using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    public void CreateNewPlayerProfile(string playerName)
    {

        PlayerProfile playerProfile = new PlayerProfile();
        playerProfile.SetPlayerName(playerName);
    }
}
