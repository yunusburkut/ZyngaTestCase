using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct CardData
{
    public byte Number;
    public byte Suit;
    public byte GroupID;

    public CardData(byte number, byte suit, byte groupID)
    {
        Number = number;
        Suit = suit;
        GroupID = groupID;
    }
}

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CardData _cardData;
    private Image _image;
    private RectTransform _cachedRectTransform;
    private Vector2 pointerOffset;
    private float originalY;

    // Kart görselleri için renk dizisi: GroupID 0 için beyaz, sonrasında farklı renkler.
    private static readonly Color[] GroupColors = new Color[]
    {
        Color.white,    // GroupID 0
        Color.green,    // GroupID 1
        Color.red,      // GroupID 2
        Color.blue,     // GroupID 3
        Color.cyan,     // GroupID 4
        Color.magenta,  // GroupID 5
        Color.yellow,   // GroupID 6
        Color.gray,     // GroupID 7
        Color.black     // GroupID 8
    };

    private void Awake()
    {
        _image = GetComponent<Image>();
        _cachedRectTransform = GetComponent<RectTransform>();
    }

    public RectTransform CachedRectTransform => _cachedRectTransform;

    /// <summary>
    /// CardData'yı atar ve CardAtlasManager üzerinden kart görselini çeker.
    /// </summary>
    public void Initialize(CardData cardData)
    {
        _cardData = cardData;
        // Kart görselini atlas üzerinden alıyoruz.
        Sprite sprite = CardAtlasManager.Instance.GetCardSprite(cardData.Suit, cardData.Number);
        _image.sprite = sprite;

        // GPU instancing destekleyen materyal kullanıyorsanız, burada materyalin GPU instancing ayarlarının açık olduğundan emin olun.
    }

    public byte GetPoint()
    {
        return _cardData.Number > 10 ? (byte)10 : _cardData.Number;
    }

    public void SetGroupID(byte groupID)
    {
        _cardData.GroupID = groupID;
        _image.color = groupID < GroupColors.Length ? GroupColors[groupID] : Color.white;
    }

    public CardData GetCardData()
    {
        return _cardData;
    }

    public void SetCardData(CardData cardData)
    {
        _cardData = cardData;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalY = _cachedRectTransform.anchoredPosition.y;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_cachedRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
        _cachedRectTransform.SetAsLastSibling();
        _cachedRectTransform.DOKill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransform parentRect = _cachedRectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            float newX = localPoint.x - pointerOffset.x;
            _cachedRectTransform.anchoredPosition = new Vector2(newX, originalY);
        }
        MyDeckManager.Instance.UpdateCardsAnimationDuringDrag(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _cachedRectTransform.DOKill();
        MyDeckManager.Instance.OnCardDragEnd(this, eventData.position, eventData.pressEventCamera);
    }
}
