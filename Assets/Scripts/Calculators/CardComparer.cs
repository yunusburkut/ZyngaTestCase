public static class CardComparer
{
    /// <summary>
    /// Compares two cards first by their suit, then by their number.
    /// This is useful when you need to sort cards so that cards with the same suit are grouped together
    /// and, within the same suit, sorted in ascending order by their number.
    /// </summary>
    /// <param name="cardA">The first card to compare.</param>
    /// <param name="cardB">The second card to compare.</param>
    /// <returns>
    /// A negative value if cardA should come before cardB, zero if they are equal,
    /// and a positive value if cardA should come after cardB.
    /// </returns>
    public static int CompareBySuitThenNumber(Card cardA, Card cardB)
    {
        // Retrieve the card data (which contains the suit and number) for each card.
        var dataA = cardA.GetCardData();
        var dataB = cardB.GetCardData();

        // Compare the suits first.
        int suitDiff = dataA.Suit.CompareTo(dataB.Suit);
        // If the suits are different, return the comparison result.
        if (suitDiff != 0)
            return suitDiff;
        
        // If the suits are the same, compare the numbers.
        return dataA.Number.CompareTo(dataB.Number);
    }

    /// <summary>
    /// Compares two cards first by their number, then by their suit.
    /// This is useful when you need to sort cards primarily by their number
    /// and then, for cards with the same number, sort them by their suit.
    /// </summary>
    /// <param name="cardA">The first card to compare.</param>
    /// <param name="cardB">The second card to compare.</param>
    /// <returns>
    /// A negative value if cardA should come before cardB, zero if they are equal,
    /// and a positive value if cardA should come after cardB.
    /// </returns>
    public static int CompareByNumberThenSuit(Card cardA, Card cardB)
    {
        // Retrieve the card data for each card.
        var dataA = cardA.GetCardData();
        var dataB = cardB.GetCardData();

        // Compare the numbers first.
        int numDiff = dataA.Number.CompareTo(dataB.Number);
        // If the numbers differ, return the result.
        if (numDiff != 0)
            return numDiff;
        
        // If the numbers are the same, compare the suits.
        return dataA.Suit.CompareTo(dataB.Suit);
    }
}
