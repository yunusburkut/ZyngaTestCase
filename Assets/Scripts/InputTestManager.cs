using UnityEngine;

public class InputTestManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < 10; i++)
            {
                DrawAndLogCard();
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SuitSortTest();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            NumberSortTest();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            SmartSort();
        }
    }

    public void OnDrawCardButtonPressed()
    {
        DrawAndLogCard();
    }
    private void SuitSortTest()
    {
        MyDeckManager.Instance.CalculateSet();
    }
    private void NumberSortTest()
    {
       MyDeckManager.Instance.CalculateRun();
    }
    private void SmartSort()
    { 
        MyDeckManager.Instance.ComputeOptimalMelds();
    }
    private void DrawAndLogCard()
    {
       BoardManager.Instance.DrawCard();
       
    }
}