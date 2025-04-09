using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VisualKittensAction : MonoBehaviour
{
    private int _playerIndex = -1;
    private static readonly Vector2 PlayerPosition1 = new (-250, -140);
    private static readonly Vector2 PlayerPosition2 = new (0, -180);
    private static readonly Vector2 PlayerPosition3 = new (250, -140);
    
    public GameObject playerIcon1, playerIcon2, playerIcon3;
    public GameObject selectPlayerGo;
    
    private readonly Dictionary<int, GameObject> _playerIcons = new();

    private void Start()
    {
        _playerIcons.Add(1, playerIcon1);
        _playerIcons.Add(2, playerIcon2);
        _playerIcons.Add(3, playerIcon3);
    }

    public async Task<int> SelectPlayerTarget(KittensObservation observable)
    {
        List<int> playersTarget = new();

        int index = 0;
        foreach (KittensPlayerStatus player in observable.PlayersStatus)
        {
            if (player != observable.PlayersStatus[observable.GetPlayerTurnIndex()] && player.Hand.Count > 1) playersTarget.Add(index);
            index++;
        }
        
        ShowPlayerCanvas(true);
        _playerIndex = -1;
        PlacePlayerButtons(playersTarget);
        while (_playerIndex == -1)
        {
            await Task.Yield();
        }
        ShowPlayerCanvas(false);
        return _playerIndex;
    }
    
    private void PlacePlayerButtons(List<int> players)
    {
        switch (players.Count)
        {
            case 1:
                _playerIcons[players[0]].GetComponent<RectTransform>().anchoredPosition = PlayerPosition2;
                break;
            case 2:
                _playerIcons[players[0]].GetComponent<RectTransform>().anchoredPosition = PlayerPosition1;
                _playerIcons[players[1]].GetComponent<RectTransform>().anchoredPosition = PlayerPosition3;
                break;
            case 3:
                _playerIcons[players[0]].GetComponent<RectTransform>().anchoredPosition = PlayerPosition1;
                _playerIcons[players[1]].GetComponent<RectTransform>().anchoredPosition = PlayerPosition2;
                _playerIcons[players[2]].GetComponent<RectTransform>().anchoredPosition = PlayerPosition3;
                break;
        }
        foreach (int playerIndex in players)
        {
            _playerIcons[playerIndex].SetActive(true);
        }
    }

    private void ShowPlayerCanvas(bool show)
    {
        foreach (GameObject objects in _playerIcons.Values)
        {
            objects.SetActive(false);
        }
        selectPlayerGo.SetActive(show);
    }

    public async Task<int> SelectCard(VisualKittensCard[] listCards)
    {
        foreach (VisualKittensCard card in listCards)
        {
            card.selected = false;
        }
        
        while (listCards.Count(card => card.selected) > 0)
        {
            await Task.Yield();
        }

        for (int index = 0; index < listCards.Length; index++)
        {
            if (listCards[index].selected) return index;
        }

        return -1;
    }
    
    public async void ExitGame()
    {
        GameManager.CancellationTokenSource.Cancel();
        await Task.Delay(1000);
        SceneManager.LoadScene(1);
    }
}
