using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardLayoutController
{
    // Configuration values for layout and animations.
    private readonly float cardSpacing;      // Space between cards in layout
    private readonly float shiftDuration;    // Duration for the layout shift animation
    private readonly float spacing;          // Spacing used for repositioning cards after grouping
    private readonly float moveDuration;     // Duration for the repositioning animation
    private readonly float dragOffsetY;      // Vertical offset applied during drag animation

    // Caches to reuse lists and reduce allocations during grouping and layout.
    private readonly Dictionary<int, List<Card>> _groupedCardsCache = new Dictionary<int, List<Card>>();
    private readonly List<Card> _ungroupedCardsCache = new List<Card>();
    private readonly List<int> _sortedGroupIDsCache = new List<int>();
    private readonly List<Card> _newOrderCache = new List<Card>();

    /// <summary>
    /// Constructor to initialize layout parameters.
    /// </summary>
    /// <param name="cardSpacing">Spacing between cards when initially laying them out.</param>
    /// <param name="shiftDuration">Duration of the shift animation.</param>
    /// <param name="spacing">Spacing between cards when repositioning after grouping.</param>
    /// <param name="moveDuration">Duration of the repositioning animation.</param>
    /// <param name="dragOffsetY">Vertical offset applied to non-dragged cards during a drag operation.</param>
    public CardLayoutController(float cardSpacing, float shiftDuration, float spacing, float moveDuration, float dragOffsetY = 20f)
    {
        this.cardSpacing = cardSpacing;
        this.shiftDuration = shiftDuration;
        this.spacing = spacing;
        this.moveDuration = moveDuration;
        this.dragOffsetY = dragOffsetY;
    }

    /// <summary>
    /// Updates the layout of the deck using the initial card spacing and shift duration.
    /// </summary>
    /// <param name="deck">The list of cards to layout.</param>
    public void UpdateLayout(List<Card> deck)
    {
        ApplyLayout(deck, cardSpacing, shiftDuration, updateSiblingIndices: false);
    }

    /// <summary>
    /// Repositions the cards with a specified spacing and move duration, updating their sibling order.
    /// </summary>
    /// <param name="deck">The list of cards to reposition.</param>
    public void RepositionCards(List<Card> deck)
    {
        ApplyLayout(deck, spacing, moveDuration, updateSiblingIndices: true);
    }

    /// <summary>
    /// Repositions the deck after grouping operations. The cards are rearranged based on their groupings.
    /// Ungrouped cards are added at the end.
    /// </summary>
    /// <param name="deck">The deck to reposition.</param>
    public void RepositionDpGroupedCardsLeftAligned(List<Card> deck)
    {
        // Get the new order based on grouping.
        List<Card> newOrder = GetDPGroupedOrder(deck);
        // Reposition using the defined spacing and move duration, and update sibling indices.
        ApplyLayout(newOrder, spacing, moveDuration, updateSiblingIndices: true);
        // Clear the original deck and refill it with the new order.
        deck.Clear();
        deck.AddRange(newOrder);
    }

    /// <summary>
    /// Handles the drag end event for a card.
    /// It calculates the new index based on the drop position and updates the deck order accordingly.
    /// </summary>
    /// <param name="deck">The current deck of cards.</param>
    /// <param name="draggedCard">The card that was dragged.</param>
    /// <param name="screenPosition">The drop screen position.</param>
    /// <param name="eventCamera">The camera used for the event.</param>
    public void HandleDragEnd(List<Card> deck, Card draggedCard, Vector2 screenPosition, Camera eventCamera)
    {
        // Get the parent RectTransform (i.e., the deck container).
        RectTransform deckRect = draggedCard.CachedRectTransform.parent as RectTransform;
        if (deckRect == null) return;

        // Convert the screen position to a local point within the deck container.
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(deckRect, screenPosition, eventCamera, out Vector2 localPoint))
            return;

        float dropX = localPoint.x;
        int count = deck.Count;
        // Calculate starting X coordinate for layout.
        float startX = CalculateStartX(count, spacing);
        // Determine the new index based on drop position and spacing.
        int newIndex = Mathf.RoundToInt((dropX - startX) / spacing);
        newIndex = Mathf.Clamp(newIndex, 0, count - 1);
        
        // Remove the dragged card and insert it at the new index.
        if (deck.Contains(draggedCard))
            deck.Remove(draggedCard);
        deck.Insert(newIndex, draggedCard);
    }

    /// <summary>
    /// Updates the animation of all cards during a drag operation.
    /// Non-dragged cards animate to either an offset position (if near the dragged card) or to their default position.
    /// </summary>
    /// <param name="deck">The deck of cards.</param>
    /// <param name="draggedCard">The card currently being dragged.</param>
    public void UpdateCardsAnimationDuringDrag(List<Card> deck, Card draggedCard)
    {
        int count = deck.Count;
        if (count == 0) return;

        // Calculate the starting X coordinate for card layout.
        float startX = CalculateStartX(count, spacing);
        // Define a threshold distance for applying a vertical offset.
        float threshold = spacing * 0.5f;
        // Get the current X position of the dragged card.
        float draggedX = draggedCard.CachedRectTransform.anchoredPosition.x;

        // Loop through all cards to update their positions.
        for (int i = 0; i < count; i++)
        {
            Card card = deck[i];
            if (card == draggedCard) continue;

            RectTransform rt = card.CachedRectTransform;
            if (rt == null) continue;

            // Calculate the default X position based on index and spacing.
            float defaultX = startX + i * spacing;
            // Calculate the absolute difference between the default position and dragged card's X.
            float diff = Mathf.Abs(defaultX - draggedX);
            // Determine the target Y position: if within threshold, apply dragOffsetY; otherwise, use 0.
            Vector2 targetPos = (diff < threshold) ? new Vector2(defaultX, dragOffsetY) : new Vector2(defaultX, 0f);
            // Animate the card to the new position over the specified duration.
            AnimateCard(card, targetPos, 0.2f);
        }
    }

    /// <summary>
    /// Calculates the starting X coordinate for laying out a given number of cards with a specified spacing.
    /// The layout is centered horizontally.
    /// </summary>
    /// <param name="count">The number of cards in the layout.</param>
    /// <param name="spacingValue">The spacing between cards.</param>
    /// <returns>The starting X coordinate.</returns>
    private float CalculateStartX(int count, float spacingValue)
    {
        return -((count - 1) * spacingValue) / 2f;
    }

    /// <summary>
    /// Animates the given card's RectTransform to the target position over the given duration using DOTween.
    /// </summary>
    /// <param name="card">The card to animate.</param>
    /// <param name="targetPos">The target anchored position.</param>
    /// <param name="duration">The duration of the animation.</param>
    private void AnimateCard(Card card, Vector2 targetPos, float duration)
    {
        RectTransform rt = card.CachedRectTransform;
        if (rt != null)
        {
            // Kill any existing tween on the RectTransform to prevent conflicts.
            rt.DOKill();
            // Animate to the target position with a smooth easing.
            rt.DOAnchorPos(targetPos, duration).SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// Applies a layout to the given deck of cards by positioning them horizontally based on spacing.
    /// Optionally updates the sibling indices in the UI hierarchy.
    /// </summary>
    /// <param name="deck">The list of cards to layout.</param>
    /// <param name="spacingValue">The spacing between cards.</param>
    /// <param name="duration">The duration of the animation.</param>
    /// <param name="updateSiblingIndices">If true, update the transform sibling indices to match the order.</param>
    private void ApplyLayout(List<Card> deck, float spacingValue, float duration, bool updateSiblingIndices)
    {
        int count = deck.Count;
        if (count == 0) return;

        float startX = CalculateStartX(count, spacingValue);
        for (int i = 0; i < count; i++)
        {
            if (updateSiblingIndices)
                deck[i].transform.SetSiblingIndex(i);
            // Animate each card to its target position.
            AnimateCard(deck[i], new Vector2(startX + i * spacingValue, 0), duration);
        }
    }

    /// <summary>
    /// Generates a new order for the deck based on groupings.
    /// Cards with a non-zero GroupID are grouped and sorted by number,
    /// then the ungrouped cards are appended at the end.
    /// </summary>
    /// <param name="deck">The original deck of cards.</param>
    /// <returns>A new list of cards arranged by groups followed by ungrouped cards.</returns>
    private List<Card> GetDPGroupedOrder(List<Card> deck)
    {
        // Clear all caches for a fresh grouping.
        _groupedCardsCache.Clear();
        _ungroupedCardsCache.Clear();
        _sortedGroupIDsCache.Clear();
        _newOrderCache.Clear();

        int deckCount = deck.Count;
        // Group cards based on their GroupID.
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

        // Collect all group IDs from the grouped cards.
        foreach (var pair in _groupedCardsCache)
            _sortedGroupIDsCache.Add(pair.Key);
        _sortedGroupIDsCache.Sort();

        int sortedCount = _sortedGroupIDsCache.Count;
        // For each group (by GroupID), sort the cards by number and add them to the new order.
        for (int i = 0; i < sortedCount; i++)
        {
            int id = _sortedGroupIDsCache[i];
            _groupedCardsCache[id].Sort(CompareCardNumber);
            _newOrderCache.AddRange(_groupedCardsCache[id]);
        }
        // Append any ungrouped cards at the end.
        _newOrderCache.AddRange(_ungroupedCardsCache);
        return _newOrderCache;
    }

    /// <summary>
    /// Compares two cards based on their number.
    /// This is used for sorting cards within a group.
    /// </summary>
    private static int CompareCardNumber(Card a, Card b)
    {
        return a.GetCardData().Number.CompareTo(b.GetCardData().Number);
    }
}
