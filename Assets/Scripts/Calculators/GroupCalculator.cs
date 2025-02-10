using System.Collections.Generic;
using UnityEngine;

public class GroupCalculator
{
    // This field stores the calculated deadwood points.
    private int deadwood = 0;

    /// <summary>
    /// Groups cards in the deck into melds based on runs (consecutive cards of the same suit)
    /// or sets (cards with the same number). It resets all GroupIDs before grouping.
    /// </summary>
    /// <param name="deck">List of cards to group.</param>
    public void MarkGroups(List<Card> deck)
    {
        int n = deck.Count;
        // Reset all cards' GroupID to 0.
        for (int i = 0; i < n; i++)
            deck[i].SetGroupID(0);

        int index = 0;
        byte groupId = 1; // Starting GroupID for the first meld.
        while (index < n)
        {
            bool grouped = false; // Flag to indicate if a group was formed starting at this index.
            Card current = deck[index];
            // Get the current card's data once to avoid repeated calls.
            CardData currentData = current.GetCardData();

            // Check for a run meld (consecutive cards of the same suit).
            if (index < n - 1)
            {
                // Get the next card's data.
                CardData nextData = deck[index + 1].GetCardData();
                // If the suit is the same, try to form a run.
                if (currentData.Suit == nextData.Suit)
                {
                    // Determine the length of the run starting at this card.
                    int chainLength = GetRunChainLength(deck, index, n, currentData.Suit, currentData.Number);
                    // If the run has at least 3 cards, mark them with the same GroupID.
                    if (chainLength >= 3)
                    {
                        for (int k = index; k < index + chainLength; k++)
                            deck[k].SetGroupID(groupId);
                        groupId++;             // Increment GroupID for the next meld.
                        index += chainLength;  // Skip over the cards that have been grouped.
                        grouped = true;
                        continue; // Continue to the next iteration.
                    }
                }
            }
            // If no run was found, check for a set meld (cards with the same number).
            if (!grouped && index < n - 1)
            {
                int targetNumber = currentData.Number;
                CardData nextData = deck[index + 1].GetCardData();
                // If the next card has the same number, try to form a set.
                if (targetNumber == nextData.Number)
                {
                    // Determine the length of the set (cards with the same number).
                    int chainLength = GetSetChainLength(deck, index, n, targetNumber);
                    // If the set contains at least 3 cards, assign them the same GroupID.
                    if (chainLength >= 3)
                    {
                        for (int k = index; k < index + chainLength; k++)
                            deck[k].SetGroupID(groupId);
                        groupId++;
                        index += chainLength;
                        continue;
                    }
                }
            }
            index++; // Move to the next card if no meld is formed.
        }
    }

    /// <summary>
    /// Sorts the deck by suit then number, groups cards into runs,
    /// and calculates deadwood points afterward.
    /// Cards that are part of a run receive a non-zero GroupID.
    /// </summary>
    /// <param name="deck">The deck of cards to process.</param>
    public void CalculateRun(List<Card> deck)
    {
        // Sort deck by suit and then by number.
        deck.Sort(CardComparer.CompareBySuitThenNumber);
        int n = deck.Count;
        // Reset all GroupIDs.
        for (int i = 0; i < n; i++)
            deck[i].SetGroupID(0);

        byte groupId = 1;
        int index = 0;
        while (index < n)
        {
            // Retrieve current card data.
            CardData currentData = deck[index].GetCardData();
            // Determine the length of the consecutive run (chain) starting from the current card.
            int chainLength = GetRunChainLength(deck, index, n, currentData.Suit, currentData.Number);
            if (chainLength >= 3)
            {
                // If run length is at least 3, assign the same groupId to all cards in the run.
                for (int k = index; k < index + chainLength; k++)
                    deck[k].SetGroupID(groupId);
                groupId++;
                index += chainLength; // Skip the entire run.
            }
            else
            {
                index++; // No valid run; move to the next card.
            }
        }
        // After grouping, calculate the deadwood (points of ungrouped cards).
        CalculateDeadwood(deck);
    }

