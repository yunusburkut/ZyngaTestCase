using UnityEngine;

public class InputTestManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DrawAndLogCard();
        }
    }

    public void OnDrawCardButtonPressed()
    {
        DrawAndLogCard();
    }
    
    private void DrawAndLogCard()
    {
        Card drawnCard = BoardManager.Instance.DrawCard();
        
        if (drawnCard != null)
        {
            CardData cardData = drawnCard.GetCardData();
            
            Debug.Log("Çekilen Kart => Number: " + cardData.Number +
                      ", Suit: " + cardData.Suit +
                      ", GroupID: " + cardData.GroupID);
        }
        else
        {
            Debug.Log("Deste boş, kart çekilemiyor!");
        }
    }
}