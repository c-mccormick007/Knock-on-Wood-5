using UnityEngine;
using Fusion;
using FusionHelpers;
using System.Collections.Generic;

namespace bandcProd
{
    public class Player : NetworkBehaviour
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

        private void Awake()
        {
        }

        public override void Spawned()
        {
            deadwoodNet = 0;
            turnScore = 0;
            totalScore = 0;
            ready = false;
        }
    }
}
