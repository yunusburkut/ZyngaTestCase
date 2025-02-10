public static class CardComparer
{
    public static int CompareBySuitThenNumber(Card cardA, Card cardB)
    {
        var dataA = cardA.GetCardData();
        var dataB = cardB.GetCardData();
        int suitDiff = dataA.Suit.CompareTo(dataB.Suit);
        if (suitDiff != 0) return suitDiff;
        return dataA.Number.CompareTo(dataB.Number);
    }

    public static int CompareByNumberThenSuit(Card cardA, Card cardB)
    {
        var dataA = cardA.GetCardData();
        var dataB = cardB.GetCardData();
        int numDiff = dataA.Number.CompareTo(dataB.Number);
        if (numDiff != 0) return numDiff;
        return dataA.Suit.CompareTo(dataB.Suit);
    }
}