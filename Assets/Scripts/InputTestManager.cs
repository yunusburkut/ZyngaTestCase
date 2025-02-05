using UnityEngine;

public class InputTestManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < 2; i++)
            {
                DrawAndLogCard();
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            NumberSortTest();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            SuitSortTest();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GROUP();
        }
    }

    public void OnDrawCardButtonPressed()
    {
        DrawAndLogCard();
    }
    private void SuitSortTest()
    {
        MyDeckManager.Instance.SortAndRepositionDeckBySuitAscending();
    }
    private void NumberSortTest()
    {
       MyDeckManager.Instance.SortAndRepositionDeckByNumberThenSuitAscending();
    }
    private void GROUP()
    {
      
    }
    private void DrawAndLogCard()
    {
       BoardManager.Instance.DrawCard();
       
    }
}