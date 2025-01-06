using UnityEngine;
using Fusion;
using FusionHelpers;

namespace bandcProd
{
	public class GameManager : FusionSession
	{
		public enum PlayState { LOBBY, LEVEL, END }
		[Networked] public PlayState currentPlayState { get; set; }
		[Networked, Capacity(2)] private NetworkArray<int> score => default;

		protected override void OnPlayerAvatarAdded(FusionPlayer fusionPlayer)
		{
			Runner.Spawn(fusionPlayer);
		}

		protected override void OnPlayerAvatarRemoved(FusionPlayer fusionPlayer)
		{
			Debug.Log("Player removed");
		}
	}

}
