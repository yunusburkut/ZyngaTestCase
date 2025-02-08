using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Text;

public class MyDeckManager : MonoBehaviour
{
    // Kartların bulunduğu deste
    [SerializeField] private List<Card> myDeck = new List<Card>();
    [SerializeField] private float cardSpacing = 30f;
    [SerializeField] private float shiftDuration = 0.5f;
    [SerializeField] private float spacing = 50f;
    [SerializeField] private float moveDuration = 0.5f;

    // Gruplanmamış (ungrouped) kartlar
    private List<Card> ungroupedCards = new List<Card>();

    public static MyDeckManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    #region Kart Düzenleme ve Görsel İşlemler

    // Destenize kart ekler ve layout'u günceller.
    public void AddCard(Card card)
    {
        if (card != null)
        {
            myDeck.Add(card);
            UpdateDeckLayout();
        }
    }

    // Kartların konumlarını tween (animasyon) ile günceller.
    public void UpdateDeckLayout()
    {
        int n = myDeck.Count;
        if (n == 0)
            return;
        float startX = -((n - 1) * cardSpacing) / 2f;
        for (int i = 0; i < n; i++)
        {
            RectTransform rt = myDeck[i].CachedRectTransform;
            if (rt != null)
            {
                Vector2 targetPos = new Vector2(startX + i * cardSpacing, 0);
                rt.DOKill();
                rt.DOAnchorPos(targetPos, shiftDuration).SetEase(Ease.OutQuad);
            }
        }
    }

    // Kartları suit'e göre sıralayıp yeniden pozisyonlar.
    public void SortAndRepositionDeckBySuitAscending()
    {
        myDeck.Sort(CompareBySuitThenNumberAscending);
    }

    // Kartları numara ve suit'e göre sıralayıp yeniden pozisyonlar.
    public void SortAndRepositionDeckByNumberThenSuitAscending()
    {
        myDeck.Sort(CompareByNumberThenSuitAscending);
    }

