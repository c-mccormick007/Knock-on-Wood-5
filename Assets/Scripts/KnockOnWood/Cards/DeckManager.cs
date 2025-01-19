using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Fusion;

namespace bandcProd
{
    public class DeckManager : NetworkBehaviour
    {
        [Networked, Capacity(52)]
        public NetworkArray<int> DeckState { get; }

        [SerializeField] public NetworkManager NetworkManager;

        public List<Card> allCards;

        [SerializeField]
        private List<Card> deck = new List<Card> ();

        public override void Spawned()
        {
            StartCoroutine(WaitForFusionInitialization());
            Debug.Log("ALL CARDS COUNT: " + allCards.Count);
        }

        private IEnumerator WaitForFusionInitialization()
        {
            while (NetworkManager == null)
            {
                NetworkManager = NetworkManager.Instance;
                yield return new WaitForSeconds(0.1f);
            }
            Debug.Log("Manager Initalized.");
            if (Object.HasStateAuthority)
            {
                InitializeDeck();
                Debug.Log("DECK COUNT: " + deck.Count);
                ShuffleDeck();
                SyncDeckState();
            }
        }

        void InitializeDeck()
        {
            deck.Clear();
            deck.AddRange(allCards);
        }

        void ShuffleDeck()
        {
            for (int i = 0; i < deck.Count; i++)
            {
                int randomIndex = Random.Range(0, deck.Count);
                Card temp = deck[i];
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }

        void SyncDeckState()
        {
            Debug.Log($"Syncing DeckState... Deck count: {deck.Count}, DeckState capacity: {DeckState.Length}");

            for (int i = 0; i < DeckState.Length; i++)
            {
                if (i < deck.Count)
                {
                    int index = allCards.IndexOf(deck[i]);
                    DeckState.Set(i, index);
                    Debug.Log($"DeckState[{i}] set to card index: {index}");
                }
                else
                {
                    DeckState.Set(i, -1); 
                    Debug.Log($"DeckState[{i}] set to empty slot (-1)");
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateDeckState()
        {
            LoadDeckFromState();
        }   

        public void LoadDeckFromState()
        {
            deck.Clear();
            for (int i = 0; i < DeckState.Length; i++)
            {
                int cardIndex = DeckState[i];
                if (cardIndex >= 0 && cardIndex < allCards.Count) 
                {
                    deck.Add(allCards[cardIndex]);
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            RPC_UpdateDeckState();
            if (!Object.HasStateAuthority && deck.Count == 0)
            {
                LoadDeckFromState();
            }
        }

        public Card DrawCard(NetworkObject player)
        {
            if (deck.Count == 0) return null;
            Card drawnCard = deck[0];
            deck.RemoveAt(0);
            
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController.hand.Add(drawnCard);

            if (Object.HasStateAuthority)
            {
                SyncDeckState();
            }

            return drawnCard;
        }

        public override string ToString()
        {
            if (deck == null || deck.Count == 0)
                return "Deck is empty.";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Deck Contents:");

            foreach (Card card in deck)
            {
                sb.AppendLine(card.ToString());
            }

            return sb.ToString();
        }
    }


}
