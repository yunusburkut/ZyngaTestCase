using System.Collections.Generic;
using UnityEngine;

public class GroupCalculator
{
    private int deadwood = 0;
    public void MarkGroups(List<Card> deck)
    {
        int n = deck.Count;
        // Tüm kartların GroupID'sini sıfırla.
        for (int i = 0; i < n; i++)
            deck[i].SetGroupID(0);

        int index = 0;
        byte groupId = 1;
        while (index < n)
        {
            bool grouped = false;
            Card current = deck[index];
            // currentData'yi bir kere alıyoruz.
            CardData currentData = current.GetCardData();

            // Run kontrolü: Aynı suit içinde ardışık kartlar
            if (index < n - 1)
            {
                // Önceki değeri alıp karşılaştırıyoruz.
                CardData nextData = deck[index + 1].GetCardData();
                if (currentData.Suit == nextData.Suit)
                {
                    int chainLength = GetRunChainLength(deck, index, n, currentData.Suit, currentData.Number);
                    if (chainLength >= 3)
                    {
                        for (int k = index; k < index + chainLength; k++)
                            deck[k].SetGroupID(groupId);
                        groupId++;
                        index += chainLength;
                        grouped = true;
                        continue;
                    }
                }
            }
            // Set kontrolü: Aynı numaralı kartlar
            if (!grouped && index < n - 1)
            {
                int targetNumber = currentData.Number;
                CardData nextData = deck[index + 1].GetCardData();
                if (targetNumber == nextData.Number)
                {
                    int chainLength = GetSetChainLength(deck, index, n, targetNumber);
                    if (chainLength >= 3)
                    {
                        for (int k = index; k < index + chainLength; k++)
                            deck[k].SetGroupID(groupId);
                        groupId++;
                        index += chainLength;
                        continue;
                    }
                }
            }
            index++;
        }
    }

    public void CalculateRun(List<Card> deck)
    {
        deck.Sort(CardComparer.CompareBySuitThenNumber);
        int n = deck.Count;
        // Sıfırlama işlemini minimal GC etkisiyle yapalım.
        for (int i = 0; i < n; i++)
            deck[i].SetGroupID(0);

        byte groupId = 1;
        int index = 0;
        while (index < n)
        {
            // currentData'yi bir kere alıyoruz.
            CardData currentData = deck[index].GetCardData();
            int chainLength = GetRunChainLength(deck, index, n, currentData.Suit, currentData.Number);
            if (chainLength >= 3)
            {
                for (int k = index; k < index + chainLength; k++)
                    deck[k].SetGroupID(groupId);
                groupId++;
                index += chainLength;
            }
            else
            {
                index++;
            }
        }
        CalculateDeadwood(deck);
    }

    public void CalculateSet(List<Card> deck)
    {
        deck.Sort(CardComparer.CompareByNumberThenSuit);
        int n = deck.Count;
        for (int i = 0; i < n; i++)
            deck[i].SetGroupID(0);

        byte groupId = 1;
        int index = 0;
        while (index < n)
        {
            int targetNumber = deck[index].GetCardData().Number;
            int chainLength = GetSetChainLength(deck, index, n, targetNumber);
            if (chainLength >= 3)
            {
                for (int k = index; k < index + chainLength; k++)
                    deck[k].SetGroupID(groupId);
                groupId++;
                index += chainLength;
            }
            else
            {
                index++;
            }
        }
        CalculateDeadwood(deck);
    }

    // Run zincir uzunluğunu hesaplar. Eğer GetCardData() metodunun her çağrısı yeni nesne oluşturuyorsa,
    // bu değerleri lokal değişkende saklamak GC baskısını azaltır.
    private static int GetRunChainLength(List<Card> deck, int startIndex, int n, int suit, int startNumber)
    {
        int chainLength = 1;
        for (int i = startIndex + 1; i < n; i++)
        {
            CardData nextData = deck[i].GetCardData();
            if (nextData.Suit != suit)
                break;
            if (nextData.Number == startNumber + chainLength)
                chainLength++;
            else
                break;
        }
        return chainLength;
    }
    public int CalculateDeadwood(List<Card> deck)
    {
        deadwood = 0;
        int n = deck.Count;
        for (int i = 0; i < n; i++)
        {
            if (deck[i].GetCardData().GroupID == 0)
                deadwood += deck[i].GetPoint();
        }
        Debug.Log("Deadwood : " + deadwood);
        return deadwood;
    }

    public int GetDeadwood()
    {
        return deadwood;
    }
    // Set zincir uzunluğunu hesaplar.
    private static int GetSetChainLength(List<Card> deck, int startIndex, int n, int targetNumber)
    {
        int chainLength = 1;
        for (int i = startIndex + 1; i < n; i++)
        {
            if (deck[i].GetCardData().Number == targetNumber)
                chainLength++;
            else
                break;
        }
        return chainLength;
    }
}