    /// <summary>
    /// Sorts the deck by number then suit, groups cards into sets (cards with the same number),
    /// and calculates deadwood points afterward.
    /// Cards that are part of a set receive a non-zero GroupID.
    /// </summary>
    /// <param name="deck">The deck of cards to process.</param>
    public void CalculateSet(List<Card> deck)
    {
        // Sort deck by number then suit.
        deck.Sort(CardComparer.CompareByNumberThenSuit);
        int n = deck.Count;
        // Reset all GroupIDs.
        for (int i = 0; i < n; i++)
            deck[i].SetGroupID(0);

        byte groupId = 1;
        int index = 0;
        while (index < n)
        {
            // Get the target number from the current card.
            int targetNumber = deck[index].GetCardData().Number;
            // Calculate how many consecutive cards have the same number.
            int chainLength = GetSetChainLength(deck, index, n, targetNumber);
            if (chainLength >= 3)
            {
                // If there are at least 3 cards with the same number, group them.
                for (int k = index; k < index + chainLength; k++)
                    deck[k].SetGroupID(groupId);
                groupId++;
                index += chainLength; // Skip over the grouped set.
            }
            else
            {
                index++; // No valid set; move to the next card.
            }
        }
        // Calculate deadwood after grouping.
        CalculateDeadwood(deck);
    }

    /// <summary>
    /// Calculates the length of a run chain (consecutive cards of the same suit).
    /// Starts from the given index and returns how many cards form a consecutive sequence.
    /// </summary>
    /// <param name="deck">The deck of cards.</param>
    /// <param name="startIndex">Index to start the run from.</param>
    /// <param name="n">Total number of cards in the deck.</param>
    /// <param name="suit">The suit to match.</param>
    /// <param name="startNumber">The starting number of the run.</param>
    /// <returns>The number of consecutive cards forming the run.</returns>
    private static int GetRunChainLength(List<Card> deck, int startIndex, int n, int suit, int startNumber)
    {
        int chainLength = 1;
        for (int i = startIndex + 1; i < n; i++)
        {
            CardData nextData = deck[i].GetCardData();
            if (nextData.Suit != suit)
                break;
            // Check if the card number is exactly the expected next number.
            if (nextData.Number == startNumber + chainLength)
                chainLength++;
            else
                break;
        }
        return chainLength;
    }

    /// <summary>
    /// Calculates the total deadwood points for the deck.
    /// Deadwood points are the sum of the points of cards that are not part of any meld (GroupID == 0).
    /// </summary>
    /// <param name="deck">The deck of cards.</param>
    /// <returns>Total deadwood points.</returns>
    public int CalculateDeadwood(List<Card> deck)
    {
        deadwood = 0;
        int n = deck.Count;
        // Sum points for cards with GroupID 0 (ungrouped cards).
        for (int i = 0; i < n; i++)
        {
            if (deck[i].GetCardData().GroupID == 0)
                deadwood += deck[i].GetPoint();
        }
        Debug.Log("Deadwood : " + deadwood);
        return deadwood;
    }

    /// <summary>
    /// Returns the current deadwood value (calculated in CalculateDeadwood).
    /// </summary>
    public int GetDeadwood()
    {
        return deadwood;
    }

    /// <summary>
    /// Calculates the length of a set chain (cards with the same number).
    /// Starts from the given index and returns the number of consecutive cards with the same number.
    /// </summary>
    /// <param name="deck">The deck of cards.</param>
    /// <param name="startIndex">Index to start the set from.</param>
    /// <param name="n">Total number of cards in the deck.</param>
    /// <param name="targetNumber">The number to match.</param>
    /// <returns>The count of cards in the set chain.</returns>
    private static int GetSetChainLength(List<Card> deck, int startIndex, int n, int targetNumber)
    {
        int chainLength = 1;
        for (int i = startIndex + 1; i < n; i++)
        {
            if (deck[i].GetCardData().Number == targetNumber)
                chainLength++;
            else
                break;
        }
        return chainLength;
    }
}
