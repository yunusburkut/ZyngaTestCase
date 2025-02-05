using UnityEngine;
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

public class Card : MonoBehaviour
{
    private CardData _cardData;
    private Image _image;// We don't want to take reference type on struct because of the heap usage

    private void Awake()
    {
        _image = GetComponent<Image>(); //Run once 
    }

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
}