using UnityEngine;
using Fusion;
using FusionHelpers;
using System.Collections.Generic;

namespace bandcProd
{
    [RequireComponent(typeof(NetworkCharacterController))]
    public class Player : FusionPlayer
    {
        [SerializeField] private int _deadwood;
        [SerializeField] private int _finalScore;
        [SerializeField] private int _currentTurnScore;

        [SerializeField] private List<int> _hand = new List<int>();
        //[SerializeField] private List<Card> _hand = new List<Card>(); -implement cards
        [SerializeField] private List<Vector3> _cardPositions = new List<Vector3>();

        [Networked] private int turnScore { get; set; }
        [Networked] private int deadwoodNet { get; set; }
        [Networked] private int totalScore { get; set; }
        [Networked] public bool ready { get; set; }

        private NetworkCharacterController _cc;

        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterController>();
        }

        public override void InitNetworkState()
        {
            deadwoodNet = 0;
            turnScore = 0;
            totalScore = 0;
            ready = false;
        }
    }
}
