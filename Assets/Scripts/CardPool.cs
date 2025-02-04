using System.Collections.Generic;
using UnityEngine;

public class CardPool : MonoBehaviour
{
    public GameObject cardPrefab;
    public int initialPoolSize = 20;
    
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            var cardObj = Instantiate(cardPrefab, transform);
            cardObj.SetActive(false);
            pool.Enqueue(cardObj);
        }
    }

    public GameObject GetCard()
    {
        if (pool.Count > 0)
        {
            var cardObj = pool.Dequeue();
            cardObj.SetActive(true);
            return cardObj;
        }
        return Instantiate(cardPrefab, transform);
    }

    public void ReturnCard(GameObject cardObj)
    {
        cardObj.SetActive(false);
        pool.Enqueue(cardObj);
    }
}