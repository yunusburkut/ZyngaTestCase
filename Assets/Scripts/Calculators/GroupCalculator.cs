using System.Collections.Generic;
using UnityEngine;

public class GroupCalculator
{
    private int deadwood = 0;
    public void MarkGroups(List<Card> deck)
    {
        int n = deck.Count;
        for (int i = 0; i < n; i++)
            deck[i].SetGroupID(0);

        int index = 0;
        byte groupId = 1;
        while (index < n)
        {
            bool grouped = false;
            Card current = deck[index];
            CardData currentData = current.GetCardData();

            if (index < n - 1)
            {
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
        for (int i = 0; i < n; i++)
            deck[i].SetGroupID(0);

        byte groupId = 1;
        int index = 0;
        while (index < n)
        {
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
