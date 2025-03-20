using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class PruebaAnim : MonoBehaviour
{
    public Transform endPos;

    public float throwDuration = 0.5f;
    
    public Transform cardParent; // Objeto padre que contiene las cartas
    public Vector3 tablePosition; // Posición final en la mesa
    public int gridRows = 7;
    public int gridColumns = 7;
    public float gridSpacing = 1.5f;

    async void Start()
    {
        await Pruebas();
        Debug.Log("Ha terminao");
    }

    async Task Pruebas()
    {
        const float animDuration = 0.5f;
        const float cardHeight = 1f;
        List<Task> tasks = new();
        
        List<Transform> cards = cardParent.GetComponentsInChildren<Transform>().Where(card => card != cardParent).ToList();

        int index = 0;
        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridColumns; j++)
            {
                if (index >= cards.Count) break;
                Vector3 targetPos = cardParent.position + new Vector3(j * gridSpacing, 0, i * gridSpacing);
                tasks.Add(cards[index].DOMove(targetPos, animDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion());
                index++;
            }
        }
        
        await Task.WhenAll(tasks);
        //await Task.Delay((int)(animDuration * 1000));
        
        cards = cards.OrderBy(x => Random.value).ToList();
        
        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 targetPos = cardParent.position + new Vector3(0, i * (cardHeight + 0.1f), 0);
            tasks.Add(cards[i].DOMove(targetPos, animDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion());
        }
        await Task.WhenAll(tasks);
        
        //await Task.Delay((int)(animDuration * 1000));
        
        tasks.Add(cardParent.DOMove(tablePosition, animDuration).SetEase(Ease.InOutQuad).AsyncWaitForCompletion());
        await Task.WhenAll(tasks);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void ThrowCard(Vector3 destination)
    {
        transform.DOMove(destination + new Vector3(Random.Range(-0.05f, 0.05f), 0f, Random.Range(-0.05f, 0.05f)), throwDuration).SetEase(Ease.OutSine);
    }
    
    public void DrawCard(Vector3 destination)
    {
        transform.DOMove(destination + new Vector3(Random.Range(-0.05f, 0.05f), 0f, Random.Range(-0.05f, 0.05f)), throwDuration).SetEase(Ease.OutSine);
        transform.DORotate(new Vector3 (0, transform.rotation.eulerAngles.y + Random.Range(-15, 15), 180), throwDuration/2).SetEase(Ease.OutQuad);
    }
}
