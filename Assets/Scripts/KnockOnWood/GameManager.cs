using UnityEngine;
using Fusion;
using FusionHelpers;

namespace bandcProd
{
	public class GameManager : NetworkBehaviour
	{
		public enum PlayState { LOBBY, GIN, END }
		[Networked] public PlayState currentPlayState { get; set; }
        [Networked, Capacity(2)] private NetworkArray<int> score => default;

		[SerializeField] GameObject playerReady;
		[SerializeField] GameObject oppReady;
        [SerializeField] GameObject readyUpControllerLobby;

        [SerializeField] DeckManager deckManager;

        [SerializeField] NetworkObject clientPlayer;
		[SerializeField] NetworkObject oppPlayer;

        public override void Render()
        {
            Debug.Log("Current playstate: " + currentPlayState);
        }
        public void StartGame()
        {
            currentPlayState = PlayState.GIN;

            if (clientPlayer == null)
            {
                foreach (var player in Runner.ActivePlayers)
                {
                    var networkObject = Runner.GetPlayerObject(player);

                    if (networkObject != null)
                    {
                        if (player == Runner.LocalPlayer)
                        {
                            clientPlayer = networkObject.GetComponent<NetworkObject>();
                            Debug.Log("Client Player Assigned: " + clientPlayer.name);
                        }
                        else
                        {
                            oppPlayer = networkObject.GetComponent<NetworkObject>();
                            Debug.Log("Opponent Player Assigned: " + oppPlayer.name);
                        }
                    }
                }
            }

            playerReady.SetActive(false);
            oppReady.SetActive(false);
            readyUpControllerLobby = GameObject.FindGameObjectWithTag("readycontroller");
            readyUpControllerLobby.SetActive(false);
            dealHands();
        }

        private void dealHands()
        {
            for(int i = 0; i < 10; i++)
            {
                deckManager.DrawCard(clientPlayer);
                deckManager.DrawCard(oppPlayer);
            }
        }

    }

}
