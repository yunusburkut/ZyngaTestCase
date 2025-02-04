using UnityEngine;
using System.Collections.Generic;

public class MyDeckManager : MonoBehaviour
{
    [SerializeField] private List<Card> myDeck = new List<Card>();
    
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
        }
        else
        {
            Debug.LogWarning("Eklenmeye çalışılan kart null!");
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
