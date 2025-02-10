using System.Collections.Generic;

public class CandidateMeldGenerator
{
    // Cache for candidate melds to avoid re-allocation on every call.
    private readonly List<List<Card>> _candidatesCache = new List<List<Card>>();
    // Dictionary to group cards by their suit.
    private readonly Dictionary<int, List<Card>> _suitGroupsCache = new Dictionary<int, List<Card>>();
    // Dictionary to group cards by their number.
    private readonly Dictionary<int, List<Card>> _numberGroupsCache = new Dictionary<int, List<Card>>();
    // (Optional) Cache for sorted group IDs if needed later.
    private readonly List<int> _sortedGroupIDsCache = new List<int>();

    /// <summary>
    /// Generates candidate melds (combinations) from the given deck.
    /// It groups cards by suit to find "runs" (consecutive numbers) 
    /// and by number to find "sets" (cards with the same number),
    /// then produces candidate combinations that are at least 3 cards long.
    /// </summary>
    /// <param name="deck">The list of cards from which to generate candidate melds.</param>
    /// <returns>A list of candidate melds, where each meld is a list of cards.</returns>
    public List<List<Card>> GenerateCandidateMelds(List<Card> deck)
    {
        // Clear all caches so previous data does not interfere.
        _candidatesCache.Clear();
        _suitGroupsCache.Clear();
        _numberGroupsCache.Clear();
        _sortedGroupIDsCache.Clear();

        int n = deck.Count;

        // Group cards by suit.
        for (int i = 0; i < n; i++)
        {
            // Get the suit value of the current card.
            int suit = deck[i].GetCardData().Suit;
            // Try to get an existing group for this suit; if it doesn't exist, create it.
            if (!_suitGroupsCache.TryGetValue(suit, out List<Card> group))
            {
                group = new List<Card>();
                _suitGroupsCache.Add(suit, group);
            }
            // Add the card to the appropriate suit group.
            group.Add(deck[i]);
        }

        // Process each suit group to generate "run" candidate melds.
        foreach (var pair in _suitGroupsCache)
        {
            List<Card> cardsOfSuit = pair.Value;
            // Sort the cards in this suit group by suit then number.
            cardsOfSuit.Sort(CardComparer.CompareBySuitThenNumber);

            // Loop through the sorted cards to find consecutive sequences (runs).
            for (int i = 0; i < cardsOfSuit.Count; i++)
            {
                int start = i;
                int end = i;
                // Expand the sequence as long as the next card's number is exactly one more than the previous.
                while (end + 1 < cardsOfSuit.Count &&
                       cardsOfSuit[end + 1].GetCardData().Number ==
                       cardsOfSuit[end].GetCardData().Number + 1)
                {
                    end++;
                }
                // Calculate the length of the found run.
                int runLength = end - start + 1;
                // If the run has at least 3 cards, generate candidate melds from it.
                if (runLength >= 3)
                {
                    // Try all possible meld sizes from 3 up to the full length of the run.
                    for (int meldSize = 3; meldSize <= runLength; meldSize++)
                    {
                        // Slide a window of size "meldSize" along the run.
                        for (int j = start; j <= end - meldSize + 1; j++)
                        {
                            // Create a new candidate meld and add the appropriate cards.
                            List<Card> meld = new List<Card>(meldSize);
                            for (int k = 0; k < meldSize; k++)
                            {
                                meld.Add(cardsOfSuit[j + k]);
                            }
                            // Add this candidate meld to the cache.
                            _candidatesCache.Add(meld);
                        }
                    }
                }
                // Skip processed cards in the run.
                i = end;
            }
        }

        // Group cards by number for "set" meld candidates.
        for (int i = 0; i < n; i++)
        {
            int number = deck[i].GetCardData().Number;
            // Try to get an existing group for this number; if not, create one.
            if (!_numberGroupsCache.TryGetValue(number, out List<Card> group))
            {
                group = new List<Card>();
                _numberGroupsCache.Add(number, group);
            }
            group.Add(deck[i]);
        }

        // Process each number group to generate "set" candidate melds.
        foreach (var pair in _numberGroupsCache)
        {
            List<Card> cardsOfNumber = pair.Value;
            // Sort the cards in this number group by number then suit.
            cardsOfNumber.Sort(CardComparer.CompareByNumberThenSuit);
            // Only consider groups that have at least 3 cards.
            if (cardsOfNumber.Count >= 3)
            {
                // Generate all 3-card combinations using the CombinationGenerator.
                List<List<Card>> comb3 = CombinationGenerator.GenerateCombinations(cardsOfNumber, 3);
                for (int i = 0; i < comb3.Count; i++)
                {
                    _candidatesCache.Add(comb3[i]);
                }
                // If there are 4 or more cards, also generate 4-card combinations.
                if (cardsOfNumber.Count >= 4)
                {
                    List<List<Card>> comb4 = CombinationGenerator.GenerateCombinations(cardsOfNumber, 4);
                    for (int i = 0; i < comb4.Count; i++)
                    {
                        _candidatesCache.Add(comb4[i]);
                    }
                }
            }
        }

        // Return the full list of candidate melds.
        return _candidatesCache;
    }
}
