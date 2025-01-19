using UnityEngine;
using Fusion;
using FusionHelpers;

namespace bandcProd
{
	public class GameManager : NetworkBehaviour
	{
		public enum PlayState { LOBBY, LEVEL, END }
		[Networked] public PlayState currentPlayState { get; set; }
		[Networked, Capacity(2)] private NetworkArray<int> score => default;


		public void StartGame()
		{
			//code here
		}

	}

}
