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

    public void SortDeckByNumber()
    {
        myDeck.Sort((cardA, cardB) => cardA.GetCardData().Number.CompareTo(cardB.GetCardData().Number));
        Debug.Log("Deste, numaraya göre sıralandı.");
    }

    public void SortDeckBySuit()
    {
        myDeck.Sort((cardA, cardB) => cardA.GetCardData().Suit.CompareTo(cardB.GetCardData().Suit));
        Debug.Log("Deste, suit'e göre sıralandı.");
    }

    public void SwapCards(int indexA, int indexB)
    {
        if (indexA >= 0 && indexA < myDeck.Count && indexB >= 0 && indexB < myDeck.Count)
        {
            (myDeck[indexA], myDeck[indexB]) = (myDeck[indexB], myDeck[indexA]);
            Debug.Log("Kartlar yer değiştirdi: " + indexA + " ile " + indexB);
        }
        else
        {
            Debug.LogWarning("Swap işlemi için geçersiz indeksler!");
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
