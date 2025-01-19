using UnityEngine;

namespace bandcProd
{
    [CreateAssetMenu(fileName = "Card", menuName = "Card Game/Card")]
    public class Card : ScriptableObject
    {
        public string cardName;
        public Sprite frontSprite;
        public Sprite backSprite;
        public int value;
        public Suit suit;
    }

    public enum Suit
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs
    }
}