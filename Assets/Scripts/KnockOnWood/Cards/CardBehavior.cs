using UnityEngine;

namespace bandcProd
{
    public class CardBehavior : MonoBehaviour
    {
        public Card cardData;
        private SpriteRenderer spriteRenderer;
        private bool isFaceUp = false;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Card data)
        {
            cardData = data;
            spriteRenderer.sprite = cardData.backSprite;
        }

        public void FlipCard()
        {
            isFaceUp = !isFaceUp;
            spriteRenderer.sprite = isFaceUp ? cardData.frontSprite : cardData.backSprite;
        }
    }
}
