using UnityEngine;
using System.Collections.Generic;
using Fusion;
using TMPro;

public class ReadyUpLobbyController : NetworkBehaviour
{
    [SerializeField] public NetworkManager NetworkManager;

    [SerializeField] public List<PlayerRef> playerRefs = new List<PlayerRef>();
    [SerializeField] public TextMeshProUGUI player1;
    [SerializeField] public TextMeshProUGUI player2;

    public void Awake()
    {
        NetworkManager = NetworkManager.Instance;
    }

    public override void Render()
    {
        UpdateNameVisuals();
    }
    public void UpdateNameVisuals()
    {
        playerRefs.Clear(); 
        playerRefs.AddRange(NetworkManager.playerList);

        player1.text = "";
        player2.text = "";

        for (int i = 0; i < playerRefs.Count && i < 2; i++)
        {
            if (i == 0)
            {
                player1.text = playerRefs[i].ToString();
            }
            else if (i == 1)
            {
                player2.text = playerRefs[i].ToString();
            }
        }
    }
}
