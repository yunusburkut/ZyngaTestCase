using System.Collections.Generic;

public static class CombinationGenerator
{
    /// <summary>
    /// Generates all combinations of a specified size (targetCount) from the given list of cards.
    /// This method uses recursion to build up each combination and returns a list of combinations.
    /// </summary>
    /// <param name="list">The list of cards from which to generate combinations.</param>
    /// <param name="targetCount">The number of cards each combination should contain.</param>
    /// <returns>A list of combinations, where each combination is a list of cards.</returns>
    public static List<List<Card>> GenerateCombinations(List<Card> list, int targetCount)
    {
        // Create a list to store all the results (combinations)
        List<List<Card>> results = new List<List<Card>>();
        // Create a list that will hold the current combination being built.
        // The capacity is set to targetCount to minimize reallocation.
        List<Card> current = new List<Card>(targetCount);
        // Store the total number of cards in the list.
        int listCount = list.Count;
        // Begin the recursive generation process starting at index 0.
        GenerateCombinationsRecursive(list, targetCount, 0, current, results, listCount);
        return results;
    }

    /// <summary>
    /// Recursively generates combinations.
    /// </summary>
    /// <param name="list">The original list of cards.</param>
    /// <param name="targetCount">The number of cards still needed to complete the current combination.</param>
    /// <param name="start">The starting index in the list for choosing cards.</param>
    /// <param name="current">The current combination being built.</param>
    /// <param name="results">The list that accumulates all the valid combinations.</param>
    /// <param name="listCount">The total number of cards in the original list.</param>
    private static void GenerateCombinationsRecursive(List<Card> list, int targetCount, int start, List<Card> current, List<List<Card>> results, int listCount)
    {
        // Base case: If targetCount is 0, we have built a complete combination.
        if (targetCount == 0)
        {
            // Create a new list from the current combination and add it to the results.
            results.Add(new List<Card>(current));
            return;
        }
        // Iterate through the list starting from 'start'
        for (int i = start; i < listCount; i++)
        {
            // Add the current card to the combination.
            current.Add(list[i]);
            // Recursively build the combination by decreasing the targetCount.
            // The next call starts from the next index (i + 1) to avoid duplicates.
            GenerateCombinationsRecursive(list, targetCount - 1, i + 1, current, results, listCount);
            // Backtrack: Remove the last added card to try another candidate.
            current.RemoveAt(current.Count - 1);
        }
    }
}
