#if UNITY_EDITOR
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupCalculator_SortingTests
{
    private Card CreateTestCard(CardData data)
    {
        GameObject go = new GameObject("TestCard");
        go.AddComponent<Canvas>();
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Image>();
        Card card = go.AddComponent<Card>();
        card.SetCardData(data);
        return card;
    }

    private void ShuffleDeck<T>(List<T> list)
    {
        System.Random rnd = new System.Random();
        int count = list.Count;
        for (int i = count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

   [Test]
    public void CalculateRun_SortsDeckAndAssignsGroupIDs_Correctly()
    {
        // Test deckini oluşturuyoruz.
        List<Card> deck = new List<Card>
        {
            CreateTestCard(new CardData(8, 2, 0)),
            CreateTestCard(new CardData(3, 1, 0)),
            CreateTestCard(new CardData(10,2, 0)),
            CreateTestCard(new CardData(2, 1, 0)),
            CreateTestCard(new CardData(7, 1, 0)),
            CreateTestCard(new CardData(4, 1, 0)),
            CreateTestCard(new CardData(5, 1, 0)),
            CreateTestCard(new CardData(9, 1, 0)),
            CreateTestCard(new CardData(4, 2, 0)),
            CreateTestCard(new CardData(7, 2, 0))
        };

        // Beklenen sıralama (CalculateRun, suit ve numara bazında):
        // Suit 1 kartları: (2,1), (3,1), (4,1), (5,1), (7,1), (8,1), (9,1)
        // Suit 2 kartları: (4,2), (7,2), (10,2)
        List<(byte number, byte suit)> expectedOrderRun = new List<(byte, byte)>
        {
            (2,1), (3,1), (4,1), (5,1), (7,1), (8,1), (9,1),
            (4,2), (7,2), (10,2)
        };

        // Deck'i rastgele karıştıralım.
        ShuffleDeck(deck);

        // CalculateRun metodunu çalıştırıyoruz.
        GroupCalculator gc = new GroupCalculator();
        gc.CalculateRun(deck);

        // Öncelikle, deck'in sıralamasını kontrol ediyoruz.
        Assert.AreEqual(expectedOrderRun.Count, deck.Count, "Deck eleman sayısı beklenenle aynı olmalı.");
        for (int i = 0; i < deck.Count; i++)
        {
            CardData data = deck[i].GetCardData();
            Assert.AreEqual(expectedOrderRun[i].number, data.Number, $"Index {i}: Beklenen numara {expectedOrderRun[i].number}, bulundu {data.Number}");
            Assert.AreEqual(expectedOrderRun[i].suit, data.Suit, $"Index {i}: Beklenen suit {expectedOrderRun[i].suit}, bulundu {data.Suit}");
        }

        // Şimdi GroupID atamasını kontrol edelim.
        // Suit 1 kartlarında iki run oluşturulmuş olmalı.
        // Beklentimiz:
        // - İlk run: (2,3,4) → Tüm kartlar aynı non-zero GroupID'ye sahip.
        // - İkinci run: (5,7,8,9) veya (5,7,8,9) gibi; test deck'imize göre:
        //   Beklenen suit 1 sıralaması: (2,3,4,5,7,8,9)
        //   İlk 3 kart run olarak (2,3,4) GroupID = X (non-zero)
        //   Kalan kartlar run olarak (5,7,8,9) farklı GroupID (non-zero ve X'ten farklı)
        List<Card> suit1Cards = deck.FindAll(card => card.GetCardData().Suit == 1);
        Assert.GreaterOrEqual(suit1Cards.Count, 7, "Suit 1 kart sayısı yetersiz.");

        // İlk run: ilk 3 kart (2,3,4)
        byte groupIdFirst = suit1Cards[0].GetCardData().GroupID;
        Assert.AreNotEqual(0, groupIdFirst, "İlk run GroupID 0 olmamalı.");
        Assert.AreEqual(2, suit1Cards[0].GetCardData().Number, "İlk runun ilk elemanı 2 olmalı.");
        Assert.AreEqual(3, suit1Cards[1].GetCardData().Number, "İlk runun ikinci elemanı 3 olmalı.");
        Assert.AreEqual(4, suit1Cards[2].GetCardData().Number, "İlk runun üçüncü elemanı 4 olmalı.");
        Assert.AreEqual(groupIdFirst, suit1Cards[0].GetCardData().GroupID);
        Assert.AreEqual(groupIdFirst, suit1Cards[1].GetCardData().GroupID);
        Assert.AreEqual(groupIdFirst, suit1Cards[2].GetCardData().GroupID);

        // İkinci run: kalan Suit 1 kartları (5,7,8,9) – bu runun minimum uzunluğu 3 olması gerekir.
        if (suit1Cards.Count >= 7)
        {
            byte groupIdSecond = suit1Cards[3].GetCardData().GroupID;
            Assert.AreNotEqual(0, groupIdSecond, "İkinci run GroupID 0 olmamalı.");
            Assert.AreNotEqual(groupIdFirst, groupIdSecond, "İkinci runun GroupID'si, ilk runun GroupID'sinden farklı olmalı.");
            // Beklenen olarak, ikinci runun ilk elemanı 5, ikinci 7, üçüncü 8, dördüncü 9.
            Assert.AreEqual(5, suit1Cards[3].GetCardData().Number, "İkinci runun ilk elemanı 5 olmalı.");
            Assert.AreEqual(7, suit1Cards[4].GetCardData().Number, "İkinci runun ikinci elemanı 7 olmalı.");
            Assert.AreEqual(8, suit1Cards[5].GetCardData().Number, "İkinci runun üçüncü elemanı 8 olmalı.");
            Assert.AreEqual(9, suit1Cards[6].GetCardData().Number, "İkinci runun dördüncü elemanı 9 olmalı.");
        }

        // Suit 2 kartlarında run oluşmadığından, GroupID 0 kalmalı.
        List<Card> suit2Cards = deck.FindAll(card => card.GetCardData().Suit == 2);
        foreach (Card card in suit2Cards)
        {
            Assert.AreEqual(0, card.GetCardData().GroupID, $"Suit 2 kartı (number: {card.GetCardData().Number}) run'e dahil olmamalı.");
        }

        // Temizlik: Oluşturulan tüm GameObject'leri yok edelim.
        foreach (Card card in deck)
        {
            Object.DestroyImmediate(card.gameObject);
        }
    }

    [Test]
    public void CalculateSet_SortsDeckAndAssignsGroupIDs_Correctly()
    {
       
        List<Card> deck = new List<Card>
        {
            CreateTestCard(new CardData(8, 1, 0)),
            CreateTestCard(new CardData(8, 2, 0)),
            CreateTestCard(new CardData(8, 3, 0)),
            CreateTestCard(new CardData(3, 1, 0)),
            CreateTestCard(new CardData(5, 1, 0)),
            CreateTestCard(new CardData(7, 2, 0)),
            CreateTestCard(new CardData(9, 2, 0)),
            CreateTestCard(new CardData(2, 1, 0)),
            CreateTestCard(new CardData(4, 1, 0)),
            CreateTestCard(new CardData(10,2, 0))
        };
        
        List<(byte number, byte suit)> expectedOrderSet = new List<(byte, byte)>
        {
            (2,1), (3,1), (4,1), (5,1), (7,2), (8,1), (8,2), (8,3), (9,2), (10,2)
        };

        ShuffleDeck(deck);

        GroupCalculator gc = new GroupCalculator();
        gc.CalculateSet(deck);

        Assert.AreEqual(expectedOrderSet.Count, deck.Count, "Deck eleman sayısı beklenenle aynı olmalı.");
        for (int i = 0; i < deck.Count; i++)
        {
            CardData data = deck[i].GetCardData();
            Assert.AreEqual(expectedOrderSet[i].number, data.Number, $"Index {i}: Beklenen numara {expectedOrderSet[i].number}, bulundu {data.Number}");
            Assert.AreEqual(expectedOrderSet[i].suit, data.Suit, $"Index {i}: Beklenen suit {expectedOrderSet[i].suit}, bulundu {data.Suit}");
        }

        List<Card> setCards = deck.FindAll(card => card.GetCardData().Number == 8);
        Assert.AreEqual(3, setCards.Count, "Deck'te 8 numaralı 3 kart bulunmalı.");
        foreach (Card card in setCards)
        {
            Assert.That(card.GetCardData().GroupID, Is.Not.EqualTo(0), $"8 numaralı kart meld'e dahil olmalı.");
        }

        List<Card> otherCards = deck.FindAll(card => card.GetCardData().Number != 8);
        foreach (Card card in otherCards)
        {
            Assert.That(card.GetCardData().GroupID, Is.EqualTo(0), $"Numarası 8 olmayan kart meld'e dahil olmamalı.");
        }
        
        foreach (Card card in deck)
        {
            Object.DestroyImmediate(card.gameObject);
        }
    }
    [Test]
    public void ComputeOptimalMelds_PerfectDeck_ReturnsZeroDeadwoodAndExpectedMeldCount()
    {
        List<Card> deck = new List<Card>
        {
            CreateTestCard(new CardData(3, 1, 0)),
            CreateTestCard(new CardData(4, 1, 0)),
            CreateTestCard(new CardData(5, 1, 0)),
            CreateTestCard(new CardData(7, 1, 0)),
            CreateTestCard(new CardData(7, 2, 0)),
            CreateTestCard(new CardData(7, 3, 0)),
            CreateTestCard(new CardData(8, 3, 0)),
            CreateTestCard(new CardData(9, 3, 0)),
            CreateTestCard(new CardData(2, 1, 0)),
            CreateTestCard(new CardData(7, 4, 0))
        };

        MeldOptimizer optimizer = new MeldOptimizer();
        MeldOptimizer.OptimalResult result = optimizer.ComputeOptimalMelds(deck);

        Assert.AreEqual(0, result.Deadwood, "Optimal deadwood should be 0 for a perfect deck.");
        Assert.AreEqual(3, result.Melds.Count, "Expected meld count is 2 for a perfect deck.");

        foreach (Card card in deck)
        {
            Object.DestroyImmediate(card.gameObject);
        }
    }
}
#endif