using UnityEngine;

public class InputTestManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DrawAndLogCard();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            NumberSortTest();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            SuitSortTest();
        }
    }

    public void OnDrawCardButtonPressed()
    {
        DrawAndLogCard();
    }
    private void SuitSortTest()
    {
        MyDeckManager.Instance.SortAndRepositionDeckBySuit();
    }
    private void NumberSortTest()
    {
        MyDeckManager.Instance.SortAndRepositionDeckByNumber();
    }
    private void DrawAndLogCard()
    {
       BoardManager.Instance.DrawCard();
    }
}