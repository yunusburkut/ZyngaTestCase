using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MyDeckManager : MonoBehaviour
{
    [SerializeField] private List<Card> myDeck = new List<Card>();
    [SerializeField] private float cardSpacing = 30f;
    [SerializeField] private float shiftDuration = 0.5f;
    
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

    public void SortAndRepositionDeckByNumber()
    {
        myDeck.Sort(CompareByNumber);
        Debug.Log("Deste, numaraya göre sıralandı.");
        RepositionCards();
    }

    public void SortAndRepositionDeckBySuit()
    {
        myDeck.Sort(CompareBySuit);
        Debug.Log("Deste, suit'e göre sıralandı.");
        RepositionCards();
    }

    private void RepositionCards()
    {
        int cardCount = myDeck.Count;
       
        float startX = -((cardCount - 1) * 50) / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            RectTransform cardRect = myDeck[i].GetComponent<RectTransform>();
            if (cardRect == null)
            {
                Debug.LogWarning("Kartın RectTransform'u bulunamadı!");
                continue;
            }
            Vector2 targetPos = new Vector2(startX + i * 50, 0f);
            cardRect.DOAnchorPos(targetPos, .5f).SetEase(Ease.OutQuad);
            cardRect.transform.SetSiblingIndex(i);
        }
    }

    
    private int CompareByNumber(Card cardA, Card cardB)
    {
        return cardA.GetCardData().Number.CompareTo(cardB.GetCardData().Number);
    }

    private int CompareBySuit(Card cardA, Card cardB)
    {
        return cardA.GetCardData().Suit.CompareTo(cardB.GetCardData().Suit);
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
