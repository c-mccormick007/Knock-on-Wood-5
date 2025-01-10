using UnityEngine;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class ReadyUpLobbyController : NetworkBehaviour
{
    [SerializeField] public NetworkManager NetworkManager;

    [SerializeField] public List<PlayerRef> playerRefs = new List<PlayerRef>();
    [SerializeField] public TextMeshProUGUI player1;
    [SerializeField] public TextMeshProUGUI player2;
    [SerializeField] public bool p1IsReady;
    [SerializeField] public bool p2IsReady;

    public TextMeshProUGUI readyUp;
    public TextMeshProUGUI oppReadyUp;

    [Networked] public NetworkDictionary<PlayerRef, bool> playerReadyStatus { get; }

    public void Awake()
    {
        NetworkManager = NetworkManager.Instance;
    }

    public override void Spawned()
    {
        base.Spawned();

        foreach (var player in NetworkManager.playerList)
        {
            Debug.Log("PLAYER: " + player);
            playerReadyStatus.Set(player, false);
        }

    }
    public void AddPlayerToReadyDict(PlayerRef player)
    {
        if (!playerReadyStatus.ContainsKey(player))
        {
            playerReadyStatus.Set(player, false); 
            Debug.Log($"Player {player} added to ready dictionary.");
        }
    }

    public void RemovePlayerFromReadyDict(PlayerRef player)
    {
        if (Object.HasStateAuthority)
        {
            if (playerReadyStatus.ContainsKey(player))
            {
                playerReadyStatus.Remove(player);
                Debug.Log($"Player {player} removed from ready dictionary.");
            }
        }
    }

    public void ReadyUpButtonPressed()
    {
        NetworkManager.RPC_RequestToggleReadyStatus(Runner.LocalPlayer);
    }




    public override void Render()
    {
        UpdateNameVisuals();

        base.Render();

        foreach (var kvp in playerReadyStatus)
        {
            UpdateUIForPlayer(kvp.Key, kvp.Value);
            UpdateReadyStatusUI();
        }
    }

    private void UpdateReadyStatusUI()
    {
        foreach (var kvp in playerReadyStatus)
        {
            Debug.Log(kvp.Key + " " + kvp.Value);
            UpdateUIForPlayer(kvp.Key, kvp.Value);
        }
    }


    private void UpdateUIForPlayer(PlayerRef player, bool isReady)
    {
        if (player == Runner.LocalPlayer)
        {
            readyUp.text = isReady ? "Ready!" : "Not Ready";
        }
        else
        {
            oppReadyUp.text = isReady ? "Ready!" : "Not Ready";
        }
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
