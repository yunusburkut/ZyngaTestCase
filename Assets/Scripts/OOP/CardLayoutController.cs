using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardLayoutController
{
    private float cardSpacing;
    private float shiftDuration;
    private float spacing;
    private float moveDuration;
    private float dragOffsetY = 20f;

    public CardLayoutController(float cardSpacing, float shiftDuration, float spacing, float moveDuration)
    {
        this.cardSpacing = cardSpacing;
        this.shiftDuration = shiftDuration;
        this.spacing = spacing;
        this.moveDuration = moveDuration;
    }

    public void UpdateLayout(List<Card> deck)
    {
        int count = deck.Count;
        if (count == 0) return;
        float startX = -((count - 1) * cardSpacing) / 2f;
        for (int i = 0; i < count; i++)
        {
            Card card = deck[i];
            RectTransform rt = card.CachedRectTransform;
            if (rt != null)
            {
                Vector2 targetPos = new Vector2(startX + i * cardSpacing, 0);
                rt.DOKill();
                rt.DOAnchorPos(targetPos, shiftDuration).SetEase(Ease.OutQuad);
            }
        }
    }

    public void RepositionCards(List<Card> deck)
    {
        int count = deck.Count;
        if (count == 0) return;
        float startX = -((count - 1) * spacing) / 2f;
        for (int i = 0; i < count; i++)
        {
            Card card = deck[i];
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

    public void HandleDragEnd(List<Card> deck, Card draggedCard, Vector2 screenPosition, Camera eventCamera)
    {
        RectTransform deckRect = draggedCard.CachedRectTransform.parent as RectTransform;
        if (deckRect == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(deckRect, screenPosition, eventCamera, out Vector2 localPoint))
            return;

        float dropX = localPoint.x;
        int count = deck.Count;
        float startX = -((count - 1) * spacing) / 2f;
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
        float startX = -((count - 1) * spacing) / 2f;
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
            rt.DOKill();
            rt.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad);
        }
    }
}
