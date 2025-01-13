using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkRunner runnerInstance;
    public static NetworkManager Instance { get; private set; }
    public string lobbyName = "default";

    public Transform sessionListContentParent;
    public GameObject sessionListEntryPrefab;
    public Dictionary<string, GameObject> sessionListUiDictionary = new Dictionary<string, GameObject>();

    public SceneRef gameplaySceneRef = SceneRef.FromIndex(2);
    public bool playersInSceneTwo = false;
    public GameObject playerPrefab;

    public List<PlayerRef> playerList = new List<PlayerRef>();
    public ReadyUpLobbyController readyController;
    public GameObject readyUpControllerPrefab;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        runnerInstance = gameObject.GetComponent<NetworkRunner>();
        if (runnerInstance == null)
        {
            runnerInstance = gameObject.AddComponent<NetworkRunner>();
        }
    }

    private void Start()
    {
        Debug.Log("run start");
        runnerInstance.JoinSessionLobby(SessionLobby.Shared, lobbyName);
    }

    public async void CreateRandomSession()
    {
        int randomInt = UnityEngine.Random.Range(1000, 9999);
        string randomSessionName = "Room-" + randomInt;

        Debug.Log($"Attempting to create session: {randomSessionName}");

        var result = await runnerInstance.StartGame(new StartGameArgs
        {
            SessionName = randomSessionName,
            GameMode = GameMode.Shared,
            Scene = gameplaySceneRef,
            PlayerCount = 2,
        });

        if (result.Ok)
        {
            Debug.Log("[FusionTest] Started Shared Player session successfully.");

            PlayerRef localPlayer = runnerInstance.LocalPlayer;
            if (!playerList.Contains(localPlayer))
            {

                playerList.Add(localPlayer);
                Debug.Log($"Added Host PlayerRef: {localPlayer}");

                readyController = FindAnyObjectByType<ReadyUpLobbyController>();
                if (readyController != null)
                {
                    readyController.AddPlayerToReadyDict(localPlayer);
                    Debug.Log($"Added Host to ReadyUpLobbyController: {localPlayer}");
                }
                else
                {
                    Debug.LogWarning("ReadyUpLobbyController not found after spawning.");
                }
                RPC_UpdateGlobalPlayerList(playerList.ToArray());
            }
        }
        else
        {
            Debug.LogError($"[FusionTest] Failed to start: {result.ShutdownReason}");
        }
    }


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        StartCoroutine(HandlePlayerJoin(player));
    }

    private IEnumerator<WaitForSeconds> HandlePlayerJoin(PlayerRef player)
    {
        // Wait until ReadyUpLobbyController is initialized
        while (readyController == null)
        {
            Debug.Log("Waiting for ReadyUpLobbyController initialization...");
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("Player joined: " + player);

        if (!playerList.Contains(player))
        {
            playerList.Add(player);
            Debug.Log($"Added Player: {player} to playerList");

            if (runnerInstance.LocalPlayer == player)
            {
                Debug.Log("Spawning player network object...");
                NetworkObject playerNetworkObject = runnerInstance.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
                runnerInstance.SetPlayerObject(player, playerNetworkObject);
            }

            readyController.AddPlayerToReadyDict(player);
        }

        RPC_UpdateGlobalPlayerList(playerList.ToArray());
        StartCoroutine(HandlePlayerJoin());

        // Log the entire playerList
        Debug.Log("Current playerList:");
        foreach (var p in playerList)
        {
            Debug.Log($" - {p}");
        }
    }



    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene load complete. Initializing ReadyUpLobbyController...");
        StartCoroutine(InitializeReadyUpLobbyController());
    }
    private IEnumerator<WaitForSeconds> HandlePlayerJoin()
    {
        while (readyController == null)
        {
            readyController = FindFirstObjectByType<ReadyUpLobbyController>();
            if (readyController != null) break;
            yield return new WaitForSeconds(0.1f);
        }

        readyController.readyButton = GameObject.Find("PlayerReady").GetComponent<Button>();
        Debug.LogWarning("Found button: " + readyController.readyButton);
        readyController.readyButton.onClick.RemoveAllListeners();
        readyController.readyButton.onClick.AddListener(() => {
            Debug.Log("READY BUTTON CLICK DETECTED!");
            readyController.ReadyUpButtonPressed();
        });

        // Assign the NetworkManager
        readyController.NetworkManager = this;

        yield break;
    }
    private IEnumerator<WaitForSeconds> InitializeReadyUpLobbyController()
    {
        
        if (runnerInstance.IsSharedModeMasterClient)
        {
            NetworkObject readyUpObj = runnerInstance.Spawn(readyUpControllerPrefab, Vector3.zero, Quaternion.identity);
            readyController = readyUpObj.GetComponent<ReadyUpLobbyController>();
            Debug.Log("Spawning player network object...");
            NetworkObject playerNetworkObject = runnerInstance.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, runnerInstance.LocalPlayer);
            runnerInstance.SetPlayerObject(runnerInstance.LocalPlayer, playerNetworkObject);
        }

        while (readyController == null)
        {
            readyController = FindFirstObjectByType<ReadyUpLobbyController>();
            if (readyController != null) break;
            yield return new WaitForSeconds(0.1f);
        }

        readyController.readyButton = GameObject.Find("PlayerReady").GetComponent<Button>();
        Debug.LogWarning("Found button: " + readyController.readyButton);
        readyController.readyButton.onClick.RemoveAllListeners();
        readyController.readyButton.onClick.AddListener(() => {
            Debug.Log("READY BUTTON CLICK DETECTED!");
            readyController.ReadyUpButtonPressed();
        });

        // Assign the NetworkManager
        readyController.NetworkManager = this;

        yield break;
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_UpdateGlobalPlayerList(PlayerRef[] updatedPlayerList)
    {
        Debug.Log("RPC_UpdateGlobalPlayerList called on client.");
        playerList.Clear();
        playerList.AddRange(updatedPlayerList);
        Debug.Log($"Global player list updated: {string.Join(", ", playerList)}");
    }




    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left?");
        playerList.Remove(player);

        RPC_UpdateGlobalPlayerList(playerList.ToArray());

        Debug.Log("Updated player list sent to all clients.");
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        DeleteOldSessionsFromUI(sessionList);
        CompareLists(sessionList);
    }

    private void CompareLists(List<SessionInfo> sessionList)
    {
        foreach (SessionInfo session in sessionList)
        {
            if (sessionListUiDictionary.ContainsKey(session.Name))
            {
                UpdateEntryUI(session);
            }
            else
            {
                CreateEntryUI(session);
            }
        }
    }

    private void CreateEntryUI(SessionInfo session)
    {
        GameObject newEntry = GameObject.Instantiate(sessionListEntryPrefab);
        newEntry.transform.parent = sessionListContentParent;

        SessionListEntry entryScript = newEntry.GetComponent<SessionListEntry>();
        sessionListUiDictionary.Add(session.Name, newEntry);

        entryScript.roomName.text = session.Name;
        entryScript.playerCount.text = session.PlayerCount.ToString() + "/" + session.MaxPlayers.ToString();

        entryScript.joinButton.interactable = session.IsOpen;

        newEntry.SetActive(session.IsVisible);
    }

    private void UpdateEntryUI(SessionInfo session)
    {
        sessionListUiDictionary.TryGetValue(session.Name, out GameObject newEntry);
        sessionListUiDictionary.Add(session.Name, newEntry);

        SessionListEntry entryScript = newEntry.GetComponent<SessionListEntry>();

        entryScript.roomName.text = session.Name;
        entryScript.playerCount.text = session.PlayerCount.ToString() + "/" + session.MaxPlayers.ToString();

        entryScript.joinButton.interactable = session.IsOpen;

        newEntry.SetActive(session.IsVisible);
    }

    private void DeleteOldSessionsFromUI(List<SessionInfo> sessionList)
    {
        bool isContained = false;
        GameObject uiToDelete = null;

        foreach (KeyValuePair<string, GameObject> kvp in sessionListUiDictionary)
        {
            string sessionKey = kvp.Key;

            foreach (SessionInfo sessionInfo in sessionList)
            {
                if (sessionInfo.Name == sessionKey)
                {
                    isContained = true;
                    break;
                }
            }
            if (!isContained)
            {
                uiToDelete = kvp.Value;
                sessionListUiDictionary.Remove(sessionKey);
                Destroy(uiToDelete);
            }
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server.");
        //runnerInstance.JoinSessionLobby(SessionLobby.Shared, lobbyName);
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"[Fusion] OnConnectFailed: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.LogWarning("[Fusion] OnConnectRequest called, but not implemented.");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug.LogWarning("[Fusion] OnCustomAuthenticationResponse called, but not implemented.");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.LogError($"[Fusion] OnDisconnectedFromServer: {reason}");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }



    

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }



    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("OnSceneLoadStart");
    }



    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Shutdown? " + shutdownReason);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}
