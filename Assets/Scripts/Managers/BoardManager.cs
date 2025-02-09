using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private Stack<Card> deckStack = new Stack<Card>();
    private List<Card> deckList = new List<Card>();
    [SerializeField] private CardPool cardPool;
    [SerializeField] private Sprite[] cardSprites;
    [SerializeField] private RectTransform destinationPanel;
    
    public static BoardManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        
    }

    public void Init()
    {
        CreateAndShuffleDeck();
    }
    private void CreateAndShuffleDeck()
    {
        for (byte suit = 0; suit < 4; suit++)
        {
            for (byte number = 1; number <= 13; number++)
            {
                CardData data = new CardData(number, suit, 0);
                Card card = cardPool.GetCard();
                
                int spriteIndex = suit * 13 + (number - 1);
                card.Initialize(data, cardSprites[spriteIndex]);
                
                deckList.Add(card);
            }
        }

        for (int i = deckList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (deckList[i], deckList[randomIndex]) = (deckList[randomIndex], deckList[i]);
        }
        for (int i = 0; i < deckList.Count; i++)
        {
            deckStack.Push(deckList[i]);
        }
        deckList.Clear();
    }

    public void DrawCard()
    {
        if (deckStack.Count == 0)
        {
            Debug.LogWarning("Deste bitti!");
        }
        Card card = deckStack.Pop();
        SendDrawedCardToDeck(card);
        MyDeckManager.Instance.AddCard(card);
    }

    public void SendDrawedCardToDeck(Card card)
    {
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.SetParent(destinationPanel, false);
        cardRect.DOAnchorPos(Vector2.zero, .5f).SetEase(Ease.OutQuad);
    }
}
