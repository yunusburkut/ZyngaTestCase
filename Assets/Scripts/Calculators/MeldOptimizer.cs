using System.Collections.Generic;

public class MeldOptimizer
{
    public class OptimalResult
    {
        public int Deadwood;
        public List<List<Card>> Melds;
    }

    public OptimalResult ComputeOptimalMelds(List<Card> deck)
    {
        int deckCount = deck.Count;
        for (int i = 0; i < deckCount; i++)
            deck[i].SetGroupID(0);
        CandidateMeldGenerator candidateMeldGenerator = new CandidateMeldGenerator();
        List<List<Card>> candidateMelds = candidateMeldGenerator.GenerateCandidateMelds(deck);
        Dictionary<int, OptimalResult> memo = new Dictionary<int, OptimalResult>();
        List<Card> remaining = new List<Card>(deck);
        return ComputeOptimal(remaining, candidateMelds, memo);
    }

    private OptimalResult ComputeOptimal(List<Card> remaining, List<List<Card>> candidateMelds, Dictionary<int, OptimalResult> memo)
    {
        int remCount = remaining.Count;
        if (remCount == 0)
            return new OptimalResult { Deadwood = 0, Melds = new List<List<Card>>() };
        int key = GetStateHash(remaining);
        if (memo.TryGetValue(key, out OptimalResult cached))
            return cached;
        int bestDeadwood = SumPoints(remaining);
        List<List<Card>> bestMelds = new List<List<Card>>();
        List<Card> removed = new List<Card>();
        int candidateCount = candidateMelds.Count;
        for (int i = 0; i < candidateCount; i++)
        {
            List<Card> meld = candidateMelds[i];
            if (!IsSubset(meld, remaining))
                continue;
            removed.Clear();
            int meldCount = meld.Count;
            for (int j = 0; j < meldCount; j++)
            {
                Card card = meld[j];
                if (remaining.Remove(card))
                    removed.Add(card);
            }
            OptimalResult subResult = ComputeOptimal(remaining, candidateMelds, memo);
            if (subResult.Deadwood < bestDeadwood)
            {
                bestDeadwood = subResult.Deadwood;
                bestMelds = new List<List<Card>>(subResult.Melds);
                bestMelds.Add(meld);
            }
            remaining.AddRange(removed);
        }
        OptimalResult result = new OptimalResult { Deadwood = bestDeadwood, Melds = bestMelds };
        memo[key] = result;
        return result;
    }

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

    private int SumPoints(List<Card> cards)
    {
        int sum = 0;
        int count = cards.Count;
        for (int i = 0; i < count; i++)
            sum += cards[i].GetPoint();
        return sum;
    }

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
        return (sum * 397) ^ xor;
    }
}
