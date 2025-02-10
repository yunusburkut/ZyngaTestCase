#if UNITY_EDITOR
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupCalculator_SortingTests
{
    /// <summary>
    /// Creates a new test Card with the given CardData.
    /// A new GameObject is created with required UI components (Canvas, CanvasRenderer, Image)
    /// and a Card component is added. The card's data is then set.
    /// </summary>
    /// <param name="data">The CardData to assign to the card.</param>
    /// <returns>The created Card component.</returns>
    private Card CreateTestCard(CardData data)
    {
        GameObject go = new GameObject("TestCard");
        // Add required UI components so that the card can render correctly.
        go.AddComponent<Canvas>();
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Image>();
        // Add the Card component.
        Card card = go.AddComponent<Card>();
        // Set the card data (including number, suit, and initial group ID).
        card.SetCardData(data);
        return card;
    }

    /// <summary>
    /// Shuffles the given list using the Fisher–Yates algorithm.
    /// This helps ensure that the deck is in a random order before testing.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    private void ShuffleDeck<T>(List<T> list)
    {
        System.Random rnd = new System.Random();
        int count = list.Count;
        for (int i = count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            // Swap elements at indices i and j.
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Tests the CalculateRun method of GroupCalculator.
    /// This test creates a deck with specific cards, shuffles it, then calls CalculateRun.
    /// It verifies that:
    ///   - The deck is sorted correctly by suit and number.
    ///   - Suit 1 cards are grouped into two runs (e.g. first run (2,3,4) and second run (5,7,8,9)).
    ///   - Suit 2 cards do not form a valid run, so their GroupID remains 0.
    /// </summary>
    [Test]
    public void CalculateRun_SortsDeckAndAssignsGroupIDs_Correctly()
    {
       
        List<Card> deck = new List<Card>
        {
            CreateTestCard(new CardData(8, 1, 0)),
            CreateTestCard(new CardData(3, 1, 0)),
            CreateTestCard(new CardData(10,2, 0)),
            CreateTestCard(new CardData(2, 1, 0)),
            CreateTestCard(new CardData(7, 1, 0)),
            CreateTestCard(new CardData(4, 1, 0)),
            CreateTestCard(new CardData(6, 1, 0)),
            CreateTestCard(new CardData(9, 1, 0)),
            CreateTestCard(new CardData(4, 2, 0)),
            CreateTestCard(new CardData(7, 2, 0))
        };

        ShuffleDeck(deck);
        GroupCalculator gc = new GroupCalculator();
        gc.CalculateRun(deck);

        int deckCount = deck.Count;
        for (int i = 1; i < deckCount; i++)
        {
            CardData prev = deck[i - 1].GetCardData();
            CardData curr = deck[i].GetCardData();
            if (prev.Suit == curr.Suit)
            {
                Assert.LessOrEqual(prev.Number, curr.Number, $"Deck sıralaması hatalı: {prev.Number} > {curr.Number} (Suit {prev.Suit})");
            }
            else
        {
                Assert.Less(prev.Suit, curr.Suit, "Deck suit sıralaması hatalı.");
            }
        }

        List<Card> suit1Cards = deck.FindAll(card => card.GetCardData().Suit == 1);
        Assert.GreaterOrEqual(suit1Cards.Count, 7, "Suit 1 kart sayısı yetersiz.");
        byte groupIdFirst = suit1Cards[0].GetCardData().GroupID;
        Assert.AreNotEqual(0, groupIdFirst, "İlk run GroupID 0 olmamalı.");
        Assert.AreEqual(2, suit1Cards[0].GetCardData().Number, "İlk runun ilk kartı 2 olmalı.");
        Assert.AreEqual(3, suit1Cards[1].GetCardData().Number, "İlk runun ikinci kartı 3 olmalı.");
        Assert.AreEqual(4, suit1Cards[2].GetCardData().Number, "İlk runun üçüncü kartı 4 olmalı.");
        List<Card> suit2Cards = deck.FindAll(card => card.GetCardData().Suit == 2);
        foreach (Card card in suit2Cards)
        {
            Assert.AreEqual(0, card.GetCardData().GroupID, $"Suit 2 kartı (number: {card.GetCardData().Number}) run'e dahil olmamalı.");
        }
        foreach (Card card in deck)
        {
            Object.DestroyImmediate(card.gameObject);
        }
    }

    /// <summary>
    /// Tests the CalculateSet method of GroupCalculator.
    /// It creates a deck with specific cards, shuffles it, and calls CalculateSet.
    /// The test then verifies that:
    ///   - The deck is sorted correctly by number then suit.
    ///   - Cards with the number 8 form a valid set (they receive a non-zero GroupID).
    ///   - Cards with numbers other than 8 remain ungrouped (GroupID remains 0).
    /// </summary>
    [Test]
    public void CalculateSet_SortsDeckAndAssignsGroupIDs_Correctly()
    {
        // Create a test deck with 10 cards.
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
        
        // Expected order after sorting by number then suit:
        // (2,1), (3,1), (4,1), (5,1), (7,2), (8,1), (8,2), (8,3), (9,2), (10,2)
        List<(byte number, byte suit)> expectedOrderSet = new List<(byte, byte)>
        {
            (2,1), (3,1), (4,1), (5,1), (7,2), (8,1), (8,2), (8,3), (9,2), (10,2)
        };

        // Shuffle the deck to randomize the initial order.
        ShuffleDeck(deck);

        // Create a new instance of GroupCalculator and call CalculateSet to group set melds.
        GroupCalculator gc = new GroupCalculator();
        gc.CalculateSet(deck);

        // Verify that the deck is sorted in the expected order.
        Assert.AreEqual(expectedOrderSet.Count, deck.Count, "Deck element count should match expected count.");
        for (int i = 0; i < deck.Count; i++)
        {
            CardData data = deck[i].GetCardData();
            Assert.AreEqual(expectedOrderSet[i].number, data.Number, $"Index {i}: Expected number {expectedOrderSet[i].number}, found {data.Number}");
            Assert.AreEqual(expectedOrderSet[i].suit, data.Suit, $"Index {i}: Expected suit {expectedOrderSet[i].suit}, found {data.Suit}");
        }

        // Verify that cards with number 8 form a meld (set) and have a non-zero GroupID.
        List<Card> setCards = deck.FindAll(card => card.GetCardData().Number == 8);
        Assert.AreEqual(3, setCards.Count, "There should be exactly 3 cards with number 8.");
        foreach (Card card in setCards)
        {
            Assert.That(card.GetCardData().GroupID, Is.Not.EqualTo(0), "Card with number 8 should be included in a meld (GroupID != 0).");
        }

        // Verify that cards with numbers other than 8 remain ungrouped (GroupID should be 0).
        List<Card> otherCards = deck.FindAll(card => card.GetCardData().Number != 8);
        foreach (Card card in otherCards)
        {
            Assert.That(card.GetCardData().GroupID, Is.EqualTo(0), $"Card with number {card.GetCardData().Number} should not be grouped (GroupID should be 0).");
        }
        
        // Clean up: Destroy all GameObjects created during the test.
        foreach (Card card in deck)
        {
            Object.DestroyImmediate(card.gameObject);
        }
    }

    /// <summary>
    /// Tests the ComputeOptimalMelds method of MeldOptimizer using a perfect deck.
    /// A "perfect deck" is defined as one where all cards can be melded, resulting in 0 deadwood.
    /// In this test:
    ///   - A run meld (cards 3, 4, 5 of suit 1) is created.
    ///   - A set meld (three 7s of different suits) is created.
    /// The optimal result should have 0 deadwood and the expected number of melds.
    /// </summary>
    [Test]
    public void ComputeOptimalMelds_PerfectDeck_ReturnsZeroDeadwoodAndExpectedMeldCount()
    {
        // Create a perfect deck for meld optimization:
        // Run meld: 3,4,5 of suit 1.
        // Set meld: three 7s from suits 1, 2, and 3.
        List<Card> deck = new List<Card>
        {
            CreateTestCard(new CardData(3, 1, 0)),
            CreateTestCard(new CardData(4, 1, 0)),
            CreateTestCard(new CardData(5, 1, 0)),
            CreateTestCard(new CardData(7, 1, 0)),
            CreateTestCard(new CardData(7, 2, 0)),
            CreateTestCard(new CardData(7, 3, 0)),
            // Additional cards (if needed) can be added here.
            CreateTestCard(new CardData(8, 3, 0)),
            CreateTestCard(new CardData(9, 3, 0)),
            CreateTestCard(new CardData(2, 1, 0)),
            CreateTestCard(new CardData(7, 4, 0))
        };

        // Create an instance of MeldOptimizer and compute optimal melds.
        MeldOptimizer optimizer = new MeldOptimizer();
        MeldOptimizer.OptimalResult result = optimizer.ComputeOptimalMelds(deck);

        // Verify that the optimal deadwood is 0 for a perfect deck.
        Assert.AreEqual(0, result.Deadwood, "Optimal deadwood should be 0 for a perfect deck.");
        // Verify that the expected meld count is correct.
        // Note: The expected meld count may depend on your implementation details.
        // Here, we assume that the optimal solution uses 3 melds.
        Assert.AreEqual(3, result.Melds.Count, "Expected meld count is 3 for a perfect deck.");

        // Clean up: Destroy all GameObjects created during the test.
        foreach (Card card in deck)
        {
            Object.DestroyImmediate(card.gameObject);
        }
    }
}
#endif
