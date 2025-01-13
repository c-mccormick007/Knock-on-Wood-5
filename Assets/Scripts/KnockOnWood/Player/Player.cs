using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    private ReadyUpLobbyController readyController;
    private Button readyButton;

    public override void Spawned()
    {
        base.Spawned();

        // Find the ReadyUpLobbyController in the scene
        readyController = FindObjectOfType<ReadyUpLobbyController>();
        if (readyController == null)
        {
            Debug.LogError("PlayerController could not find ReadyUpLobbyController.");
        }

        if (Object.HasInputAuthority)
        {
            // Assign the ready button
            readyButton = GameObject.Find("PlayerReady").GetComponent<Button>();
            if (readyButton != null)
            {
                readyButton.onClick.RemoveAllListeners();
                readyButton.onClick.AddListener(ReadyUpButtonPressed);
                Debug.Log("Assigned ReadyUpButton in PlayerController.");
            }
            else
            {
                Debug.LogWarning("ReadyUpButton not found in the scene.");
            }
        }
    }

    public void ReadyUpButtonPressed()
    {
        if (readyController == null)
        {
            Debug.LogWarning("ReadyUpLobbyController is null. Cannot send ready request.");
            return;
        }

        Debug.Log("ReadyUpButtonPressed called.");

        if (Object.HasStateAuthority)
        {
            Debug.Log("Player has State Authority. Toggling ready status directly.");
            readyController.ToggleReadyStatus(Runner.LocalPlayer);
        }
        else
        {
            Debug.Log("Player does not have State Authority. Sending RPC to toggle ready status.");
            RPC_RequestToggleReadyStatus(Runner.LocalPlayer);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestToggleReadyStatus(PlayerRef player)
    {
        if (readyController != null && readyController.Object.HasStateAuthority)
        {
            readyController.ToggleReadyStatus(player);
            Debug.Log($"RPC_RequestToggleReadyStatus called for {player}");
        }
        else
        {
            Debug.LogWarning("Received RPC_RequestToggleReadyStatus on a non-authoritative object.");
        }
    }
}
