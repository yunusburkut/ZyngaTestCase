using UnityEngine;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;

public class MyDeckManager : MonoBehaviour
{
    // List that holds the current deck of cards.
    [SerializeField] private List<Card> myDeck = new List<Card>();

    // Layout configuration values:
    [SerializeField] private float cardSpacing = 30f;   // Spacing between cards during the initial layout.
    [SerializeField] private float shiftDuration = 0.5f;  // Duration for the initial layout shift animation.
    [SerializeField] private float spacing = 50f;         // Spacing used when repositioning the deck after grouping.
    [SerializeField] private float moveDuration = 0.5f;     // Duration for the repositioning animation.

    // Reference to the UI TextMeshPro element used to display the deadwood score.
    [SerializeField] private TextMeshProUGUI deadwoodText;
    
    // Singleton instance of MyDeckManager for easy global access.
    public static MyDeckManager Instance;

    // References to helper classes that control the layout, grouping, and meld optimization.
    private CardLayoutController layoutController;
    private GroupCalculator groupCalculator;
    private MeldOptimizer meldOptimizer;

    private void Awake()
    {
        // Implementing the Singleton pattern.
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Initialize helper classes with the configured parameters.
        layoutController = new CardLayoutController(cardSpacing, shiftDuration, spacing, moveDuration);
        groupCalculator = new GroupCalculator();
        meldOptimizer = new MeldOptimizer();
    }

    /// <summary>
    /// Adds a card to the deck, updates the deck layout, and updates the deadwood UI.
    /// </summary>
    /// <param name="card">The card to add.</param>
    public void AddCard(Card card)
    {
        if (card != null)
        {
            myDeck.Add(card);
            UpdateDeckLayout();
            // Update the deadwood UI after calculating the deadwood from the current deck.
            UpdateDeadwoodUI(groupCalculator.CalculateDeadwood(myDeck));
        }
    }

    /// <summary>
    /// Updates the deck layout using the initial card spacing and shift duration.
    /// </summary>
    public void UpdateDeckLayout()
    {
        layoutController.UpdateLayout(myDeck);
    }

    /// <summary>
    /// Sorts the deck by suit then number, then updates the layout.
    /// </summary>
    public void SortDeckBySuitThenNumber()
    {
        myDeck.Sort(CardComparer.CompareBySuitThenNumber);
        UpdateDeckLayout();
    }

    /// <summary>
    /// Sorts the deck by number then suit, then updates the layout.
    /// </summary>
    public void SortDeckByNumberThenSuit()
    {
        myDeck.Sort(CardComparer.CompareByNumberThenSuit);
        UpdateDeckLayout();
    }

    /// <summary>
    /// Groups the cards into melds using the grouping logic from GroupCalculator,
    /// then repositions the cards in the deck accordingly.
    /// </summary>
    public void MarkGroups()
    {
        groupCalculator.MarkGroups(myDeck);
        layoutController.RepositionCards(myDeck);
    }

    /// <summary>
    /// Processes the deck by calculating runs (consecutive cards with the same suit),
    /// updates the deadwood UI based on the current grouping, and repositions the deck.
    /// </summary>
    public void CalculateRun()
    {
        groupCalculator.CalculateRun(myDeck);
        // Update the UI with the deadwood value after run grouping.
        UpdateDeadwoodUI(groupCalculator.GetDeadwood());
        layoutController.RepositionDpGroupedCardsLeftAligned(myDeck);
    }

    /// <summary>
    /// Processes the deck by calculating sets (cards with the same number),
    /// updates the deadwood UI based on the current grouping, and repositions the deck.
    /// </summary>
    public void CalculateSet()
    {
        groupCalculator.CalculateSet(myDeck);
        // Update the UI with the deadwood value after set grouping.
        UpdateDeadwoodUI(groupCalculator.GetDeadwood());
        layoutController.RepositionDpGroupedCardsLeftAligned(myDeck);
    }
    
    /// <summary>
    /// Computes the optimal melds (a combination of runs/sets) from the current deck.
    /// It applies group IDs to the cards based on the optimal solution,
    /// repositions the deck, updates the deadwood UI, and logs the deadwood value.
    /// </summary>
    public void ComputeOptimalMelds()
    {
        MeldOptimizer.OptimalResult result = meldOptimizer.ComputeOptimalMelds(myDeck);
        byte groupId = 1;
        // Apply group IDs to the cards based on the optimal melds.
        foreach (List<Card> meld in result.Melds)
        {
            foreach (Card card in meld)
            {
                card.SetGroupID(groupId);
            }
            groupId++;
        }
        layoutController.RepositionDpGroupedCardsLeftAligned(myDeck);
        UpdateDeadwoodUI(result.Deadwood);
        Debug.Log("Deadwood : " + result.Deadwood);
    }

    /// <summary>
    /// Called when a card drag operation ends.
    /// It handles reordering of the deck based on the drop position and then re-groups the cards.
    /// </summary>
    /// <param name="draggedCard">The card that was dragged.</param>
    /// <param name="screenPosition">The screen position where the card was dropped.</param>
    /// <param name="eventCamera">The camera associated with the event.</param>
    public void OnCardDragEnd(Card draggedCard, Vector2 screenPosition, Camera eventCamera)
    {
        layoutController.HandleDragEnd(myDeck, draggedCard, screenPosition, eventCamera);
        MarkGroups();
    }

    /// <summary>
    /// Updates the animation of all cards during a drag operation.
    /// This is called to visually adjust the position of non-dragged cards.
    /// </summary>
    /// <param name="draggedCard">The card that is currently being dragged.</param>
    public void UpdateCardsAnimationDuringDrag(Card draggedCard)
    {
        layoutController.UpdateCardsAnimationDuringDrag(myDeck, draggedCard);
    }

    /// <summary>
    /// Returns the current deck of cards.
    /// </summary>
    public List<Card> GetDeck() => myDeck;

    /// <summary>
    /// Logs information about the current deck to the console.
    /// </summary>
    public void LogDeck()
    {
        StringBuilder sb = new StringBuilder("Deck: ");
        for (int i = 0; i < myDeck.Count; i++)
        {
            var data = myDeck[i].GetCardData();
            sb.Append($"[{i}] Number:{data.Number}, Suit:{data.Suit}  ");
        }
        Debug.Log(sb.ToString());
    }
    
    /// <summary>
    /// Updates the UI element (TextMeshProUGUI) with the current deadwood value.
    /// If the UI element is not assigned, the deadwood is logged to the console.
    /// </summary>
    /// <param name="deadwood">The deadwood value to display.</param>
    private void UpdateDeadwoodUI(int deadwood)
    {
        if (deadwoodText != null)
        {
            deadwoodText.text = "Deadwood: " + deadwood.ToString();
        }
        else
        {
            Debug.Log("Deadwood: " + deadwood);
        }
    }
}
