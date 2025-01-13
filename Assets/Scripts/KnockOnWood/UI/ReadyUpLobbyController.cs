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
    public Button readyButton;

    [Networked][SerializeField] public NetworkDictionary<PlayerRef, bool> playerReadyStatus { get; }

    private Dictionary<PlayerRef, bool> localReadyStatus = new Dictionary<PlayerRef, bool>();

    public void Awake()
    {
        if (player1 == null)
            player1 = GameObject.Find("PlayerText").GetComponent<TextMeshProUGUI>();

        if (player2 == null)
            player2 = GameObject.Find("Player2Text").GetComponent<TextMeshProUGUI>();

        if (readyUp == null)
            readyUp = GameObject.Find("ReadyUpTxt").GetComponent<TextMeshProUGUI>();

        if (oppReadyUp == null)
            oppReadyUp = GameObject.Find("OppReadyUpTxt").GetComponent<TextMeshProUGUI>();

        StartCoroutine(InitializeNetworkManager());
    }

    private IEnumerator<WaitForSeconds> InitializeNetworkManager()
    {
        while (NetworkManager == null)
        {
            NetworkManager = NetworkManager.Instance;
            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }

    public override void Spawned()
    {
        base.Spawned();

        foreach (var player in NetworkManager.playerList)
        {
            if (!playerReadyStatus.ContainsKey(player))
            {
                localReadyStatus[player] = false;
                playerReadyStatus.Set(player, false);
            }
        }

        foreach (var kvp in playerReadyStatus)
        {
            localReadyStatus[kvp.Key] = kvp.Value;
        }
    }

    public void AddPlayerToReadyDict(PlayerRef player)
    {
        if (Object.HasStateAuthority)
        {
            if (!playerReadyStatus.ContainsKey(player))
            {
                playerReadyStatus.Set(player, false);
                localReadyStatus[player] = false;
            }
        }
    }

    public void RemovePlayerFromReadyDict(PlayerRef player)
    {
        if (Object.HasStateAuthority)
        {
            if (playerReadyStatus.ContainsKey(player))
            {
                playerReadyStatus.Remove(player);
                localReadyStatus.Remove(player);
            }
        }
    }

    public void ReadyUpButtonPressed()
    {
        if (NetworkManager == null)
        {
            Debug.LogWarning("[ReadyUpLobbyController] NetworkManager is null. Cannot toggle ready status.");
            return;
        }
        if (Runner == null)
        {
            Debug.LogWarning("[ReadyUpLobbyController] Runner is null. Is this object actually spawned by Fusion?");
            return;
        }
        if (Runner.LocalPlayer == null)
        {
            Debug.LogWarning("[ReadyUpLobbyController] Runner.LocalPlayer is null. Player might not be joined yet.");
            return;
        }

        Debug.Log("FUNCTION IS GOING");

        if (Object.HasStateAuthority)
        {
            Debug.Log("We have authority.");
            ToggleReadyStatus(Runner.LocalPlayer);
        }
        else
        {
            Debug.Log("We have no authority.");
            RPC_RequestToggleReadyStatus(Runner.LocalPlayer);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RequestToggleReadyStatus(PlayerRef player)
    {
        Debug.Log($"RPC_RequestToggleReadyStatus called for {player}");
        Debug.Log($"readyController.HasStateAuthority = {Object.HasStateAuthority}");
        Debug.Log($"[MASTER] Toggling readiness for {player}");
        ToggleReadyStatus(player);
    }

    public void ToggleReadyStatus(PlayerRef player)
    {
        if (playerReadyStatus.ContainsKey(player))
        {
            Debug.Log($"Toggling {player} from {playerReadyStatus[player]} to {!playerReadyStatus[player]}");
            bool currentStatus = playerReadyStatus[player];
            playerReadyStatus.Set(player, !currentStatus);
            localReadyStatus[player] = !currentStatus;
            UpdateUIForPlayer(player, !currentStatus);
            Debug.Log($"New status is {playerReadyStatus[player]} for {player}");
        }
        else
        {
            Debug.LogWarning($"PlayerRef {player} not found in playerReadyStatus.");
        }
    }

    public override void Render()
    {
        base.Render();
        DetectChangesAndUpdateUI();
        UpdateNameVisuals();
    }

    private void DetectChangesAndUpdateUI()
    {
        Debug.Log("DetectChangesAndUpdateUI: Checking dictionary...");
        Debug.Log($"LocalPlayer is {Runner.LocalPlayer}");

        foreach (var kvp in playerReadyStatus)
        {
            Debug.Log($"Key={kvp.Key}, NetworkValue={kvp.Value}, localValue={(localReadyStatus.ContainsKey(kvp.Key) ? localReadyStatus[kvp.Key].ToString() : "N/A")}");

            if (!localReadyStatus.TryGetValue(kvp.Key, out var previousValue) || previousValue != kvp.Value)
            {
                localReadyStatus[kvp.Key] = kvp.Value;
                UpdateUIForPlayer(kvp.Key, kvp.Value);
            }
        }

        foreach (var key in new List<PlayerRef>(localReadyStatus.Keys))
        {
            if (!playerReadyStatus.ContainsKey(key))
            {
                localReadyStatus.Remove(key);
            }
        }
    }

    public void UpdateUIForPlayer(PlayerRef player, bool isReady)
    {
        Debug.Log("Updating UI Button");
        Debug.Log($"Player: {player}");
        Debug.Log($"IsReady: {isReady}");
        Debug.Log($"Runner.LocalPlayer: {Runner.LocalPlayer}");

        if (player == Runner.LocalPlayer)
        {
            readyUp.text = isReady ? "Ready!" : "Not Ready";
            Debug.Log("Updated readyUp text.");
        }
        else
        {
            oppReadyUp.text = isReady ? "Ready!" : "Not Ready";
            Debug.Log("Updated oppReadyUp text.");
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
