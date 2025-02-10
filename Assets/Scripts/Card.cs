using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A struct that holds the basic data for a card, including its number, suit, and group ID.
/// The GroupID is used to identify which meld (group of cards) the card belongs to.
/// </summary>
public struct CardData
{
    public byte Number;   // The numerical value of the card (1 to 13)
    public byte Suit;     // The suit of the card (e.g., 0-3 for four suits)
    public byte GroupID;  // The group ID assigned after grouping cards into melds

    public CardData(byte number, byte suit, byte groupID)
    {
        Number = number;
        Suit = suit;
        GroupID = groupID;
    }
}

/// <summary>
/// The Card class represents an individual card in the game. It derives from MonoBehaviour
/// and implements the IBeginDragHandler, IDragHandler, and IEndDragHandler interfaces
/// to support drag-and-drop interactions.
/// </summary>
public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Private fields to store the card's data and UI components.
    private CardData _cardData;                       // Holds the card's number, suit, and group information.
    private Image _image;                             // The UI Image component used to display the card's sprite.
    private RectTransform _cachedRectTransform;       // Cached reference to the RectTransform for performance.
    private Vector2 pointerOffset;                    // The offset between the card's position and the pointer during drag.
    private float originalY;                          // The original Y position of the card before dragging.

    // Static array of colors that correspond to different GroupIDs.
    // When a card is assigned to a group, its image color changes based on its GroupID.
    private static readonly Color[] GroupColors = new Color[]
    {
        Color.white,    // GroupID 0 (ungrouped)
        Color.green,    // GroupID 1
        Color.red,      // GroupID 2
        Color.blue,     // GroupID 3
        Color.cyan,     // GroupID 4
        Color.magenta,  // GroupID 5
        Color.yellow,   // GroupID 6
        Color.gray,     // GroupID 7
        Color.black     // GroupID 8 (if needed)
    };

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Here, we cache references to the Image and RectTransform components for performance.
    /// </summary>
    private void Awake()
    {
        _image = GetComponent<Image>();
        _cachedRectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Public property to get the cached RectTransform.
    /// </summary>
    public RectTransform CachedRectTransform => _cachedRectTransform;

    /// <summary>
    /// Initializes the card with the specified CardData.
    /// Retrieves the corresponding sprite from the CardAtlasManager using the card's suit and number.
    /// </summary>
    /// <param name="cardData">The data containing the card's number, suit, and group.</param>
    public void Initialize(CardData cardData)
    {
        _cardData = cardData;
        // Get the appropriate sprite from the CardAtlasManager.
        Sprite sprite = CardAtlasManager.Instance.GetCardSprite(cardData.Suit, cardData.Number);
        _image.sprite = sprite;
    }

    /// <summary>
    /// Returns the point value of the card.
    /// For cards with a number greater than 10, it returns 10 points.
    /// Otherwise, it returns the card's number.
    /// </summary>
    public byte GetPoint()
    {
        return _cardData.Number > 10 ? (byte)10 : _cardData.Number;
    }

    /// <summary>
    /// Sets the GroupID for the card and updates its color based on the GroupColors array.
    /// If the groupID is out of range, the card's color is set to white.
    /// </summary>
    /// <param name="groupID">The group ID to assign to the card.</param>
    public void SetGroupID(byte groupID)
    {
        _cardData.GroupID = groupID;
        _image.color = groupID < GroupColors.Length ? GroupColors[groupID] : Color.white;
    }

    /// <summary>
    /// Returns the current CardData for the card.
    /// </summary>
    public CardData GetCardData()
    {
        return _cardData;
    }

    /// <summary>
    /// Sets the card's data. (Optional, if you need to update card data after initialization.)
    /// </summary>
    /// <param name="cardData">The new card data to set.</param>
    public void SetCardData(CardData cardData)
    {
        _cardData = cardData;
    }

    /// <summary>
    /// Called when a drag operation begins on the card.
    /// Caches the card's original Y position and calculates the pointer offset,
    /// then brings the card to the front and stops any existing tweens.
    /// </summary>
    /// <param name="eventData">The pointer event data for the drag.</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Cache the original Y position of the card.
        originalY = _cachedRectTransform.anchoredPosition.y;
        // Calculate the offset between the pointer position and the card's local position.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _cachedRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
        // Bring this card to the front in the UI hierarchy.
        _cachedRectTransform.SetAsLastSibling();
        // Kill any existing tweens to prevent conflicts.
        _cachedRectTransform.DOKill();
    }

    /// <summary>
    /// Called during a drag operation.
    /// Updates the card's X position based on the pointer movement while keeping its Y position fixed.
    /// Also triggers an update of the other cards' animations.
    /// </summary>
    /// <param name="eventData">The pointer event data for the drag.</param>
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        // Get the parent RectTransform to convert the screen point into a local point.
        RectTransform parentRect = _cachedRectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Calculate the new X position for the card by subtracting the pointer offset.
            float newX = localPoint.x - pointerOffset.x;
            // Update the card's position while keeping its original Y position.
            _cachedRectTransform.anchoredPosition = new Vector2(newX, originalY);
        }
        // Notify the deck manager to update the animation of other cards during drag.
        MyDeckManager.Instance.UpdateCardsAnimationDuringDrag(this);
    }

    /// <summary>
    /// Called when a drag operation ends.
    /// Stops any tweens on the card and notifies the deck manager about the drag end event,
    /// so that the deck order can be updated.
    /// </summary>
    /// <param name="eventData">The pointer event data for the drag end.</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        // Stop any active tweens on this card.
        _cachedRectTransform.DOKill();
        // Notify MyDeckManager that the drag operation has ended, providing the drop position.
        MyDeckManager.Instance.OnCardDragEnd(this, eventData.position, eventData.pressEventCamera);
    }
}
