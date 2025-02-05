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
   //private List<int> chainIndices = new List<int>(10); //doldur boşalt array'i
    private List<Card> ungroupedCards = new List<Card>();

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
        Debug.Log("Deck, suit'e göre ve aynı suit içinde küçükten büyüğe sıralandı.");
        MarkGroups();
        RepositionCards();
    }

    public void SortAndRepositionDeckByNumberThenSuitAscending()
    {
        myDeck.Sort(CompareByNumberThenSuitAscending);
        Debug.Log("Deck, number'a göre ve eşit sayılar suit'e göre sıralandı.");
        MarkGroups();
        RepositionCards();
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
 
    private int CompareBySuitThenNumberAscending(Card cardA, Card cardB)
    {
        int suitDiff = cardA.GetCardData().Suit - cardB.GetCardData().Suit;
        if (suitDiff != 0)
            return suitDiff;
        return cardA.GetCardData().Number - cardB.GetCardData().Number;
    }
    private int CompareByNumberThenSuitAscending(Card cardA, Card cardB)
    {
        int diff = cardA.GetCardData().Number - cardB.GetCardData().Number;
        if (diff != 0)
            return diff;
        return cardA.GetCardData().Suit - cardB.GetCardData().Suit;
    }
  
    public void MarkGroups()
    {
        int n = myDeck.Count;
        for (int idx = 0; idx < n; idx++)
        {
            myDeck[idx].SetGroupID(0);
        }
        int i = 0;
        byte groupId = 1; // Grup ID başlangıcı

        while (i < n)
        {
            if (i < n - 1 && myDeck[i].GetCardData().Suit == myDeck[i + 1].GetCardData().Suit)
            {
                int chainLength = 1;
                while (i + chainLength < n &&
                       myDeck[i + chainLength].GetCardData().Suit == myDeck[i].GetCardData().Suit &&
                       myDeck[i + chainLength].GetCardData().Number == myDeck[i + chainLength - 1].GetCardData().Number + 1)
                {
                    chainLength++;
                }
                if (chainLength >= 3)
                {
                    for (int k = i; k < i + chainLength; k++)
                    {
                        myDeck[k].SetGroupID(groupId);
                    }
                    Debug.Log($"Suit grubu: Suit {myDeck[i].GetCardData().Suit}, başlangıç numarası {myDeck[i].GetCardData().Number}, zincir uzunluğu {chainLength} -> GroupID: {groupId}");
                    groupId++;
                    i += chainLength;
                }
            }
            if (myDeck[i].GetCardData().GroupID==0 && i < n - 1 && myDeck[i].GetCardData().Number == myDeck[i + 1].GetCardData().Number)//bool silip group id sıfır sa grupta değil check'i 
            {
                int chainLength = 1;
                while (i + chainLength < n &&
                       myDeck[i + chainLength].GetCardData().Number == myDeck[i].GetCardData().Number)
                {
                    chainLength++;
                }
                if (chainLength >= 3)
                {
                    for (int k = i; k < i + chainLength; k++)
                    {
                        myDeck[k].SetGroupID(groupId);
                    }
                    Debug.Log($"Number grubu: Number {myDeck[i].GetCardData().Number}, zincir uzunluğu {chainLength} -> GroupID: {groupId}");
                    groupId++;
                    i += chainLength;
                    continue;
                }
            }
            i++;
        }
        UpdateUngroupedCards();
    }
    private void UpdateUngroupedCards()
    {
        ungroupedCards.Clear();
        int n = myDeck.Count;
        int totalPoints = 0;
        for (int idx = 0; idx < n; idx++)
        {
            if (myDeck[idx].GetCardData().GroupID == 0)
            {
                ungroupedCards.Add(myDeck[idx]);
                totalPoints += myDeck[idx].GetPoint(); //puan hesapla
            }
        }
        Debug.Log($"Ungrouped Cards güncellendi: {ungroupedCards.Count} kart bulunuyor. Toplam puan: {totalPoints}");
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
