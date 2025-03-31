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
using Random = UnityEngine.Random;

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
    //[SerializeField] private GameObject perspectivePlayer;
    [SerializeField] private GameObject[] players;
    /*[SerializeField] private Player humanPlayer;
    [SerializeField] private List<Player> botPlayers;*/
    [Header("Miscellaneous")]
    [SerializeField] private Text endText;
    [SerializeField] private Text endTextShadow;
    [SerializeField] private Image timeCounter;

    public static Text EndText;
    public static Text EndTextShadow;
    
    private bool _stop;
    
    private List<Player> _players;
    private IGameState _gameState;
    private IForwardModel _forwardModel;
    private static CancellationTokenSource _cancellationTokenSource;
    public static int TimeToThink { get; private set; }

    public void Start()
    {
        EndText = endText;
        EndTextShadow = endTextShadow;
        _cancellationTokenSource = new CancellationTokenSource();
        DOTween.SetTweensCapacity(500, 50);
        TimeToThink = timeToThink;
        _players = new List<Player>();
        

        foreach (GameObject player in players)
        {
            Player[] playerScripts = player.GetComponents<Player>();
            if (playerScripts.Length <= 0) continue;
            
            if (playerScripts.Count(script => script.enabled) == 1)
            {
                foreach (Player script in playerScripts)
                {
                    if (script.enabled) _players.Add(script); // Si solo hay un script activado, elige 
                }
            }
            else
            {
                Player selectedScript = playerScripts[Random.Range(0, playerScripts.Length)];
                selectedScript.enabled = true;
                _players.Add(selectedScript); //De todos los jugadores que tiene el script, elige cualquiera aleatoriamente
            }
        }

        if (_players.Count < 2)
        {
            EndText.text = "Faltan jugadores";
            EndTextShadow.text = "Faltan jugadores";
        }
        else
        {
            StartPlaying();
        }
    }
    
    private void OnApplicationQuit()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        Application.Quit();
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
                Player playerTurn = _gameState.GetPlayer();
                playerTurn.StartTurn();

                Debug.Log(playerTurn.Name);
                await Task.Delay(250, _cancellationTokenSource.Token);
                
                IObservation observation = _gameState.GetObservationFromPlayer(playerTurn.index);
                IAction action = null;

                Task<IAction> task = GetActionFromPlayer(observation, playerTurn);
                Task delayTask = Task.Delay(timeToThink * 1000, _cancellationTokenSource.Token);
                ResetTimer();

                Task firstFinished = await Task.WhenAny(task, delayTask);
                
                _stop = true;
                
                if (firstFinished == task) // Si la tarea principal termina antes del timeout
                    action = task.Result;
                
                await _forwardModel.PlayAction(_gameState, action);
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

    private async void ResetTimer()
    {
        float timeRemaining = TimeToThink;
        timeCounter.fillAmount = 1;
        _stop = false;
        
        while (timeRemaining >= 0 && !_stop)
        {
            await Task.Delay(1000);
            timeRemaining -= 1f;
            timeCounter.fillAmount = timeRemaining/TimeToThink;
        }
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