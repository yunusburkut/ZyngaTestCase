using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MyDeckManager : MonoBehaviour
{
    [SerializeField] private List<Card> myDeck = new List<Card>();
    [SerializeField] private float cardSpacing = 30f;
    [SerializeField] private float shiftDuration = 0.5f;
    [SerializeField] private float spacing = 50;
    [SerializeField] private float moveDuration = 0.5f;
    
    public static MyDeckManager Instance;
    void Awake()
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
            Debug.Log("Kart eklendi: Number=" + card.GetCardData().Number +
                      ", Suit=" + card.GetCardData().Suit);
            UpdateDeckLayout();
        }
        else
        {
            Debug.LogWarning("Eklenmeye çalışılan kart null!");
        }
    }

    public void UpdateDeckLayout()
    {
        int n = myDeck.Count;
        if (n == 0) return;

        float startX = -((n - 1) * cardSpacing) / 2f;

        for (int i = 0; i < n; i++)
        {
            RectTransform rt = myDeck[i].GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 targetPos = new Vector2(startX + i * cardSpacing, 0);
                rt.DOAnchorPos(targetPos, shiftDuration).SetEase(Ease.OutQuad);
            }
            else
            {
                Debug.LogWarning("Kartın RectTransform'u bulunamadı!");
            }
        }
    }

    public void SortAndRepositionDeckBySuitAscending()
    {
        myDeck.Sort(CompareBySuitThenNumberAscending);
        Debug.Log("Deck, Suit'e göre ve aynı suit içinde küçükten büyüğe sıralandı.");
        MarkConsecutiveGroupsBySuitAscending();
        RepositionCards();
    }

    private int CompareBySuitThenNumberAscending(Card cardA, Card cardB)
    {
        int suitDiff = cardA.GetCardData().Suit - cardB.GetCardData().Suit;
        if (suitDiff != 0)
            return suitDiff;
        return cardA.GetCardData().Number - cardB.GetCardData().Number;
    }

    private void MarkConsecutiveGroupsBySuitAscending()
    {
        int n = myDeck.Count;
        int i = 0;
        byte groupId = 1; 

        while (i < n)
        {
            int currentSuit = myDeck[i].GetCardData().Suit;
            int j = i;
            while (j < n && myDeck[j].GetCardData().Suit == currentSuit)
            {
                j++;
            }
            int chainStart = i;  
            int chainLength = 1; 
            for (int k = i + 1; k < j; k++)
            {
                if (myDeck[k].GetCardData().Number == myDeck[k - 1].GetCardData().Number + 1)
                {
                    chainLength++;
                }
                else
                {
                    if (chainLength >= 3)
                    {
                        for (int idx = chainStart; idx < chainStart + chainLength; idx++)
                        {
                            myDeck[idx].SetGroupID(groupId);
                        }
                        Debug.Log($"Suit {currentSuit} için ardışık grup (ilk numara: {myDeck[chainStart].GetCardData().Number}) GroupID: {groupId} atandı.");
                        groupId++; 
                    }
                    chainStart = k;
                    chainLength = 1;
                }
            }
            if (chainLength >= 3)
            {
                for (int idx = chainStart; idx < chainStart + chainLength; idx++)
                {
                    myDeck[idx].SetGroupID(groupId);
                }
                Debug.Log($"Suit {currentSuit} için ardışık grup (ilk numara: {myDeck[chainStart].GetCardData().Number}) GroupID: {groupId} atandı.");
                groupId++;
            }
            i = j;
        }
    }

    private void RepositionCards()
    {
        int cardCount = myDeck.Count;
        float startX = -((cardCount - 1) * spacing) / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            
            RectTransform cardRect = myDeck[i].GetComponent<RectTransform>();
            if (cardRect == null)
            {
                Debug.LogWarning("Kartın RectTransform'u bulunamadı!");
                continue;
            }

            Vector2 targetPos = new Vector2(startX + i * spacing, 0f);
            cardRect.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
            cardRect.transform.SetSiblingIndex(i);
        }
    }

    public void LogDeck()
    {
        string deckInfo = "MyDeck: ";
        for (int i = 0; i < myDeck.Count; i++)
        {
            CardData data = myDeck[i].GetCardData();
            deckInfo += $"[{i}] Number:{data.Number}, Suit:{data.Suit}  ";
        }
        Debug.Log(deckInfo);
    }
}