    // Kartları gruplarına göre yeniden düzenler.
    public void RepositionCardsByGroup(bool isDPFunction)
    {
        Dictionary<int, List<Card>> groupedDict = new Dictionary<int, List<Card>>();
        List<Card> ungroupedCardsLocal = new List<Card>();

        for (int i = 0; i < myDeck.Count; i++)
        {
            int groupID = myDeck[i].GetCardData().GroupID;
            if (groupID != 0)
            {
                if (!groupedDict.ContainsKey(groupID))
                    groupedDict[groupID] = new List<Card>();
                groupedDict[groupID].Add(myDeck[i]);
            }
            else
            {
                ungroupedCardsLocal.Add(myDeck[i]);
            }
        }

        // Her grup için stable sort uygulaması.
        foreach (var kvp in groupedDict)
        {
            StableSortGroup(kvp.Value);
        }

        List<int> groupIDs = new List<int>(groupedDict.Keys);
        groupIDs.Sort();
        List<Card> newOrder = new List<Card>();
        foreach (int gid in groupIDs)
        {
            newOrder.AddRange(groupedDict[gid]);
        }
        newOrder.AddRange(ungroupedCardsLocal);

        if (isDPFunction)
        {
            myDeck = newOrder;
        }
        
        float startX = -((myDeck.Count - 1) * spacing) / 2f;
        for (int i = 0; i < myDeck.Count; i++)
        {
            Card card = myDeck[i];
            card.transform.SetSiblingIndex(i);
            RectTransform rt = card.CachedRectTransform;
            if (rt != null)
            {
                Vector2 targetPos = new Vector2(startX + i * spacing, 0f);
                rt.DOKill();
                rt.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad);
            }
        }
    }

    // Grup içi sıralama: Insertion sort gibi stabil bir sıralama.
    private void StableSortGroup(List<Card> group)
    {
        for (int i = 1; i < group.Count; i++)
        {
            Card key = group[i];
            int j = i - 1;
            while (j >= 0 && CompareCardsWithinGroup(group[j], key) > 0)
            {
                group[j + 1] = group[j];
                j--;
            }
            group[j + 1] = key;
        }
    }

    // Grup içi sıralama için karşılaştırma: Önce Number, sonra Suit.
    private int CompareCardsWithinGroup(Card a, Card b)
    {
        int numberDiff = a.GetCardData().Number.CompareTo(b.GetCardData().Number);
        if (numberDiff != 0)
            return numberDiff;
        return a.GetCardData().Suit.CompareTo(b.GetCardData().Suit);
    }

    // Suit önce, sonra numara karşılaştırması.
    private int CompareBySuitThenNumberAscending(Card cardA, Card cardB)
    {
        var dataA = cardA.GetCardData();
        var dataB = cardB.GetCardData();
        int suitDiff = dataA.Suit - dataB.Suit;
        if (suitDiff != 0)
            return suitDiff;
        return dataA.Number - dataB.Number;
    }

    // Numara önce, sonra suit karşılaştırması.
    private int CompareByNumberThenSuitAscending(Card cardA, Card cardB)
    {
        var dataA = cardA.GetCardData();
        var dataB = cardB.GetCardData();
        int diff = dataA.Number - dataB.Number;
        if (diff != 0)
            return diff;
        return dataA.Suit - dataB.Suit;
    }

    // Kartlarda grupları işaretler: Sıralı run'ler ve aynı numaralı set'ler.
    public void MarkGroups()
    {
        int n = myDeck.Count;
        for (int i = 0; i < n; i++)
            myDeck[i].SetGroupID(0);
        int iIndex = 0;
        byte groupId = 1;
        while (iIndex < n)
        {
            bool grouped = false;
            var currentCard = myDeck[iIndex];
            var currentData = currentCard.GetCardData();
            if (iIndex < n - 1)
            {
                var nextData = myDeck[iIndex + 1].GetCardData();
                if (currentData.Suit == nextData.Suit)
                {
                    int chainLength = 1;
                    while (iIndex + chainLength < n)
                    {
                        var nextChainData = myDeck[iIndex + chainLength].GetCardData();
                        var prevChainData = myDeck[iIndex + chainLength - 1].GetCardData();
                        if (nextChainData.Suit == currentData.Suit &&
                            nextChainData.Number == prevChainData.Number + 1)
                            chainLength++;
                        else
                            break;
                    }
                    if (chainLength >= 3)
                    {
                        for (int k = iIndex; k < iIndex + chainLength; k++)
                            myDeck[k].SetGroupID(groupId);
                        groupId++;
                        iIndex += chainLength;
                        grouped = true;
                        continue;
                    }
                }
            }
            if (!grouped && iIndex < n - 1)
            {
                var nextData = myDeck[iIndex + 1].GetCardData();
                if (currentData.Number == nextData.Number)
                {
                    int chainLength = 1;
                    while (iIndex + chainLength < n)
                    {
                        var chainData = myDeck[iIndex + chainLength].GetCardData();
                        if (chainData.Number == currentData.Number)
                            chainLength++;
                        else
                            break;
                    }
                    if (chainLength >= 3)
                    {
                        for (int k = iIndex; k < iIndex + chainLength; k++)
                            myDeck[k].SetGroupID(groupId);
                        groupId++;
                        iIndex += chainLength;
                        continue;
                    }
                }
            }
            iIndex++;
        }
        UpdateUngroupedCards();
    }

    // Gruplanmamış kartların listesini günceller ve toplam puanları loglar.
    private void UpdateUngroupedCards()
    {
        ungroupedCards.Clear();
        int n = myDeck.Count;
        int totalPoints = 0;
        for (int i = 0; i < n; i++)
        {
            var data = myDeck[i].GetCardData();
            if (data.GroupID == 0)
            {
                ungroupedCards.Add(myDeck[i]);
                totalPoints += myDeck[i].GetPoint();
                Debug.Log(totalPoints);
            }
        }
    }

    // Kart sürükleme (drag) bittiğinde çağrılır, kartın yeni pozisyonunu hesaplar.
    public void OnCardDragEnd(Card draggedCard, Vector2 screenPosition, Camera eventCamera)
    {
        RectTransform deckRect = draggedCard.CachedRectTransform.parent as RectTransform;
        if (deckRect == null)
            return;

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(deckRect, screenPosition, eventCamera, out localPoint))
            return;

        float dropX = localPoint.x;
        int count = myDeck.Count;
        float startX = -((count - 1) * spacing) / 2f;
        int newIndex = Mathf.RoundToInt((dropX - startX) / spacing);
        newIndex = Mathf.Clamp(newIndex, 0, count - 1);

        if (myDeck.Contains(draggedCard))
            myDeck.Remove(draggedCard);
        myDeck.Insert(newIndex, draggedCard);
        MarkGroups();
        RepositionCardsByGroup(false);
    }

    // Sürükleme sırasında diğer kartların animasyonlarını günceller.
    public void UpdateCardsAnimationDuringDrag(Card draggedCard)
    {
        int count = myDeck.Count;
        if (count == 0)
            return;
        float startX = -((count - 1) * spacing) / 2f;
        float threshold = spacing * 0.5f;
        float offsetY = 20f;
        float draggedX = draggedCard.CachedRectTransform.anchoredPosition.x;
        for (int i = 0; i < count; i++)
        {
            Card card = myDeck[i];
            if (card == draggedCard)
                continue;
            RectTransform cardRect = card.CachedRectTransform;
            if (cardRect == null)
                continue;
            float defaultX = startX + i * spacing;
            float diff = Mathf.Abs(defaultX - draggedX);
            Vector2 targetPos = (diff < threshold) ? new Vector2(defaultX, offsetY) : new Vector2(defaultX, 0f);
            cardRect.DOKill();
            cardRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    // Destenin tamamını döndürür.
    public List<Card> GetMyDeck()
    {
        return myDeck;
    }

    // Destenin içeriğini loglar.
    public void LogDeck()
    {
        StringBuilder deckInfo = new StringBuilder("MyDeck: ");
        int n = myDeck.Count;
        for (int i = 0; i < n; i++)
        {
            var data = myDeck[i].GetCardData();
            deckInfo.Append($"[{i}] Number:{data.Number}, Suit:{data.Suit}  ");
        }
        Debug.Log(deckInfo.ToString());
    }

    #endregion

    #region Hesaplama Metotları (RunHesapla, SetHesapla)

    // Sıralı (run) kombinasyonlar için hesaplama.
    public void RunHesapla()
    {
        for (int i = 0; i < myDeck.Count; i++)
            myDeck[i].SetGroupID(0);
        myDeck.Sort(CompareBySuitThenNumberAscending);

        byte groupId = 1;
        int n = myDeck.Count;
        int iIndex = 0;
        while (iIndex < n)
        {
            var currentCard = myDeck[iIndex];
            var currentData = currentCard.GetCardData();
            int chainLength = 1;
            int j = iIndex + 1;
            while (j < n)
            {
                var nextCard = myDeck[j];
                var nextData = nextCard.GetCardData();
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
                for (int k = iIndex; k < iIndex + chainLength; k++)
                    myDeck[k].SetGroupID(groupId);
                groupId++;
                iIndex += chainLength;
            }
            else
            {
                iIndex++;
            }
        }
        UpdateUngroupedCards();
        RepositionCardsByGroup(true);
    }

    // Set (aynı numaralı) kombinasyonlar için hesaplama.
    public void SetHesapla()
    {
        for (int i = 0; i < myDeck.Count; i++)
            myDeck[i].SetGroupID(0);
        myDeck.Sort(CompareByNumberThenSuitAscending);

        byte groupId = 1;
        int n = myDeck.Count;
        int iIndex = 0;
        while (iIndex < n)
        {
            var currentCard = myDeck[iIndex];
            var currentData = currentCard.GetCardData();
            int chainLength = 1;
            int j = iIndex + 1;
            while (j < n)
            {
                var nextData = myDeck[j].GetCardData();
                if (nextData.Number == currentData.Number)
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
                for (int k = iIndex; k < iIndex + chainLength; k++)
                    myDeck[k].SetGroupID(groupId);
                groupId++;
                iIndex += chainLength;
            }
            else
            {
                iIndex++;
            }
        }
        UpdateUngroupedCards();
        RepositionCardsByGroup(true);
    }

    #endregion

    #region OptimalDeckHesapla & DP (GC Dostu Versiyon)

    // Optimal hesaplama: Kartların kombinasyonları (meld'ler) üzerinden DP uygulanıyor.
    public void OptimalDeckHesapla()
    {
        // Tüm kartların GroupID'sini sıfırla.
        for (int i = 0; i < myDeck.Count; i++)
            myDeck[i].SetGroupID(0);

        // Candidate meld'leri oluşturuyoruz (bitmask yerine List<List<Card>> kullanılıyor).
        List<List<Card>> candidateMelds = GenerateCandidateMelds();

        // DP memoization için, state hash'ini int olarak kullanıyoruz.
        Dictionary<int, DPResult> memo = new Dictionary<int, DPResult>();

        // Orijinal listeyi değiştirmemek için kopyasını oluşturuyoruz.
        List<Card> remaining = new List<Card>(myDeck);

        DPResult optimal = ComputeOptimalMelds(remaining, candidateMelds, memo);

        byte groupId = 1;
        for (int i = 0; i < optimal.melds.Count; i++)
        {
            List<Card> meld = optimal.melds[i];
            for (int j = 0; j < meld.Count; j++)
                meld[j].SetGroupID(groupId);
            groupId++;
        }

        UpdateUngroupedCards();
        RepositionCardsByGroup(true);

        Debug.Log("Optimal Deadwood Puanı: " + optimal.deadwood);
    }

    // DP sonucunu tutan sınıf (liste tabanlı)
    private class DPResult
    {
        public int deadwood;           // Kalan kartların toplam puanı
        public List<List<Card>> melds; // Seçilen meld'ler (her biri kart listesidir)
    }

    // Kalan kartların toplam puanını hesaplar.
    private int SumPoints(List<Card> cards)
    {
        int sum = 0;
        for (int i = 0; i < cards.Count; i++)
            sum += cards[i].GetPoint();
        return sum;
    }

    // Order-independent state hash hesaplaması: Her kartın GetInstanceID() değeri üzerinden.
    private int GetStateHash(List<Card> remaining)
    {
        int sum = 0;
        int xor = 0;
        for (int i = 0; i < remaining.Count; i++)
        {
            int id = remaining[i].GetInstanceID();
            sum += id;
            xor ^= id;
        }
        return (sum * 397) ^ xor;
    }

    // DP metodu: Mevcut remaining listesinden in-place modifikasyon yaparak hesaplama.
    private DPResult ComputeOptimalMelds(List<Card> remaining, List<List<Card>> candidateMelds, Dictionary<int, DPResult> memo)
    {
        if (remaining.Count == 0)
            return new DPResult { deadwood = 0, melds = new List<List<Card>>() };

        int key = GetStateHash(remaining);
        if (memo.TryGetValue(key, out DPResult cached))
            return cached;

        int bestDeadwood = SumPoints(remaining);
        List<List<Card>> bestMelds = null;

        // Geçici liste: Meld'deki kartları çıkarmada kullanıyoruz.
        List<Card> removed = new List<Card>();

        for (int m = 0; m < candidateMelds.Count; m++)
        {
            List<Card> meld = candidateMelds[m];
            bool isSubset = true;
            for (int i = 0; i < meld.Count; i++)
            {
                if (!remaining.Contains(meld[i]))
                {
                    isSubset = false;
                    break;
                }
            }
            if (!isSubset)
                continue;

            // Meld'deki kartları remaining listesinden çıkarıyoruz (in-place).
            removed.Clear();
            for (int i = 0; i < meld.Count; i++)
            {
                if (remaining.Remove(meld[i]))
                    removed.Add(meld[i]);
            }

            DPResult subResult = ComputeOptimalMelds(remaining, candidateMelds, memo);
            int currentDeadwood = subResult.deadwood;
            if (currentDeadwood < bestDeadwood)
            {
                bestDeadwood = currentDeadwood;
                bestMelds = new List<List<Card>>(subResult.melds);
                bestMelds.Add(meld);
            }

            // Çıkarılan kartları tekrar ekliyoruz.
            remaining.AddRange(removed);
        }

        if (bestMelds == null)
            bestMelds = new List<List<Card>>();

        DPResult result = new DPResult { deadwood = bestDeadwood, melds = bestMelds };
        memo[key] = result;
        return result;
    }

    // Candidate meld'leri oluşturur: Run'ler (aynı suit ve ardışık) ve set'ler (aynı numara).
    private List<List<Card>> GenerateCandidateMelds()
    {
        List<List<Card>> candidates = new List<List<Card>>();
        int n = myDeck.Count;

        // Run'ler: Aynı suit içindeki ardışık kartlar.
        Dictionary<int, List<Card>> suitGroups = new Dictionary<int, List<Card>>();
        for (int i = 0; i < n; i++)
        {
            int suit = myDeck[i].GetCardData().Suit;
            if (!suitGroups.ContainsKey(suit))
                suitGroups[suit] = new List<Card>();
            suitGroups[suit].Add(myDeck[i]);
        }
        foreach (var kvp in suitGroups)
        {
            List<Card> cardsOfSuit = kvp.Value;
            cardsOfSuit.Sort((a, b) => a.GetCardData().Number.CompareTo(b.GetCardData().Number));
            for (int i = 0; i < cardsOfSuit.Count; i++)
            {
                int start = i;
                int end = i;
                while (end + 1 < cardsOfSuit.Count &&
                       cardsOfSuit[end + 1].GetCardData().Number == cardsOfSuit[end].GetCardData().Number + 1)
                {
                    end++;
                }
                int runLength = end - start + 1;
                if (runLength >= 3)
                {
                    for (int runSize = 3; runSize <= runLength; runSize++)
                    {
                        for (int j = start; j <= end - runSize + 1; j++)
                        {
                            List<Card> meld = new List<Card>();
                            for (int k = 0; k < runSize; k++)
                                meld.Add(cardsOfSuit[j + k]);
                            candidates.Add(meld);
                        }
                    }
                }
                i = end;
            }
        }

        // Set'ler: Aynı numaraya sahip kartlar.
        Dictionary<int, List<Card>> numberGroups = new Dictionary<int, List<Card>>();
        for (int i = 0; i < n; i++)
        {
            int number = myDeck[i].GetCardData().Number;
            if (!numberGroups.ContainsKey(number))
                numberGroups[number] = new List<Card>();
            numberGroups[number].Add(myDeck[i]);
        }
        foreach (var kvp in numberGroups)
        {
            List<Card> cardsOfNumber = kvp.Value;
            if (cardsOfNumber.Count >= 3)
            {
                List<List<Card>> combinations3 = GenerateCombinations(cardsOfNumber, 3);
                candidates.AddRange(combinations3);
                if (cardsOfNumber.Count >= 4)
                {
                    List<List<Card>> combinations4 = GenerateCombinations(cardsOfNumber, 4);
                    candidates.AddRange(combinations4);
                }
            }
        }
        return candidates;
    }

    // Verilen listeden targetCount eleman içeren tüm kombinasyonları üretir.
    private List<List<Card>> GenerateCombinations(List<Card> list, int targetCount)
    {
        List<List<Card>> results = new List<List<Card>>();
        GenerateCombinationsRecursive(list, targetCount, 0, new List<Card>(), results);
        return results;
    }

    private void GenerateCombinationsRecursive(List<Card> list, int targetCount, int start, List<Card> current, List<List<Card>> results)
    {
        if (targetCount == 0)
        {
            results.Add(new List<Card>(current));
            return;
        }
        for (int i = start; i < list.Count; i++)
        {
            current.Add(list[i]);
            GenerateCombinationsRecursive(list, targetCount - 1, i + 1, current, results);
            current.RemoveAt(current.Count - 1);
        }
    }

    #endregion
}
