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

		[SerializeField] NetworkObject clientPlayer;
		[SerializeField] NetworkObject oppPlayer;


        public void StartGame()
		{
			currentPlayState = PlayState.GIN;

			playerReady.SetActive(false);
			oppReady.SetActive(false);	
		}

	}

}
