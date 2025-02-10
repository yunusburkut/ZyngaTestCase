using System.Collections.Generic;

public class MeldOptimizer
{
    /// <summary>
    /// Represents the result of the optimal meld computation.
    /// It contains the total deadwood (points from ungrouped cards)
    /// and the list of melds (each meld is a list of cards) used to achieve that result.
    /// </summary>
    public class OptimalResult
    {
        public int Deadwood;                 // Total deadwood points
        public List<List<Card>> Melds;       // List of melds used in the optimal solution
    }

    /// <summary>
    /// Computes the optimal melds from the given deck.
    /// It first resets all GroupIDs, then generates candidate melds from the deck.
    /// Finally, it uses recursion with memoization (DP) to compute the combination of melds
    /// that minimizes the deadwood (the sum of points of ungrouped cards).
    /// </summary>
    /// <param name="deck">The deck of cards to process.</param>
    /// <returns>An OptimalResult containing the minimal deadwood and the corresponding melds.</returns>
    public OptimalResult ComputeOptimalMelds(List<Card> deck)
    {
        int deckCount = deck.Count;
        // Reset each card's GroupID to 0 before computation.
        for (int i = 0; i < deckCount; i++)
            deck[i].SetGroupID(0);
        
        // Generate candidate melds (possible valid combinations of cards) from the deck.
        CandidateMeldGenerator candidateMeldGenerator = new CandidateMeldGenerator();
        List<List<Card>> candidateMelds = candidateMeldGenerator.GenerateCandidateMelds(deck);
        
        // Create a memoization dictionary to cache intermediate results.
        Dictionary<int, OptimalResult> memo = new Dictionary<int, OptimalResult>();
        
        // Create a working copy of the deck, so we don't modify the original deck.
        List<Card> remaining = new List<Card>(deck);
        
        // Recursively compute the optimal melds for the remaining cards.
        return ComputeOptimal(remaining, candidateMelds, memo);
    }

    /// <summary>
    /// Recursive helper method for ComputeOptimalMelds.
    /// It attempts to remove candidate melds from the remaining deck and computes the deadwood (unmelded points)
    /// for the resulting state using memoization to avoid re-computation.
    /// </summary>
    /// <param name="remaining">The list of remaining cards after some melds have been removed.</param>
    /// <param name="candidateMelds">The list of all candidate melds available.</param>
    /// <param name="memo">Memoization dictionary to store intermediate results based on a state hash.</param>
    /// <returns>An OptimalResult representing the best solution for the current remaining cards.</returns>
    private OptimalResult ComputeOptimal(List<Card> remaining, List<List<Card>> candidateMelds, Dictionary<int, OptimalResult> memo)
    {
        int remCount = remaining.Count;
        // Base case: if no cards remain, there is no deadwood and no melds.
        if (remCount == 0)
            return new OptimalResult { Deadwood = 0, Melds = new List<List<Card>>() };

        // Generate a unique state hash for the current remaining deck.
        int key = GetStateHash(remaining);
        // If this state has already been computed, return the cached result.
        if (memo.TryGetValue(key, out OptimalResult cached))
            return cached;

        // Initialize best deadwood with the sum of points of all remaining cards
        // (worst-case: no melds are used).
        int bestDeadwood = SumPoints(remaining);
        // Initialize best meld list as empty.
        List<List<Card>> bestMelds = new List<List<Card>>();
        // Create a temporary list to store cards removed when a candidate meld is applied.
        List<Card> removed = new List<Card>();

        int candidateCount = candidateMelds.Count;
        // Loop through all candidate melds.
        for (int i = 0; i < candidateCount; i++)
        {
            List<Card> meld = candidateMelds[i];
            // If the candidate meld is not a subset of the remaining cards, skip it.
            if (!IsSubset(meld, remaining))
                continue;

            // Clear the removed list for this candidate.
            removed.Clear();
            int meldCount = meld.Count;
            // Remove each card in the meld from the remaining deck.
            for (int j = 0; j < meldCount; j++)
            {
                Card card = meld[j];
                if (remaining.Remove(card))
                    removed.Add(card);
            }

            // Recursively compute the optimal result for the new remaining deck.
            OptimalResult subResult = ComputeOptimal(remaining, candidateMelds, memo);
            // If the current candidate meld reduces deadwood, update best result.
            if (subResult.Deadwood < bestDeadwood)
            {
                bestDeadwood = subResult.Deadwood;
                // Clone the melds from the subResult and add the current candidate meld.
                bestMelds = new List<List<Card>>(subResult.Melds);
                bestMelds.Add(meld);
            }
            // Backtrack: add the removed cards back into the remaining deck.
            remaining.AddRange(removed);
        }

        // Create the result for the current state.
        OptimalResult result = new OptimalResult { Deadwood = bestDeadwood, Melds = bestMelds };
        // Cache the result in memoization dictionary.
        memo[key] = result;
        return result;
    }

    /// <summary>
    /// Checks if the candidate meld (subset) is fully contained within the set of remaining cards.
    /// </summary>
    /// <param name="subset">The candidate meld to test.</param>
    /// <param name="set">The current remaining deck of cards.</param>
    /// <returns>True if every card in subset is in set; otherwise, false.</returns>
    private bool IsSubset(List<Card> subset, List<Card> set)
    {
        int subsetCount = subset.Count;
        for (int i = 0; i < subsetCount; i++)
        {
            if (!set.Contains(subset[i]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Sums up the point values of all cards in the list.
    /// </summary>
    /// <param name="cards">The list of cards whose points are to be summed.</param>
    /// <returns>The total sum of points.</returns>
    private int SumPoints(List<Card> cards)
    {
        int sum = 0;
        int count = cards.Count;
        for (int i = 0; i < count; i++)
            sum += cards[i].GetPoint();
        return sum;
    }

    /// <summary>
    /// Computes a unique hash for the current state of the remaining deck.
    /// The hash is based on the instance IDs of the cards.
    /// </summary>
    /// <param name="remaining">The list of remaining cards.</param>
    /// <returns>An integer hash representing the state of the deck.</returns>
    private int GetStateHash(List<Card> remaining)
    {
        int sum = 0;
        int xor = 0;
        int count = remaining.Count;
        for (int i = 0; i < count; i++)
        {
            int id = remaining[i].GetInstanceID();
            sum += id;
            xor ^= id;
        }
        // Multiply the sum by a prime (397) and XOR with the accumulated xor to generate a unique hash.
        return (sum * 397) ^ xor;
    }
}
