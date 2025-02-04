using UnityEngine;
using System.Collections.Generic;

public class CardPool : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int initialPoolSize = 20;

    private Queue<Card> pool = new Queue<Card>();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, transform);
            cardObj.SetActive(false); 
            Card cardComponent = cardObj.GetComponent<Card>();
            
            if (cardComponent == null)
            {
                Debug.LogError("cardPrefab, Card component içermiyor!");
            }
            
            pool.Enqueue(cardComponent); 
        }
    }

    public Card GetCard()
    {
        Card card;

        if (pool.Count > 0)
        {
            card = pool.Dequeue();
        }
        else
        {
            GameObject cardObj = Instantiate(cardPrefab, transform);
            card = cardObj.GetComponent<Card>();

            if (card == null)
            {
                Debug.LogError("cardPrefab, Card component içermiyor!");
            }
        }
        card.gameObject.SetActive(true); 
        return card;
    }
    
    public void ReturnCard(Card card)
    {
        card.gameObject.SetActive(false);
        pool.Enqueue(card);
    }
}
