using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MyDeckManager : MonoBehaviour
{
    [SerializeField] private List<Card> myDeck = new List<Card>();
    [SerializeField] private float cardSpacing = 30f;
    [SerializeField] private float shiftDuration = 0.5f;
    [SerializeField] private float spacing = 50f;
    [SerializeField] private float moveDuration = 0.5f;
    
    private List<Card> ungroupedCards = new List<Card>();

    public static MyDeckManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddCard(Card card)
    {
        if (card != null)
        {
            myDeck.Add(card);
            UpdateDeckLayout();
        }
    }

    public void UpdateDeckLayout()
    {
        int n = myDeck.Count;
        if (n == 0)
            return;
        float startX = -((n - 1) * cardSpacing) / 2f;
        for (int i = 0; i < n; i++)
        {
            RectTransform rt = myDeck[i].CachedRectTransform;
            if (rt != null)
            {
                Vector2 targetPos = new Vector2(startX + i * cardSpacing, 0);
                rt.DOKill();
                rt.DOAnchorPos(targetPos, shiftDuration).SetEase(Ease.OutQuad);
            }
        }
    }

    public void SortAndRepositionDeckBySuitAscending()
    {
        myDeck.Sort(CompareBySuitThenNumberAscending);
    }

    public void SortAndRepositionDeckByNumberThenSuitAscending()
    {
        myDeck.Sort(CompareByNumberThenSuitAscending);
    }


public void RepositionCardsByGroup()
{
    Dictionary<int, List<Card>> groupedDict = new Dictionary<int, List<Card>>();
    List<Card> ungroupedCardsLocal = new List<Card>();

    foreach (Card card in myDeck)
    {
        int groupID = card.GetCardData().GroupID;
        if (groupID != 0)
        {
            if (!groupedDict.ContainsKey(groupID))
                groupedDict[groupID] = new List<Card>();
            groupedDict[groupID].Add(card);
        }
        else
        {
            ungroupedCardsLocal.Add(card);
        }
    }

    foreach (var kvp in groupedDict)
    {
        List<Card> groupList = kvp.Value;
        StableSortGroup(groupList);
    }

    List<int> groupIDs = new List<int>(groupedDict.Keys);
    groupIDs.Sort(); // Bu, int için stabil bir sıralama sağlar.
    List<Card> newOrder = new List<Card>();
    foreach (int gid in groupIDs)
    {
        newOrder.AddRange(groupedDict[gid]);
    }
    newOrder.AddRange(ungroupedCardsLocal);

    myDeck = newOrder;

    float startX = -((myDeck.Count - 1) * spacing) / 2f;
    for (int i = 0; i < myDeck.Count; i++)
    {
        Card card = myDeck[i];
    
        card.transform.SetSiblingIndex(i);

        RectTransform rt = card.CachedRectTransform;
        if (rt != null)
        {
            Vector2 targetPos = new Vector2(startX + i * spacing, 0f);
            rt.DOKill();
            rt.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
        }
    }
}

private void StableSortGroup(List<Card> group)
{
    for (int i = 1; i < group.Count; i++)
    {
        Card key = group[i];
        int j = i - 1;
        while (j >= 0 && CompareCardsWithinGroup(group[j], key) > 0)
        {
            group[j + 1] = group[j];
            j--;
        }
        group[j + 1] = key;
    }
}

private int CompareCardsWithinGroup(Card a, Card b)
{
    int numberDiff = a.GetCardData().Number.CompareTo(b.GetCardData().Number);
    if (numberDiff != 0)
        return numberDiff;
    return a.GetCardData().Suit.CompareTo(b.GetCardData().Suit);
}



    private int CompareBySuitThenNumberAscending(Card cardA, Card cardB)
    {
        var dataA = cardA.GetCardData();
        var dataB = cardB.GetCardData();
        int suitDiff = dataA.Suit - dataB.Suit;
        if (suitDiff != 0)
            return suitDiff;
        return dataA.Number - dataB.Number;
    }

    private int CompareByNumberThenSuitAscending(Card cardA, Card cardB)
    {
        var dataA = cardA.GetCardData();
        var dataB = cardB.GetCardData();
        int diff = dataA.Number - dataB.Number;
        if (diff != 0)
            return diff;
        return dataA.Suit - dataB.Suit;
    }

    public void MarkGroups()
    {
        int n = myDeck.Count;
        for (int i = 0; i < n; i++)
            myDeck[i].SetGroupID(0);
        int iIndex = 0;
        byte groupId = 1;
        while (iIndex < n)
        {
            bool grouped = false;
            var currentCard = myDeck[iIndex];
            var currentData = currentCard.GetCardData();
            if (iIndex < n - 1)
            {
                var nextData = myDeck[iIndex + 1].GetCardData();
                if (currentData.Suit == nextData.Suit)
                {
                    int chainLength = 1;
                    while (iIndex + chainLength < n)
                    {
                        var nextChainData = myDeck[iIndex + chainLength].GetCardData();
                        var prevChainData = myDeck[iIndex + chainLength - 1].GetCardData();
                        if (nextChainData.Suit == currentData.Suit &&
                            nextChainData.Number == prevChainData.Number + 1)
                            chainLength++;
                        else
                            break;
                    }
                    if (chainLength >= 3)
                    {
                        for (int k = iIndex; k < iIndex + chainLength; k++)
                            myDeck[k].SetGroupID(groupId);
                        groupId++;
                        iIndex += chainLength;
                        grouped = true;
                        continue;
                    }
                }
            }
            if (!grouped && iIndex < n - 1)
            {
                var nextData = myDeck[iIndex + 1].GetCardData();
                if (currentData.Number == nextData.Number)
                {
                    int chainLength = 1;
                    while (iIndex + chainLength < n)
                    {
                        var chainData = myDeck[iIndex + chainLength].GetCardData();
                        if (chainData.Number == currentData.Number)
                            chainLength++;
                        else
                            break;
                    }
                    if (chainLength >= 3)
                    {
                        for (int k = iIndex; k < iIndex + chainLength; k++)
                            myDeck[k].SetGroupID(groupId);
                        groupId++;
                        iIndex += chainLength;
                        continue;
                    }
                }
            }
            iIndex++;
        }
        UpdateUngroupedCards();
    }

    private void UpdateUngroupedCards()
    {
        ungroupedCards.Clear();
        int n = myDeck.Count;
        int totalPoints = 0;
        for (int i = 0; i < n; i++)
        {
            var data = myDeck[i].GetCardData();
            if (data.GroupID == 0)
            {
                ungroupedCards.Add(myDeck[i]);
                totalPoints += myDeck[i].GetPoint();
                Debug.Log(totalPoints);
            }
        }
    }

    public void OnCardDragEnd(Card draggedCard, Vector2 screenPosition, Camera eventCamera)
    {
        RectTransform deckRect = draggedCard.CachedRectTransform.parent as RectTransform;
        if (deckRect == null)
            return;

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(deckRect, screenPosition, eventCamera, out localPoint))
            return;

        float dropX = localPoint.x;
        int count = myDeck.Count;
        float startX = -((count - 1) * spacing) / 2f;
        int newIndex = Mathf.RoundToInt((dropX - startX) / spacing);
        newIndex = Mathf.Clamp(newIndex, 0, count - 1);

        if (myDeck.Contains(draggedCard))
            myDeck.Remove(draggedCard);
        myDeck.Insert(newIndex, draggedCard);
        MarkGroups();
        RepositionCardsByGroup();
    }

    public void UpdateCardsAnimationDuringDrag(Card draggedCard)
    {
        int count = myDeck.Count;
        if (count == 0)
            return;
        float startX = -((count - 1) * spacing) / 2f;
        float threshold = spacing * 0.5f;
        float offsetY = 20f;
        float draggedX = draggedCard.CachedRectTransform.anchoredPosition.x;
        for (int i = 0; i < count; i++)
        {
            Card card = myDeck[i];
            if (card == draggedCard)
                continue;
            RectTransform cardRect = card.CachedRectTransform;
            if (cardRect == null)
                continue;
            float defaultX = startX + i * spacing;
            float diff = Mathf.Abs(defaultX - draggedX);
            Vector2 targetPos = (diff < threshold) ? new Vector2(defaultX, offsetY) : new Vector2(defaultX, 0f);
            cardRect.DOKill();
            cardRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    public List<Card> GetMyDeck()
    {
        return myDeck;
    }
    public void LogDeck()
    {
        System.Text.StringBuilder deckInfo = new System.Text.StringBuilder("MyDeck: ");
        int n = myDeck.Count;
        for (int i = 0; i < n; i++)
        {
            var data = myDeck[i].GetCardData();
            deckInfo.Append($"[{i}] Number:{data.Number}, Suit:{data.Suit}  ");
        }
        Debug.Log(deckInfo.ToString());
    }

    public void RunHesapla()
    {
        foreach (var card in myDeck)
            card.SetGroupID(0);
        myDeck.Sort(CompareBySuitThenNumberAscending);

        byte groupId = 1;
        int n = myDeck.Count;
        int i = 0;
        while (i < n)
        {
            var currentCard = myDeck[i];
            var currentData = currentCard.GetCardData();
            int chainLength = 1;
            int j = i + 1;
            while (j < n)
            {
                var nextCard = myDeck[j];
                var nextData = nextCard.GetCardData();
                if (currentData.Suit == nextData.Suit && nextData.Number == currentData.Number + chainLength)
                {
                    chainLength++;
                    j++;
                }
                else
                {
                    break;
                }
            }
           
            if (chainLength >= 3)
            {
                for (int k = i; k < i + chainLength; k++)
                    myDeck[k].SetGroupID(groupId);
                groupId++;
                i += chainLength; 
            }
            else
            {
                i++;
            }
        }
        UpdateUngroupedCards();
        RepositionCardsByGroup();
    }
    public void SetHesapla()
    {
        foreach (var card in myDeck)
            card.SetGroupID(0);
        myDeck.Sort(CompareByNumberThenSuitAscending);

        byte groupId = 1;
        int n = myDeck.Count;
        int i = 0;
        while (i < n)
        {
            var currentCard = myDeck[i];
            var currentData = currentCard.GetCardData();
            int chainLength = 1;
            int j = i + 1;
            while (j < n)
            {
                var nextData = myDeck[j].GetCardData();
                if (nextData.Number == currentData.Number)
                {
                    chainLength++;
                    j++;
                }
                else
                {
                    break;
                }
            }
            if (chainLength >= 3)
            {
                for (int k = i; k < i + chainLength; k++)
                    myDeck[k].SetGroupID(groupId);
                groupId++;
                i += chainLength;
            }
            else
            {
                i++;
            }
        }
        UpdateUngroupedCards();
        RepositionCardsByGroup();
    }

    public void OptimalDeckHesapla()
    {
        foreach (var card in myDeck)
            card.SetGroupID(0);

        int n = myDeck.Count;
        List<int> candidateMelds = GenerateCandidateMelds();

        Dictionary<int, DPResult> memo = new Dictionary<int, DPResult>();
        int fullMask = (1 << n) - 1;
        DPResult optimal = ComputeOptimalMelds(fullMask, candidateMelds, n, memo);

        byte groupId = 1;
        foreach (int meldMask in optimal.meldMasks)
        {
            for (int i = 0; i < n; i++)
            {
                if ((meldMask & (1 << i)) != 0)
                    myDeck[i].SetGroupID(groupId);
            }
            groupId++;
        }

        UpdateUngroupedCards();
        RepositionCardsByGroup();

        Debug.Log("Optimal Deadwood Puanı: " + optimal.deadwood);
    }

    
    private class DPResult
    {
        public int deadwood;           
        public List<int> meldMasks;    
    }

    private int SumPoints(int mask, int n)
    {
        int sum = 0;
        for (int i = 0; i < n; i++)
        {
            if ((mask & (1 << i)) != 0)
                sum += myDeck[i].GetPoint();
        }
        return sum;
    }
    private DPResult ComputeOptimalMelds(int mask, List<int> candidateMelds, int n, Dictionary<int, DPResult> memo)
    {
        if (mask == 0)
            return new DPResult { deadwood = 0, meldMasks = new List<int>() };

        if (memo.ContainsKey(mask))
            return memo[mask];

        int bestDeadwood = SumPoints(mask, n);
        List<int> bestMelds = new List<int>(); 

        foreach (int meld in candidateMelds)
        {
            if ((mask & meld) == meld)
            {
                int remainingMask = mask & ~meld; 
                DPResult subResult = ComputeOptimalMelds(remainingMask, candidateMelds, n, memo);
                if (subResult.deadwood < bestDeadwood)
                {
                    bestDeadwood = subResult.deadwood;
                    bestMelds = new List<int>(subResult.meldMasks);
                    bestMelds.Add(meld);
                }
            }
        }

        DPResult result = new DPResult { deadwood = bestDeadwood, meldMasks = bestMelds };
        memo[mask] = result;
        return result;
    }

    private List<int> GenerateCandidateMelds()
    {
        List<int> candidates = new List<int>();
        int n = myDeck.Count;

        Dictionary<int, List<int>> suitGroups = new Dictionary<int, List<int>>();
        for (int i = 0; i < n; i++)
        {
            int suit = myDeck[i].GetCardData().Suit;
            if (!suitGroups.ContainsKey(suit))
                suitGroups[suit] = new List<int>();
            suitGroups[suit].Add(i);
        }
        foreach (var kvp in suitGroups)
        {
            List<int> indices = kvp.Value;
            indices.Sort((a, b) => myDeck[a].GetCardData().Number.CompareTo(myDeck[b].GetCardData().Number));
            for (int i = 0; i < indices.Count; i++)
            {
                int start = i;
                int end = i; 
                while (end + 1 < indices.Count &&
                       myDeck[indices[end + 1]].GetCardData().Number == myDeck[indices[end]].GetCardData().Number + 1)
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
                            int meldMask = 0;
                            for (int k = 0; k < runSize; k++)
                            {
                                int cardIndex = indices[j + k];
                                meldMask |= (1 << cardIndex);
                            }
                            candidates.Add(meldMask);
                        }
                    }
                }
                i = end; 
            }
        }

        Dictionary<int, List<int>> numberGroups = new Dictionary<int, List<int>>();
        for (int i = 0; i < n; i++)
        {
            int number = myDeck[i].GetCardData().Number;
            if (!numberGroups.ContainsKey(number))
                numberGroups[number] = new List<int>();
            numberGroups[number].Add(i);
        }
        foreach (var kvp in numberGroups)
        {
            List<int> indices = kvp.Value;
            if (indices.Count >= 3)
            {
                List<int> meldCombinationMasks = new List<int>();
                GenerateCombinations(indices, 0, indices.Count, 3, 0, meldCombinationMasks);
                if (indices.Count >= 4)
                    GenerateCombinations(indices, 0, indices.Count, 4, 0, meldCombinationMasks);
                candidates.AddRange(meldCombinationMasks);
            }
        }
        return candidates;
    }

    private void GenerateCombinations(List<int> indices, int start, int n, int targetCount, int currentMask, List<int> results)
    {
        if (targetCount == 0)
        {
            results.Add(currentMask);
            return;
        }
        for (int i = start; i < n; i++)
        {
            int newMask = currentMask | (1 << indices[i]);
            GenerateCombinations(indices, i + 1, n, targetCount - 1, newMask, results);
        }
    }
}
