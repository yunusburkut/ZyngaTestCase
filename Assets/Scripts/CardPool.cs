using UnityEngine;
using System.Collections.Generic;

public class CardPool : MonoBehaviour
{
    // The card prefab that will be instantiated for the pool.
    // This prefab must have a Card component attached.
    [SerializeField] private GameObject cardPrefab;
    
    // The initial number of cards to instantiate and add to the pool.
    [SerializeField] private int initialPoolSize;

    // A queue to manage the card pool.
    // Cards are enqueued when not in use and dequeued when needed.
    private Queue<Card> pool = new Queue<Card>();

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Here, we initialize the card pool by instantiating a predefined number of card prefabs.
    /// Each instantiated card is deactivated and added to the pool.
    /// </summary>
    private void Awake()
    {
        // Create the initial pool of cards.
        for (int i = 0; i < initialPoolSize; i++)
        {
            // Instantiate a new card GameObject as a child of this object.
            GameObject cardObj = Instantiate(cardPrefab, transform);
            
            // Deactivate the card so it is not visible or active in the scene.
            cardObj.SetActive(false);
            
            // Get the Card component from the instantiated GameObject.
            Card cardComponent = cardObj.GetComponent<Card>();
            
            // Log an error if the card prefab does not contain a Card component.
            if (cardComponent == null)
            {
                Debug.LogError("cardPrefab does not contain a Card component!");
            }
            
            // Enqueue the card into the pool for later use.
            pool.Enqueue(cardComponent);
        }
    }

    /// <summary>
    /// Retrieves a card from the pool.
    /// If the pool is empty, a new card is instantiated.
    /// The card is then activated before being returned.
    /// </summary>
    /// <returns>A Card instance ready for use.</returns>
    public Card GetCard()
    {
        Card card;

        // If there is at least one card in the pool, dequeue it.
        if (pool.Count > 0)
        {
            card = pool.Dequeue();
        }
        else
        {
            // If the pool is empty, instantiate a new card.
            GameObject cardObj = Instantiate(cardPrefab, transform);
            card = cardObj.GetComponent<Card>();

            // Log an error if the card prefab does not contain a Card component.
            if (card == null)
            {
                Debug.LogError("cardPrefab does not contain a Card component!");
            }
        }
        
        // Activate the card so it becomes visible and functional.
        card.gameObject.SetActive(true);
        return card;
    }
    
    /// <summary>
    /// Returns a card back to the pool.
    /// The card is deactivated and then enqueued for later reuse.
    /// </summary>
    /// <param name="card">The Card instance to return to the pool.</param>
    public void ReturnCard(Card card)
    {
        // Deactivate the card so that it is not visible or interactive.
        card.gameObject.SetActive(false);
        // Enqueue the card back into the pool.
        pool.Enqueue(card);
    }
}
