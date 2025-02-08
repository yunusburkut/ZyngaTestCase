using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MyDeckManagerSort10CardsTests
{
    private GameObject deckManagerGO;
    private MyDeckManager deckManager;

    [SetUp]
    public void SetUp()
    {
        // Test ortamı için MyDeckManager'ın bulunduğu GameObject oluşturuluyor.
        deckManagerGO = new GameObject("DeckManager");
        deckManager = deckManagerGO.AddComponent<MyDeckManager>();

        // Singleton instance ayarlanıyor.
        MyDeckManager.Instance = deckManager;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(deckManagerGO);
        MyDeckManager.Instance = null;
    }

    // Yardımcı metod: Belirtilen number ve suit değerlerine sahip test kartı oluşturur.
    private Card CreateTestCard(int number, int suit)
    {
        GameObject cardGO = new GameObject($"TestCard_{number}_{suit}");
        // Kartların parent'ı, deckManagerGO olarak ayarlanıyor.
        cardGO.transform.SetParent(deckManagerGO.transform);

        // Gerekli component'lar ekleniyor.
        cardGO.AddComponent<RectTransform>();
        cardGO.AddComponent<Image>();
        Card card = cardGO.AddComponent<Card>();

        // Texture ve sprite oluştururken Rect'in texture boyutlarıyla uyumlu olduğuna dikkat ediyoruz.
        Texture2D texture = new Texture2D(10, 10);
        Sprite dummySprite = Sprite.Create(texture, new Rect(0, 0, 10, 10), new Vector2(0.5f, 0.5f));

        CardData data = new CardData((byte)number, (byte)suit, 0);
        card.Initialize(data, dummySprite);

        return card;
    }

    [Test]
    public void SortAndRepositionDeckBySuitAscending_Sorts10CardsCorrectly()
    {
        /*
         Oluşturulan 10 kart:
         - Kart1: (7, 2)
         - Kart2: (5, 1)
         - Kart3: (9, 1)
         - Kart4: (4, 3)
         - Kart5: (3, 2)
         - Kart6: (8, 1)
         - Kart7: (2, 3)
         - Kart8: (6, 2)
         - Kart9: (10, 1)
         - Kart10: (1, 3)
         
         Beklenen sıralama (önce suit, sonra number ascending):
         Suit 1 kartları (sırasıyla): (5,1), (8,1), (9,1), (10,1)
         Suit 2 kartları: (3,2), (6,2), (7,2)
         Suit 3 kartları: (1,3), (2,3), (4,3)
         
         Dolayısıyla, beklenen nihai sıralama index'e göre:
         Index 0: (5,1)
         Index 1: (8,1)
         Index 2: (9,1)
         Index 3: (10,1)
         Index 4: (3,2)
         Index 5: (6,2)
         Index 6: (7,2)
         Index 7: (1,3)
         Index 8: (2,3)
         Index 9: (4,3)
        */

        // Kartları oluşturup ekliyoruz (oluşturma sırası karışık, sıralama metodunun işi sıralamak olacak).
        Card card1 = CreateTestCard(7, 2);    // (7,2)
        Card card2 = CreateTestCard(5, 1);    // (5,1)
        Card card3 = CreateTestCard(9, 1);    // (9,1)
        Card card4 = CreateTestCard(4, 3);    // (4,3)
        Card card5 = CreateTestCard(3, 2);    // (3,2)
        Card card6 = CreateTestCard(8, 1);    // (8,1)
        Card card7 = CreateTestCard(2, 3);    // (2,3)
        Card card8 = CreateTestCard(6, 2);    // (6,2)
        Card card9 = CreateTestCard(10, 1);   // (10,1)
        Card card10 = CreateTestCard(1, 3);   // (1,3)

        // Kartlar, MyDeckManager'ın myDeck listesine ekleniyor.
        deckManager.AddCard(card1);
        deckManager.AddCard(card2);
        deckManager.AddCard(card3);
        deckManager.AddCard(card4);
        deckManager.AddCard(card5);
        deckManager.AddCard(card6);
        deckManager.AddCard(card7);
        deckManager.AddCard(card8);
        deckManager.AddCard(card9);
        deckManager.AddCard(card10);

        // Sıralama metodunu çağırıyoruz.
        deckManager.SortDeckByNumberThenSuit();

        // deckManagerGO altındaki kartların sıralı hali, sibling index'e göre belirlenecektir.
        Card[] cardsInParent = deckManagerGO.GetComponentsInChildren<Card>();
        Assert.AreEqual(10, cardsInParent.Length, "Eklenen kart sayısı 10 olmalı.");

        // Kartları sibling index'e göre sıralayalım.
        List<(Card card, int siblingIndex)> cardIndices = new List<(Card, int)>();
        foreach (var card in cardsInParent)
        {
            int sibIndex = card.transform.GetSiblingIndex();
            cardIndices.Add((card, sibIndex));
        }
        cardIndices.Sort((a, b) => a.siblingIndex.CompareTo(b.siblingIndex));

        // Beklenen sıralama:
        // Index 0: (5,1)
        // Index 1: (8,1)
        // Index 2: (9,1)
        // Index 3: (10,1)
        // Index 4: (3,2)
        // Index 5: (6,2)
        // Index 6: (7,2)
        // Index 7: (1,3)
        // Index 8: (2,3)
        // Index 9: (4,3)

        // Sırasıyla kontrol edelim:
        CardData[] expectedOrder = new CardData[10];
        expectedOrder[0] = new CardData(5, 1, 0);
        expectedOrder[1] = new CardData(8, 1, 1);
        expectedOrder[2] = new CardData(9, 1, 1);
        expectedOrder[3] = new CardData(10, 1, 1);
        expectedOrder[4] = new CardData(3, 2, 0);
        expectedOrder[5] = new CardData(6, 2, 0);
        expectedOrder[6] = new CardData(7, 2, 0);
        expectedOrder[7] = new CardData(1, 3, 0);
        expectedOrder[8] = new CardData(2, 3, 0);
        expectedOrder[9] = new CardData(4, 3, 0);

        for (int i = 0; i < cardIndices.Count; i++)
        {
            CardData actual = cardIndices[i].card.GetCardData();
            CardData expected = expectedOrder[i];

            Assert.AreEqual(expected.GroupID, actual.GroupID, $"Index {i}: Beklenen Suit {expected.Suit} fakat bulundu {actual.Suit}");
            Assert.AreEqual(expected.Suit, actual.Suit, $"Index {i}: Beklenen Suit {expected.Suit} fakat bulundu {actual.Suit}");
            Assert.AreEqual(expected.Number, actual.Number, $"Index {i}: Beklenen Number {expected.Number} fakat bulundu {actual.Number}");
        }
    }
    [Test]
    public void SortAndRepositionDeckByNumberThenSuitAscending_Sorts10CardsCorrectly()
    {
        /*
         Oluşturulan 10 kart:
         - card1: (7, 2)
         - card2: (5, 1)
         - card3: (9, 1)
         - card4: (4, 3)
         - card5: (3, 2)
         - card6: (8, 1)
         - card7: (2, 3)
         - card8: (6, 2)
         - card9: (10, 1)
         - card10: (1, 3)
         
         Beklenen sıralama (önce Number, sonra Suit ascending):
         Index 0: (1, 3)   -> card10
         Index 1: (2, 3)   -> card7
         Index 2: (3, 2)   -> card5
         Index 3: (4, 3)   -> card4
         Index 4: (5, 1)   -> card2
         Index 5: (6, 2)   -> card8
         Index 6: (7, 2)   -> card1
         Index 7: (8, 1)   -> card6
         Index 8: (9, 1)   -> card3
         Index 9: (10, 1)  -> card9
        */

        // Kartları oluşturup ekliyoruz (oluşturma sırası karışık, sıralama metodunun işi sıralamak olacak).
        Card card1 = CreateTestCard(7, 2);    // (7,2)
        Card card2 = CreateTestCard(5, 1);    // (5,1)
        Card card3 = CreateTestCard(9, 1);    // (9,1)
        Card card4 = CreateTestCard(4, 3);    // (4,3)
        Card card5 = CreateTestCard(3, 2);    // (3,2)
        Card card6 = CreateTestCard(8, 1);    // (8,1)
        Card card7 = CreateTestCard(2, 3);    // (2,3)
        Card card8 = CreateTestCard(6, 2);    // (6,2)
        Card card9 = CreateTestCard(10, 1);   // (10,1)
        Card card10 = CreateTestCard(1, 3);   // (1,3)

        // Kartlar, MyDeckManager'ın myDeck listesine ekleniyor.
        deckManager.AddCard(card1);
        deckManager.AddCard(card2);
        deckManager.AddCard(card3);
        deckManager.AddCard(card4);
        deckManager.AddCard(card5);
        deckManager.AddCard(card6);
        deckManager.AddCard(card7);
        deckManager.AddCard(card8);
        deckManager.AddCard(card9);
        deckManager.AddCard(card10);

        // Sıralama metodunu çağırıyoruz.
        deckManager.SortDeckBySuitThenNumber();

        // deckManagerGO altındaki kartların sıralı hali, sibling index'e göre belirlenecektir.
        Card[] cardsInParent = deckManagerGO.GetComponentsInChildren<Card>();
        Assert.AreEqual(10, cardsInParent.Length, "Eklenen kart sayısı 10 olmalı.");

        // Kartları sibling index'e göre sıralayalım.
        List<(Card card, int siblingIndex)> cardIndices = new List<(Card, int)>();
        foreach (var card in cardsInParent)
        {
            int sibIndex = card.transform.GetSiblingIndex();
            cardIndices.Add((card, sibIndex));
        }
        cardIndices.Sort((a, b) => a.siblingIndex.CompareTo(b.siblingIndex));

        // Beklenen sıralama:
        // Index 0: (1,3)   -> card10
        // Index 1: (2,3)   -> card7
        // Index 2: (3,2)   -> card5
        // Index 3: (4,3)   -> card4
        // Index 4: (5,1)   -> card2
        // Index 5: (6,2)   -> card8
        // Index 6: (7,2)   -> card1
        // Index 7: (8,1)   -> card6
        // Index 8: (9,1)   -> card3
        // Index 9: (10,1)  -> card9

        // Beklenen sıralamayı CardData üzerinden tanımlayalım:
        CardData[] expectedOrder = new CardData[10];
        expectedOrder[0] = new CardData(1, 3, 0);
        expectedOrder[1] = new CardData(2, 3, 0);
        expectedOrder[2] = new CardData(3, 2, 0);
        expectedOrder[3] = new CardData(4, 3, 0);
        expectedOrder[4] = new CardData(5, 1, 0);
        expectedOrder[5] = new CardData(6, 2, 0);
        expectedOrder[6] = new CardData(7, 2, 0);
        expectedOrder[7] = new CardData(8, 1, 0);
        expectedOrder[8] = new CardData(9, 1, 0);
        expectedOrder[9] = new CardData(10, 1, 0);

        for (int i = 0; i < cardIndices.Count; i++)
        {
            CardData actual = cardIndices[i].card.GetCardData();
            CardData expected = expectedOrder[i];

            Assert.AreEqual(expected.Number, actual.Number, $"Index {i}: Beklenen Number {expected.Number} fakat bulundu {actual.Number}");
            Assert.AreEqual(expected.Suit, actual.Suit, $"Index {i}: Beklenen Suit {expected.Suit} fakat bulundu {actual.Suit}");
        }
    }
}
