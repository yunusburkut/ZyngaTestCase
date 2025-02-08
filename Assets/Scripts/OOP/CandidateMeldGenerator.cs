using System.Collections.Generic;

public static class CandidateMeldGenerator
{
    public static List<List<Card>> GenerateCandidateMelds(List<Card> deck)
    {
        List<List<Card>> candidates = new List<List<Card>>();
        int n = deck.Count;

        // Run'ler: Suit bazında gruplama.
        Dictionary<int, List<Card>> suitGroups = new Dictionary<int, List<Card>>();
        foreach (Card card in deck)
        {
            int suit = card.GetCardData().Suit;
            if (!suitGroups.ContainsKey(suit))
                suitGroups[suit] = new List<Card>();
            suitGroups[suit].Add(card);
        }

        foreach (var kvp in suitGroups)
        {
            List<Card> cardsOfSuit = kvp.Value;
            cardsOfSuit.Sort((a, b) => a.GetCardData().Number.CompareTo(b.GetCardData().Number));
            for (int i = 0; i < cardsOfSuit.Count; i++)
            {
                int start = i;
                int end = i;
                while (end + 1 < cardsOfSuit.Count && 
                       cardsOfSuit[end + 1].GetCardData().Number == cardsOfSuit[end].GetCardData().Number + 1)
                {
                    end++;
                }
                int runLength = end - start + 1;
                if (runLength >= 3)
                {
                    for (int runSize = 3; runSize <= runLength; runSize++)
                    {
                        for (int j = start; j <= end - runSize + 1; j++)
                        {
                            List<Card> meld = new List<Card>();
                            for (int k = 0; k < runSize; k++)
                                meld.Add(cardsOfSuit[j + k]);
                            candidates.Add(meld);
                        }
                    }
                }
                i = end;
            }
        }

        // Set'ler: Aynı numaralı kartlar.
        Dictionary<int, List<Card>> numberGroups = new Dictionary<int, List<Card>>();
        foreach (Card card in deck)
        {
            int number = card.GetCardData().Number;
            if (!numberGroups.ContainsKey(number))
                numberGroups[number] = new List<Card>();
            numberGroups[number].Add(card);
        }
        foreach (var kvp in numberGroups)
        {
            List<Card> cardsOfNumber = kvp.Value;
            if (cardsOfNumber.Count >= 3)
            {
                // 3'lü kombinasyonlar.
                var combinations3 = CombinationGenerator.GenerateCombinations(cardsOfNumber, 3);
                candidates.AddRange(combinations3);
                if (cardsOfNumber.Count >= 4)
                {
                    var combinations4 = CombinationGenerator.GenerateCombinations(cardsOfNumber, 4);
                    candidates.AddRange(combinations4);
                }
            }
        }

        return candidates;
    }
}
