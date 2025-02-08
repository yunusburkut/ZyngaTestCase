using System.Collections.Generic;

public class GroupCalculator
{
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
            CardData currentData = current.GetCardData();

            // Run kontrolü: Aynı suit içinde ardışık kartlar
            if (index < n - 1)
            {
                CardData nextData = deck[index + 1].GetCardData();
                if (currentData.Suit == nextData.Suit)
                {
                    int chainLength = 1;
                    while (index + chainLength < n)
                    {
                        CardData nextChain = deck[index + chainLength].GetCardData();
                        CardData prevChain = deck[index + chainLength - 1].GetCardData();
                        if (nextChain.Suit == currentData.Suit && nextChain.Number == prevChain.Number + 1)
                            chainLength++;
                        else
                            break;
                    }
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
                CardData nextData = deck[index + 1].GetCardData();
                if (currentData.Number == nextData.Number)
                {
                    int chainLength = 1;
                    while (index + chainLength < n)
                    {
                        if (deck[index + chainLength].GetCardData().Number == currentData.Number)
                            chainLength++;
                        else
                            break;
                    }
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
        for (int i = 0; i < deck.Count; i++)
            deck[i].SetGroupID(0);

        byte groupId = 1;
        int n = deck.Count;
        int index = 0;
        while (index < n)
        {
            Card current = deck[index];
            CardData currentData = current.GetCardData();
            int chainLength = 1;
            int j = index + 1;
            while (j < n)
            {
                CardData nextData = deck[j].GetCardData();
                if (currentData.Suit == nextData.Suit && nextData.Number == currentData.Number + chainLength)
                {
                    chainLength++;
                    j++;
                }
                else
                {
                    break;
                }
            }
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
    }

    public void CalculateSet(List<Card> deck)
    {
        deck.Sort(CardComparer.CompareByNumberThenSuit);
        for (int i = 0; i < deck.Count; i++)
            deck[i].SetGroupID(0);

        byte groupId = 1;
        int n = deck.Count;
        int index = 0;
        while (index < n)
        {
            Card current = deck[index];
            CardData currentData = current.GetCardData();
            int chainLength = 1;
            int j = index + 1;
            while (j < n)
            {
                if (deck[j].GetCardData().Number == currentData.Number)
                {
                    chainLength++;
                    j++;
                }
                else
                {
                    break;
                }
            }
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
    }
}
