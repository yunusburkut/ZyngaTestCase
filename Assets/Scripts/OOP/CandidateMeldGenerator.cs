using System.Collections.Generic;

public class CandidateMeldGenerator
{
    private readonly List<List<Card>> _candidatesCache = new List<List<Card>>();
    private readonly Dictionary<int, List<Card>> _suitGroupsCache = new Dictionary<int, List<Card>>();
    private readonly Dictionary<int, List<Card>> _numberGroupsCache = new Dictionary<int, List<Card>>();
    private readonly List<int> _sortedGroupIDsCache = new List<int>();

    public List<List<Card>> GenerateCandidateMelds(List<Card> deck)
    {
        _candidatesCache.Clear();
        _suitGroupsCache.Clear();
        _numberGroupsCache.Clear();
        _sortedGroupIDsCache.Clear();

        int n = deck.Count;

        for (int i = 0; i < n; i++)
        {
            int suit = deck[i].GetCardData().Suit;
            if (!_suitGroupsCache.TryGetValue(suit, out List<Card> group))
            {
                group = new List<Card>();
                _suitGroupsCache.Add(suit, group);
            }
            group.Add(deck[i]);
        }

        foreach (var pair in _suitGroupsCache)
        {
            List<Card> cardsOfSuit = pair.Value;
            cardsOfSuit.Sort(CardComparer.CompareBySuitThenNumber);

            for (int i = 0; i < cardsOfSuit.Count; i++)
            {
                int start = i;
                int end = i;
                while (end + 1 < cardsOfSuit.Count &&
                       cardsOfSuit[end + 1].GetCardData().Number ==
                       cardsOfSuit[end].GetCardData().Number + 1)
                {
                    end++;
                }
                int runLength = end - start + 1;
                if (runLength >= 3)
                {
                    for (int meldSize = 3; meldSize <= runLength; meldSize++)
                    {
                        for (int j = start; j <= end - meldSize + 1; j++)
                        {
                            List<Card> meld = new List<Card>(meldSize);
                            for (int k = 0; k < meldSize; k++)
                            {
                                meld.Add(cardsOfSuit[j + k]);
                            }
                            _candidatesCache.Add(meld);
                        }
                    }
                }
                i = end;
            }
        }

        for (int i = 0; i < n; i++)
        {
            int number = deck[i].GetCardData().Number;
            if (!_numberGroupsCache.TryGetValue(number, out List<Card> group))
            {
                group = new List<Card>();
                _numberGroupsCache.Add(number, group);
            }
            group.Add(deck[i]);
        }

        foreach (var pair in _numberGroupsCache)
        {
            List<Card> cardsOfNumber = pair.Value;
            cardsOfNumber.Sort(CardComparer.CompareByNumberThenSuit);
            if (cardsOfNumber.Count >= 3)
            {
                List<List<Card>> comb3 = CombinationGenerator.GenerateCombinations(cardsOfNumber, 3);
                for (int i = 0; i < comb3.Count; i++)
                {
                    _candidatesCache.Add(comb3[i]);
                }
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

        return _candidatesCache;
    }
}
