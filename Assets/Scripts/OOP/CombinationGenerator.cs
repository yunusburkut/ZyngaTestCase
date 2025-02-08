using System.Collections.Generic;

public static class CombinationGenerator
{
    public static List<List<Card>> GenerateCombinations(List<Card> list, int targetCount)
    {
        List<List<Card>> results = new List<List<Card>>();
        GenerateCombinationsRecursive(list, targetCount, 0, new List<Card>(), results);
        return results;
    }

    private static void GenerateCombinationsRecursive(List<Card> list, int targetCount, int start, List<Card> current, List<List<Card>> results)
    {
        if (targetCount == 0)
        {
            results.Add(new List<Card>(current));
            return;
        }
        for (int i = start; i < list.Count; i++)
        {
            current.Add(list[i]);
            GenerateCombinationsRecursive(list, targetCount - 1, i + 1, current, results);
            current.RemoveAt(current.Count - 1);
        }
    }
}