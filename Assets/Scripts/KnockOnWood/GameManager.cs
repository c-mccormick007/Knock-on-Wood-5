using UnityEngine;
using Fusion;
using FusionHelpers;

namespace bandcProd
{
	public class GameManager : NetworkBehaviour
	{
        public enum PlayState { LOBBY, DEAL, TURN, DISCARD, CHECK_END, END }
        [Networked] public PlayState currentPlayState { get; set; }
        [Networked, Capacity(2)] private NetworkArray<int> score => default;

		[SerializeField] GameObject playerReady;
		[SerializeField] GameObject oppReady;
        [SerializeField] GameObject readyUpControllerLobby;

        [SerializeField] DeckManager deckManager;

        [SerializeField] NetworkObject clientPlayer;
        [SerializeField] NetworkObject oppPlayer;

        [Networked] public NetworkObject currentPlayer { get; set; }
        [Networked] public NetworkObject otherPlayer { get; set; }
        [Networked] private NetworkArray<int> discardPile { get; set; }
        [Networked] private int topOfDeck { get; set; }

        public override void Render()
        {
            Debug.Log("Current playstate: " + currentPlayState);

            switch (currentPlayState)
            {
                case PlayState.LOBBY:
                    break;

                case PlayState.DEAL:
                    //dealHands();
                    currentPlayState = PlayState.TURN;
                    break;

                case PlayState.TURN:
                    if (currentPlayer.InputAuthority == Runner.LocalPlayer)
                    {
                        // Allow local player to draw/discard/declare Gin
                    }
                    break;

                case PlayState.DISCARD:
                    // Process discard and switch turns
                    break;

                case PlayState.CHECK_END:
                    // Check for scores over win value
                    break;

                case PlayState.END:
                    //EndGame();
                    break;
            }
        }

        public void StartGame()
        {


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


            currentPlayer = clientPlayer;
            otherPlayer = oppPlayer;
            topOfDeck = deckManager.ReturnTopCard();
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
