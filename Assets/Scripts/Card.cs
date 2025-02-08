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

    // Statik renk dizisi: Index 0 için beyaz (grupsuz), 1..8 için diğer renkler.
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
        Color.black     // GroupID 8 (varsa)
    };

    private void Awake()
    {
        // Bileşenler yalnızca bir kere alınır.
        _image = GetComponent<Image>();
        _cachedRectTransform = GetComponent<RectTransform>();
    }

    public RectTransform CachedRectTransform => _cachedRectTransform;

    public void Initialize(CardData cardData, Sprite sprite)
    {
        _cardData = cardData;
        _image.sprite = sprite;
    }

    public byte GetPoint()
    {
        // 10'dan büyük değerler 10 olarak hesaplanır.
        return _cardData.Number > 10 ? (byte)10 : _cardData.Number;
    }

    public void SetGroupID(byte groupID)
    {
        _cardData.GroupID = groupID;
        // Eğer tanımlı aralık dışıysa varsayılan olarak beyaz yap.
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
        // Y pozisyonunu cache'leyelim.
        originalY = _cachedRectTransform.anchoredPosition.y;
        // Pointer offset'ini hesapla.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_cachedRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
        // Drag sırasında en üstte görünmesi için sibling index'i son yap.
        _cachedRectTransform.SetAsLastSibling();
        _cachedRectTransform.DOKill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransform parentRect = _cachedRectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Y sabit, sadece X değişir.
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
