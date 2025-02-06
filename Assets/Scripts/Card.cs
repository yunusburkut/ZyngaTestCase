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
    private Image _image;// We don't want to take reference type on struct because of the heap usage
    private RectTransform _cachedRectTransform;
    private Vector2 originalPosition;
    private int originalSiblingIndex;
    private Vector2 pointerOffset;
    private void Awake()
    {
        _image = GetComponent<Image>(); //Run once 
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
        if (_cardData.Number>10)
        {
            return 10;
        }
        else
        {
            return _cardData.Number;
        }
    }
    public void SetGroupID(byte groupID)
    {
        _cardData.GroupID = groupID;
        if (groupID==1)
        {
            _image.color = Color.green;
        }
        else if (groupID == 2)
        {
            _image.color = Color.red;
        }
        else if (groupID == 3)
        {
            _image.color = Color.blue;
        }
        else if (groupID == 4)
        {
            _image.color = Color.cyan;
        }
        else if (groupID == 5)
        {
            _image.color = Color.magenta;
        }
        else if (groupID == 6)
        {
            _image.color = Color.yellow;
        }
        else if (groupID == 7)
        {
            _image.color = Color.gray;
        }
        else if (groupID == 8)
        {
            _image.color = Color.black;
        }
        else if (groupID == 0)
        {
            _image.color = Color.white;
        }
    }
    public CardData GetCardData()
    {
        return _cardData;
    }

    public void setCardData(CardData cardData)
    {
        _cardData = cardData;
    }
   

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Kart üzerindeki pointer konumunu alıp offset hesaplıyoruz.
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
            _cachedRectTransform.anchoredPosition = localPoint - pointerOffset;
        }
        MyDeckManager.Instance.UpdateCardsAnimationDuringDrag(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _cachedRectTransform.DOKill();
        // Kartın bırakıldığı ekran noktasını da gönderiyoruz.
        MyDeckManager.Instance.OnCardDragEnd(this, eventData.position, eventData.pressEventCamera);
    }
}