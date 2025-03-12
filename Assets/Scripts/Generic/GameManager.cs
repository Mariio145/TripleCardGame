using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

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
    private List<Player> _players;
    private IGameState _gameState;
    private IForwardModel _forwardModel;
    public static int TimeToThink { get; private set; }

    public void Start()
    {
        DOTween.SetTweensCapacity(500, 50);
        TimeToThink = timeToThink;
        _players = new List<Player> {perspectivePlayer.GetComponent<Player>()};

        foreach (GameObject player in players)
        {
            _players.Add(player.GetComponent<Player>());
        }
        
        StartPlaying();
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
        _gameState.Reset(_players);
        _ = CheckPlayers();

        await Task.Delay(1000);

        while (!_gameState.IsTerminal() || true)
        {
            await Task.Delay(250);
            //await WaitForSpace();
            Player playerTurn = _gameState.GetPlayer();

            Debug.Log(playerTurn.Name);
            
            //Debug.Log("Antes de obtener observation");
            //await CheckPlayers();
            IObservation observation = _gameState.GetObservationFromPlayer(playerTurn.index);
            IAction action = null;
            
            //Debug.Log("Antes de pensar");
            //await CheckPlayers();

            Task<IAction> task = GetActionFromPlayer(observation, playerTurn);
            Task delayTask = Task.Delay(timeToThink * 1000);

            Task firstFinished = await Task.WhenAny(task, delayTask);

            if (firstFinished == task) // Si la tarea principal termina antes del timeout
                action = task.Result;
            
            Debug.Log(action);
            //await CheckPlayers();
            await _forwardModel.PlayAction(_gameState, action);
            //Debug.Log("ActionPlayed");
            //await CheckPlayers();
        }

        Debug.Log("HAY GANADOR!");

        await WaitForSpace();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private async Task<IAction> GetActionFromPlayer(IObservation observation, Player player)
    {
        return await Task.Run(() => player.Think(observation, timeToThink));
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