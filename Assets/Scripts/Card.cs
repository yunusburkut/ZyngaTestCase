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

    public CardData GetCardData()
    {
        return _cardData;
    }
}