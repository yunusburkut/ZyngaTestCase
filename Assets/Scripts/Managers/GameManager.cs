using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Number of cards to deal during the game start.
    [SerializeField] private int cardsToDeal = 11;
    // Delay (in seconds) between drawing each card.
    [SerializeField] private float drawDelay = 0.5f;

    private void Start()
    {
        // Initialize the board manager.
        // This typically sets up the deck, shuffles the cards, etc.
        BoardManager.Instance.Init();

        // Start the coroutine that will deal the cards sequentially.
        StartCoroutine(DealCardsRoutine());
    }
    
    /// <summary>
    /// Coroutine that deals cards one by one.
    /// For each card to deal, it calls the DrawCard method of the BoardManager,
    /// then waits for a specified delay before drawing the next card.
    /// After all cards are dealt, it logs that the process is complete.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator DealCardsRoutine()
    {
        // Loop through the number of cards to be dealt.
        for (int i = 0; i < cardsToDeal; i++)
        {
            // Draw one card from the BoardManager.
            BoardManager.Instance.DrawCard();

            // Wait for a specified delay before drawing the next card.
            yield return new WaitForSeconds(drawDelay);
        }
        
        // Log that all cards have been dealt.
        Debug.Log("Tüm kartlar dağıtıldı.");
    }
}