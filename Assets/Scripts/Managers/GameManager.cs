using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int cardsToDeal = 11;
    [SerializeField] private float drawDelay = .5f;

    private void Start()
    {
        BoardManager.Instance.Init();
        StartCoroutine(DealCardsRoutine());
    }
    
    private IEnumerator DealCardsRoutine()
    {
        for (int i = 0; i < cardsToDeal; i++)
        {
            BoardManager.Instance.DrawCard();
            yield return new WaitForSeconds(drawDelay);
        }
        Debug.Log("Tüm kartlar dağıtıldı.");
    }
}