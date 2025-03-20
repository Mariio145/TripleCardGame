using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum GameToPlay
{
    VirusGame,
    UnoGame,
    KittensGame
}

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private GameToPlay gameToPlay;
    [SerializeField] private GameObject deckHolder, discardHolder;
    [SerializeField] private int timeToThink;
    [Header("Player Settings")]
    [SerializeField] private GameObject perspectivePlayer;
    [SerializeField] private GameObject[] players;
    /*[SerializeField] private Player humanPlayer;
    [SerializeField] private List<Player> botPlayers;*/
    [Header("Miscellaneous")]
    [SerializeField] private Text endText;

    public static Text EndText;
    
    private List<Player> _players;
    private IGameState _gameState;
    private IForwardModel _forwardModel;
    private static CancellationTokenSource _cancellationTokenSource;
    public static int TimeToThink { get; private set; }

    public void Start()
    {
        EndText = endText;
        _cancellationTokenSource = new CancellationTokenSource();
        DOTween.SetTweensCapacity(500, 50);
        TimeToThink = timeToThink;
        _players = new List<Player>();

        foreach (Player humanPlayer in perspectivePlayer.GetComponents<Player>().Where(script => script.enabled))
        {
            _players.Add(humanPlayer);
        }

        foreach (GameObject player in players)
        {
            _players.Add(player.GetComponent<Player>());
        }
        
        StartPlaying();
    }
    
    private void OnApplicationQuit()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    private async Task CheckPlayers()
    {
        /*return Task.CompletedTask;
        if (_gameState.IsTerminal())
        {
            Debug.Log("El juego ha acabado");
            return Task.CompletedTask;
        }
        KittensGameState kgs = _gameState as KittensGameState;
        Debug.Log("Mano del jugador0: " + kgs.PlayersStatus[0].Hand.Count);
        Debug.Log("Mano del jugador1: " + kgs.PlayersStatus[1].Hand.Count);
        if (kgs.PlayersStatus.Any(player => player.Hand.Count == 0))
            Debug.Log("Se acaba de romper");
        return Task.CompletedTask;*/
    }

    private void StartPlaying()
    {
        switch (gameToPlay)
        {
            case GameToPlay.VirusGame:
                _gameState = new VirusGameState(deckHolder, discardHolder);
                _forwardModel = new VirusForwardModel();
                break;
            case GameToPlay.UnoGame:
                _gameState = new UnoGameState(deckHolder, discardHolder);
                _forwardModel = new UnoForwardModel();
                break;
            case GameToPlay.KittensGame:
                _gameState = new KittensGameState(deckHolder, discardHolder);
                _forwardModel = new KittensForwardModel();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        for (int index = 0; index < _players.Count; index++)
        {
            Player player = _players[index];
            player.index = index;
            player.SetHeuristic(gameToPlay);
            player.SetForwardModel(_forwardModel);
        }


        if(_players.Any(p => p != null)) Run();
        else Debug.Log("No hay players disponibles");
    }

    private async void Run()
    {
        try
        {
            _gameState.Reset(_players);

            await Task.Delay(1000, _cancellationTokenSource.Token);

            while (!_gameState.IsTerminal())
            {
                await Task.Delay(250, _cancellationTokenSource.Token);
                //await WaitForSpace();
                Player playerTurn = _gameState.GetPlayer();
                playerTurn.StartTurn();

                Debug.Log(playerTurn.Name);
                await Task.Delay(250, _cancellationTokenSource.Token);

                //Debug.Log("Antes de obtener observation");
                //await CheckPlayers();
                IObservation observation = _gameState.GetObservationFromPlayer(playerTurn.index);
                IAction action = null;

                //Debug.Log("Antes de pensar");
                //await CheckPlayers();

                Task<IAction> task = GetActionFromPlayer(observation, playerTurn);
                Task delayTask = Task.Delay(timeToThink * 1000, _cancellationTokenSource.Token);

                Task firstFinished = await Task.WhenAny(task, delayTask);

                if (firstFinished == task) // Si la tarea principal termina antes del timeout
                    action = task.Result;

                Debug.Log(action);
                //if (action is null) action = _gameState.GetDefaultAction();
                //await CheckPlayers();
                await _forwardModel.PlayAction(_gameState, action);
                //Debug.Log("ActionPlayed");
                //await CheckPlayers();
                playerTurn.StopTurn();
            }

            endText.text = "HAY GANADOR!\nPresiona el espacio para\nun nuevo juego.";

            await WaitForSpace();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Juego cancelado");
        }
    }

    private async Task<IAction> GetActionFromPlayer(IObservation observation, Player player)
    {
        return await Task.Run(() => player.Think(observation, timeToThink), _cancellationTokenSource.Token);
    }
    
    async Task WaitForSpace()
    {
        // Espera hasta que el jugador presione la tecla Espacio
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            await Task.Yield(); // Libera el control hasta el siguiente frame
        }
    }
}