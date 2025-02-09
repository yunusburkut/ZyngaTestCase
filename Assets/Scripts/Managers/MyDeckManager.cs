using UnityEngine;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;

public class MyDeckManager : MonoBehaviour
{
    [SerializeField] private List<Card> myDeck = new List<Card>();
    [SerializeField] private float cardSpacing = 30f;
    [SerializeField] private float shiftDuration = 0.5f;
    [SerializeField] private float spacing = 50f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private TextMeshProUGUI deadwoodText;
    
    public static MyDeckManager Instance;

    private CardLayoutController layoutController;
    private GroupCalculator groupCalculator;
    private MeldOptimizer meldOptimizer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Bağımlılıklar constructor aracılığıyla veya burada initialize ediliyor.
        layoutController = new CardLayoutController(cardSpacing, shiftDuration, spacing, moveDuration);
        groupCalculator = new GroupCalculator();
        meldOptimizer = new MeldOptimizer();
    }

    public void AddCard(Card card)
    {
        if (card != null)
        {
            myDeck.Add(card);
            UpdateDeckLayout();
            UpdateDeadwoodUI(groupCalculator.CalculateDeadwood(myDeck));
        }
    }

    public void UpdateDeckLayout()
    {
        layoutController.UpdateLayout(myDeck);
    }

    public void SortDeckBySuitThenNumber()
    {
        myDeck.Sort(CardComparer.CompareBySuitThenNumber);
        UpdateDeckLayout();
    }

    public void SortDeckByNumberThenSuit()
    {
        myDeck.Sort(CardComparer.CompareByNumberThenSuit);
        UpdateDeckLayout();
    }

    public void MarkGroups()
    {
        groupCalculator.MarkGroups(myDeck);
        layoutController.RepositionCards(myDeck);
    }

    public void CalculateRun()
    {
        groupCalculator.CalculateRun(myDeck);
        UpdateDeadwoodUI(groupCalculator.GetDeadwood());
        layoutController.RepositionDpGroupedCardsLeftAligned(myDeck);
    }

    public void CalculateSet()
    {
        groupCalculator.CalculateSet(myDeck);
        UpdateDeadwoodUI(groupCalculator.GetDeadwood());
        layoutController.RepositionDpGroupedCardsLeftAligned(myDeck);
        
    }
    
    public void ComputeOptimalMelds()
    {
        MeldOptimizer.OptimalResult result = meldOptimizer.ComputeOptimalMelds(myDeck);
        byte groupId = 1;
        foreach (List<Card> meld in result.Melds)
        {
            foreach (Card card in meld)
            {
                card.SetGroupID(groupId);
            }
            groupId++;
        }
        layoutController.RepositionDpGroupedCardsLeftAligned(myDeck);
        UpdateDeadwoodUI(result.Deadwood);
        Debug.Log("Deadwood : " + result.Deadwood);
    }

    public void OnCardDragEnd(Card draggedCard, Vector2 screenPosition, Camera eventCamera)
    {
        layoutController.HandleDragEnd(myDeck, draggedCard, screenPosition, eventCamera);
        MarkGroups();
    }

    public void UpdateCardsAnimationDuringDrag(Card draggedCard)
    {
        layoutController.UpdateCardsAnimationDuringDrag(myDeck, draggedCard);
    }

    public List<Card> GetDeck() => myDeck;

    public void LogDeck()
    {
        StringBuilder sb = new StringBuilder("Deck: ");
        for (int i = 0; i < myDeck.Count; i++)
        {
            var data = myDeck[i].GetCardData();
            sb.Append($"[{i}] Number:{data.Number}, Suit:{data.Suit}  ");
        }
        Debug.Log(sb.ToString());
    }
    
    private void UpdateDeadwoodUI(int deadwood)
    {
        if (deadwoodText != null)
        {
            deadwoodText.text = "Deadwood: " + deadwood.ToString();
        }
        else
        {
            Debug.Log("Deadwood: " + deadwood);
        }
    }
}
