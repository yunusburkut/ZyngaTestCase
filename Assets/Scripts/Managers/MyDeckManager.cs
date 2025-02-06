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
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
        if (n == 0) return;

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
        MarkGroups();
        RepositionCards();
    }

    public void SortAndRepositionDeckByNumberThenSuitAscending()
    {
        myDeck.Sort(CompareByNumberThenSuitAscending);
        MarkGroups();
        RepositionCards();
    }

    private void RepositionCards()
    {
        int cardCount = myDeck.Count;
        if (cardCount == 0) return;

        float startX = -((cardCount - 1) * spacing) / 2f;
        for (int i = 0; i < cardCount; i++)
        {
            Card card = myDeck[i];
            RectTransform cardRect = card.CachedRectTransform;
            if (cardRect == null)
                continue;

            Vector2 targetPos = new Vector2(startX + i * spacing, 0f);
            cardRect.DOKill();
            cardRect.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
            cardRect.transform.SetSiblingIndex(i);
        }
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
        {
            myDeck[i].SetGroupID(0);
        }

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
                        {
                            chainLength++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (chainLength >= 3)
                    {
                        for (int k = iIndex; k < iIndex + chainLength; k++)
                        {
                            myDeck[k].SetGroupID(groupId);
                        }
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
                        {
                            chainLength++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (chainLength >= 3)
                    {
                        for (int k = iIndex; k < iIndex + chainLength; k++)
                        {
                            myDeck[k].SetGroupID(groupId);
                        }
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
            }
        }
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
    }
}
