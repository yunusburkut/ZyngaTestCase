using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardLayoutController
{
    private readonly float cardSpacing;
    private readonly float shiftDuration;
    private readonly float spacing;
    private readonly float moveDuration;
    private readonly float dragOffsetY;

    private readonly Dictionary<int, List<Card>> _groupedCardsCache = new Dictionary<int, List<Card>>();
    private readonly List<Card> _ungroupedCardsCache = new List<Card>();
    private readonly List<int> _sortedGroupIDsCache = new List<int>();
    private readonly List<Card> _newOrderCache = new List<Card>();

    public CardLayoutController(float cardSpacing, float shiftDuration, float spacing, float moveDuration, float dragOffsetY = 20f)
    {
        this.cardSpacing = cardSpacing;
        this.shiftDuration = shiftDuration;
        this.spacing = spacing;
        this.moveDuration = moveDuration;
        this.dragOffsetY = dragOffsetY;
    }

    public void UpdateLayout(List<Card> deck)
    {
        ApplyLayout(deck, cardSpacing, shiftDuration, updateSiblingIndices: false);
    }

    public void RepositionCards(List<Card> deck)
    {
        ApplyLayout(deck, spacing, moveDuration, updateSiblingIndices: true);
    }

    public void RepositionDpGroupedCardsLeftAligned(List<Card> deck)
    {
        List<Card> newOrder = GetDPGroupedOrder(deck);
        ApplyLayout(newOrder, spacing, moveDuration, updateSiblingIndices: true);
        deck.Clear();
        deck.AddRange(newOrder);
    }

    public void HandleDragEnd(List<Card> deck, Card draggedCard, Vector2 screenPosition, Camera eventCamera)
    {
        RectTransform deckRect = draggedCard.CachedRectTransform.parent as RectTransform;
        if (deckRect == null) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(deckRect, screenPosition, eventCamera, out Vector2 localPoint))
            return;
        float dropX = localPoint.x;
        int count = deck.Count;
        float startX = CalculateStartX(count, spacing);
        int newIndex = Mathf.RoundToInt((dropX - startX) / spacing);
        newIndex = Mathf.Clamp(newIndex, 0, count - 1);
        if (deck.Contains(draggedCard))
            deck.Remove(draggedCard);
        deck.Insert(newIndex, draggedCard);
    }

    public void UpdateCardsAnimationDuringDrag(List<Card> deck, Card draggedCard)
    {
        int count = deck.Count;
        if (count == 0) return;
        float startX = CalculateStartX(count, spacing);
        float threshold = spacing * 0.5f;
        float draggedX = draggedCard.CachedRectTransform.anchoredPosition.x;
        for (int i = 0; i < count; i++)
        {
            Card card = deck[i];
            if (card == draggedCard) continue;
            RectTransform rt = card.CachedRectTransform;
            if (rt == null) continue;
            float defaultX = startX + i * spacing;
            float diff = Mathf.Abs(defaultX - draggedX);
            Vector2 targetPos = (diff < threshold) ? new Vector2(defaultX, dragOffsetY) : new Vector2(defaultX, 0f);
            AnimateCard(card, targetPos, 0.2f);
        }
    }

    private float CalculateStartX(int count, float spacingValue)
    {
        return -((count - 1) * spacingValue) / 2f;
    }

    private void AnimateCard(Card card, Vector2 targetPos, float duration)
    {
        RectTransform rt = card.CachedRectTransform;
        if (rt != null)
        {
            rt.DOKill();
            rt.DOAnchorPos(targetPos, duration).SetEase(Ease.OutQuad);
        }
    }

    private void ApplyLayout(List<Card> deck, float spacingValue, float duration, bool updateSiblingIndices)
    {
        int count = deck.Count;
        if (count == 0) return;
        float startX = CalculateStartX(count, spacingValue);
        for (int i = 0; i < count; i++)
        {
            if (updateSiblingIndices)
                deck[i].transform.SetSiblingIndex(i);
            AnimateCard(deck[i], new Vector2(startX + i * spacingValue, 0), duration);
        }
    }

    private List<Card> GetDPGroupedOrder(List<Card> deck)
    {
        _groupedCardsCache.Clear();
        _ungroupedCardsCache.Clear();
        _sortedGroupIDsCache.Clear();
        _newOrderCache.Clear();

        int deckCount = deck.Count;
        for (int i = 0; i < deckCount; i++)
        {
            Card card = deck[i];
            int groupId = card.GetCardData().GroupID;
            if (groupId > 0)
            {
                if (!_groupedCardsCache.TryGetValue(groupId, out List<Card> group))
                {
                    group = new List<Card>();
                    _groupedCardsCache.Add(groupId, group);
                }
                group.Add(card);
            }
            else
            {
                _ungroupedCardsCache.Add(card);
            }
        }

        foreach (var pair in _groupedCardsCache)
            _sortedGroupIDsCache.Add(pair.Key);
        _sortedGroupIDsCache.Sort();

        int sortedCount = _sortedGroupIDsCache.Count;
        for (int i = 0; i < sortedCount; i++)
        {
            int id = _sortedGroupIDsCache[i];
            _groupedCardsCache[id].Sort(CompareCardNumber);
            _newOrderCache.AddRange(_groupedCardsCache[id]);
        }
        _newOrderCache.AddRange(_ungroupedCardsCache);
        return _newOrderCache;
    }

    private static int CompareCardNumber(Card a, Card b)
    {
        return a.GetCardData().Number.CompareTo(b.GetCardData().Number);
    }
}
