using System.Collections.Generic;

public static class CandidateMeldGenerator
{
    /// <summary>
    /// Verilen deck içinden candidate meld'leri (run ve set kombinasyonlarını) oluşturur.
    /// Mevcut yardımcı metotlar (CardComparer ve CombinationGenerator) yeniden kullanılır.
    /// </summary>
    /// <param name="deck">Deck'teki kartların listesi</param>
    /// <returns>Candidate meld'lerin listesi (her biri bir kart listesi)</returns>
    public static List<List<Card>> GenerateCandidateMelds(List<Card> deck)
    {
        List<List<Card>> candidates = new List<List<Card>>();
        int n = deck.Count;

        // ====================
        // 1. Run'ler (Aynı suit içinde ardışık kartlar)
        // ====================

        // Suit bazında gruplama: Her suit için kartları topluyoruz.
        Dictionary<int, List<Card>> suitGroups = new Dictionary<int, List<Card>>();
        for (int i = 0; i < n; i++)
        {
            int suit = deck[i].GetCardData().Suit;
            if (!suitGroups.ContainsKey(suit))
            {
                suitGroups.Add(suit, new List<Card>());
            }
            suitGroups[suit].Add(deck[i]);
        }

        // Her suit grubunu, daha önce tanımlı CompareBySuitThenNumber metodu ile sıralıyoruz.
        foreach (KeyValuePair<int, List<Card>> pair in suitGroups)
        {
            List<Card> cardsOfSuit = pair.Value;
            cardsOfSuit.Sort(CardComparer.CompareBySuitThenNumber);

            // Sıralı kartlardan ardışık (run) candidate meld'leri oluşturuyoruz.
            for (int i = 0; i < cardsOfSuit.Count; i++)
            {
                int start = i;
                int end = i;
                // Ardışıklığı kontrol ediyoruz: Sonraki kartın numarası, öncekinin numarasına 1 eklenmiş olmalı.
                while (end + 1 < cardsOfSuit.Count &&
                       cardsOfSuit[end + 1].GetCardData().Number == 
                       cardsOfSuit[end].GetCardData().Number + 1)
                {
                    end++;
                }
                int runLength = end - start + 1;
                // Eğer ardışıklık en az 3 kart içeriyorsa, candidate meld üretelim.
                if (runLength >= 3)
                {
                    // Run meld'lerinde, minimum 3 karttan başlayıp, tüm ardışık diziyi de candidate yapabiliriz.
                    for (int meldSize = 3; meldSize <= runLength; meldSize++)
                    {
                        for (int j = start; j <= end - meldSize + 1; j++)
                        {
                            List<Card> meld = new List<Card>();
                            for (int k = 0; k < meldSize; k++)
                            {
                                meld.Add(cardsOfSuit[j + k]);
                            }
                            candidates.Add(meld);
                        }
                    }
                }
                // i'yi, bu ardışığın son elemanına eşitleyerek gereksiz tekrarları önlüyoruz.
                i = end;
            }
        }

        // ====================
        // 2. Set'ler (Aynı numaralı kartlar)
        // ====================

        // Numara bazında gruplama: Aynı numaraya sahip kartları topluyoruz.
        Dictionary<int, List<Card>> numberGroups = new Dictionary<int, List<Card>>();
        for (int i = 0; i < n; i++)
        {
            int number = deck[i].GetCardData().Number;
            if (!numberGroups.ContainsKey(number))
            {
                numberGroups.Add(number, new List<Card>());
            }
            numberGroups[number].Add(deck[i]);
        }

// Her numara grubunda, eğer kart sayısı 3 veya daha fazlaysa, önce kartları CompareByNumberThenSuit ile sıralayalım.
        foreach (KeyValuePair<int, List<Card>> pair in numberGroups)
        {
            List<Card> cardsOfNumber = pair.Value;
            cardsOfNumber.Sort(CardComparer.CompareByNumberThenSuit);

            if (cardsOfNumber.Count >= 3)
            {
                // Daha önce tanımlı CombinationGenerator ile 3 kartlık kombinasyonlar üretiyoruz.
                List<List<Card>> comb3 = CombinationGenerator.GenerateCombinations(cardsOfNumber, 3);
                for (int i = 0; i < comb3.Count; i++)
                {
                    candidates.Add(comb3[i]);
                }
                // Eğer varsa, 4 kartlık kombinasyonları da candidate olarak ekleyelim.
                if (cardsOfNumber.Count >= 4)
                {
                    List<List<Card>> comb4 = CombinationGenerator.GenerateCombinations(cardsOfNumber, 4);
                    for (int i = 0; i < comb4.Count; i++)
                    {
                        candidates.Add(comb4[i]);
                    }
                }
            }
        }

        return candidates;
    }
}
