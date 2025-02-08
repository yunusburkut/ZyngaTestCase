using System.Collections.Generic;
using System.Linq;

public class MeldOptimizer
{
    public class OptimalResult
    {
        public int Deadwood;
        public List<List<Card>> Melds;
    }

    public OptimalResult ComputeOptimalMelds(List<Card> deck)
    {
        // Tüm kartların GroupID'si sıfırlanır.
        foreach (Card card in deck)
            card.SetGroupID(0);

        List<List<Card>> candidateMelds = CandidateMeldGenerator.GenerateCandidateMelds(deck);
        Dictionary<int, OptimalResult> memo = new Dictionary<int, OptimalResult>();
        List<Card> remaining = new List<Card>(deck);
        OptimalResult result = ComputeOptimal(remaining, candidateMelds, memo);
        return result;
    }

    private OptimalResult ComputeOptimal(List<Card> remaining, List<List<Card>> candidateMelds, Dictionary<int, OptimalResult> memo)
    {
        if (remaining.Count == 0)
            return new OptimalResult { Deadwood = 0, Melds = new List<List<Card>>() };

        int key = GetStateHash(remaining);
        if (memo.TryGetValue(key, out OptimalResult cached))
            return cached;

        int bestDeadwood = SumPoints(remaining);
        List<List<Card>> bestMelds = new List<List<Card>>();

        List<Card> removed = new List<Card>();
        foreach (List<Card> meld in candidateMelds)
        {
            if (!IsSubset(meld, remaining))
                continue;

            removed.Clear();
            foreach (Card card in meld)
            {
                if (remaining.Remove(card))
                    removed.Add(card);
            }

            OptimalResult subResult = ComputeOptimal(remaining, candidateMelds, memo);
            int currentDeadwood = subResult.Deadwood;
            if (currentDeadwood < bestDeadwood)
            {
                bestDeadwood = currentDeadwood;
                bestMelds = new List<List<Card>>(subResult.Melds);
                bestMelds.Add(meld);
            }
            // Çıkarılan kartlar geri eklenir.
            remaining.AddRange(removed);
        }

        OptimalResult result = new OptimalResult { Deadwood = bestDeadwood, Melds = bestMelds };
        memo[key] = result;
        return result;
    }

    private bool IsSubset(List<Card> subset, List<Card> set)
    {
        return subset.All(card => set.Contains(card));
    }

    private int SumPoints(List<Card> cards)
    {
        int sum = 0;
        foreach (Card card in cards)
            sum += card.GetPoint();
        return sum;
    }

    private int GetStateHash(List<Card> remaining)
    {
        int sum = 0;
        int xor = 0;
        foreach (Card card in remaining)
        {
            int id = card.GetInstanceID();
            sum += id;
            xor ^= id;
        }
        return (sum * 397) ^ xor;
    }
}
