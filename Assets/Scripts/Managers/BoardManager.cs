using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    // A stack to hold the shuffled deck for drawing cards.
    private Stack<Card> deckStack = new Stack<Card>();
    // A temporary list used during deck creation and shuffling.
    private List<Card> deckList = new List<Card>();
    
    // Reference to the card pool to get card instances.
    [SerializeField] private CardPool cardPool;
    // Destination panel in the UI where drawn cards will be moved.
    [SerializeField] private RectTransform destinationPanel;
    // Start panel holds the initial position for the drawn card animation.
    [SerializeField] private RectTransform StartPanel;
    
    // Singleton instance of the BoardManager.
    public static BoardManager Instance;

    private void Awake()
    {
        // Set up the singleton instance.
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Optionally, you could initialize the deck here by calling Init().
    }

    /// <summary>
    /// Initializes the board by creating and shuffling the deck.
    /// </summary>
    public void Init()
    {
        CreateAndShuffleDeck();
    }

    /// <summary>
    /// Creates a full deck of cards (4 suits x 13 numbers = 52 cards),
    /// shuffles the deck using a Fisher–Yates algorithm,
    /// and then pushes all cards onto a stack for later drawing.
    /// </summary>
    private void CreateAndShuffleDeck()
    {
        // Loop through each suit (assumed to be 0 through 3).
        for (byte suit = 0; suit < 4; suit++)
        {
            // Loop through each number from 1 to 13.
            for (byte number = 1; number <= 13; number++)
            {
                // Create the card data for this card.
                CardData data = new CardData(number, suit, 0);
                // Get a card instance from the card pool.
                Card card = cardPool.GetCard();
                // Initialize the card with its data. The Card's Initialize method now handles
                // assigning the appropriate sprite from a sprite atlas if needed.
                card.Initialize(data);
                // Add the card to the deck list.
                deckList.Add(card);
            }
        }

        // Shuffle the deck list using the Fisher–Yates algorithm.
        for (int i = deckList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            // Swap the current card with a card at a random index.
            (deckList[i], deckList[randomIndex]) = (deckList[randomIndex], deckList[i]);
        }
        // Push all shuffled cards onto the deck stack.
        for (int i = 0; i < deckList.Count; i++)
        {
            deckStack.Push(deckList[i]);
        }
        // Clear the temporary deck list to free memory.
        deckList.Clear();
    }

    /// <summary>
    /// Draws the top card from the deck stack.
    /// The drawn card is sent to the destination panel with an animation,
    /// and then added to the player's deck via MyDeckManager.
    /// </summary>
    public void DrawCard()
    {
        if (deckStack.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            return;
        }
        // Pop the top card from the deck stack.
        Card card = deckStack.Pop();
        // Move the drawn card to the destination panel with animation.
        SendDrawedCardToDeck(card);
        // Add the drawn card to the player's deck managed by MyDeckManager.
        MyDeckManager.Instance.AddCard(card);
    }

    /// <summary>
    /// Moves a drawn card to the destination panel.
    /// The card's RectTransform is re-parented to the destinationPanel,
    /// its position is set based on the StartPanel, and then it is animated to position Vector2.zero.
    /// </summary>
    /// <param name="card">The drawn card to be moved.</param>
    public void SendDrawedCardToDeck(Card card)
    {
        // Get the RectTransform of the card.
        RectTransform cardRect = card.GetComponent<RectTransform>();
        // Set the parent of the card to the destination panel without changing its local scale.
        cardRect.SetParent(destinationPanel, false);
        // Set the card's position to the starting position (from StartPanel).
        cardRect.anchoredPosition = new Vector2(StartPanel.anchoredPosition.x, StartPanel.anchoredPosition.y);
        // Animate the card's anchored position to Vector2.zero over 0.5 seconds with an ease-out effect.
        cardRect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutQuad);
    }
}
