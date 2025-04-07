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
    [SerializeField] private int botTimeToThink;
    [SerializeField] private int humanTimeToThink;
    [Header("Player Settings")]
    //[SerializeField] private GameObject perspectivePlayer;
    [SerializeField] private GameObject[] players;
    /*[SerializeField] private Player humanPlayer;
    [SerializeField] private List<Player> botPlayers;*/
    [Header("Miscellaneous")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image endIcon;
    [SerializeField] private Text endText;
    [SerializeField] private Text endTextShadow;
    [SerializeField] private Image timeCounter;
    [SerializeField] private GameObject tutorialGameObject;
    [SerializeField] private Image showCardRenderer;
    [SerializeField] private GameObject reverse;
    [SerializeField] private GameObject block;
    

    public static Text EndText;
    public static Text EndTextShadow;
    public static Image ShowCardRender;
    public static GameObject Reverse;
    public static GameObject Block;
    
    private bool _stop;
    
    private List<Player> _players;
    private IGameState _gameState;
    private IForwardModel _forwardModel;
    internal static CancellationTokenSource CancellationTokenSource;
    private FirebaseManager _firebaseManager;
    private string _gameID;
    private double _deltaTime;
    private bool _isPaused;

    public static bool IsHumanPlayer { get; private set; }
    public static int BotTimeToThink { get; private set; }
    public static int HumanTimeToThink { get; private set; }

    public void Start()
    {
        _firebaseManager = new FirebaseManager();
        EndText = endText;
        EndTextShadow = endTextShadow;
        ShowCardRender = showCardRenderer;
        Block = block;
        Reverse = reverse;
        CancellationTokenSource = new CancellationTokenSource();
        DOTween.SetTweensCapacity(500, 50);
        IsHumanPlayer = false;
        BotTimeToThink = botTimeToThink;
        HumanTimeToThink = humanTimeToThink;
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
        CancellationTokenSource?.Cancel();
        CancellationTokenSource?.Dispose();
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

            if (player is VirusHumanPlayer or UnoHumanPlayer or KittensHumanPlayer)
            {
                Debug.Log("Humanoooooo");
                IsHumanPlayer = true;
            }
        }


        if(_players.Any(p => p != null)) Run();
        else Debug.Log("No hay players disponibles");
    }

    private async void Run()
    {
        try
        {
            _gameState.Reset(_players);

            await Task.Delay(1000, CancellationTokenSource.Token);

            await Task.Delay(1000);

            while (!_gameState.IsTerminal() && CancellationTokenSource.Token.IsCancellationRequested == false)
            {
                await Task.Delay(250, CancellationTokenSource.Token);
                Player playerTurn = _gameState.GetPlayer();
                if (CancellationTokenSource.IsCancellationRequested) return;
                playerTurn.StartTurn();
                
                Debug.Log(playerTurn.Name);
                await Task.Delay(250, CancellationTokenSource.Token);
                
                IObservation observation = _gameState.GetObservationFromPlayer(playerTurn.index);
                IAction action = null;

                Task<IAction> task = GetActionFromPlayer(observation, playerTurn);
                int delay = IsHumanPlayer ? HumanTimeToThink * 1000 : BotTimeToThink * 1000;
                
                if (IsHumanPlayer && _gameState.GetPlayerTurnIndex() == 0)
                {
                    ShowTurnText();
                    ResetTimer();
                }
                
                while (!task.IsCompleted && delay > 0)
                {
                    await Task.Delay(100);
                    delay -= 100;
                    while (_isPaused)
                    {
                        await Task.Yield();
                    }
                }
                
                _stop = true;
                
                if (task.IsCompleted) // Si la tarea principal termina antes del timeout
                    action = task.Result;

                if (CancellationTokenSource.IsCancellationRequested) return;
                
                await _forwardModel.PlayAction(_gameState, action);
                playerTurn.StopTurn();
                
                while (_isPaused)
                {
                    await Task.Yield();
                }
            }
            
            while (_isPaused)
            {
                await Task.Yield();
            }
            
            if (CancellationTokenSource.IsCancellationRequested) return;

            ShowEndText();
            HideTutorial();
            
            _gameID = _firebaseManager.CreateGame(gameToPlay);

            foreach (PlayerStatus player in _gameState.GetPlayerStatus())
            {
                _firebaseManager.AddPlayerToGame(_gameID, player.Player.Name, player.Player.GetType().ToString() ,player.GetPunctuation());
            }

            await WaitForSpace();

            SceneManager.LoadScene(1);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Juego cancelado");
        }
    }

    private void ShowEndText()
    {
        Player winner = _gameState.GetWinner();
        if (!winner) return;
        
        
        Sprite icon = winner.GetIcon();
        backgroundImage.transform.parent.gameObject.SetActive(true);
        backgroundImage.enabled = true;
        endIcon.enabled = true;
        endIcon.sprite = icon;
        endIcon.sprite = icon;
        endText.alignment = TextAnchor.LowerCenter;
        endTextShadow.alignment = TextAnchor.LowerCenter;
        endTextShadow.transform.SetParent(backgroundImage.transform.parent);
        endText.transform.SetParent(backgroundImage.transform.parent);
        endText.text = "HA GANADO!\nPresiona el espacio para\nun nuevo juego.";
        endTextShadow.text = "HA GANADO!\nPresiona el espacio para\nun nuevo juego.";
    }

    private async void ShowTurnText()
    {
        try
        {
            float alphaReduction = 0.005f;
            
            endText.text = "Tu turno";
            endTextShadow.text = "Tu turno";
            await Task.Delay(500, CancellationTokenSource.Token);
            while (endText.color.a > 0)
            {
                endText.color = new Color(1, 1, 1, endText.color.a - alphaReduction);
                endTextShadow.color = new Color(0, 0, 0, endText.color.a);
                await Task.Yield();
            }
            await Task.Delay(1000, CancellationTokenSource.Token);
            endText.text = "";
            endTextShadow.text = "";
            endText.color = new Color(1, 1, 1, 1);
            endTextShadow.color = new Color(0, 0, 0, 1);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Juego cancelado");
        }
    }

    private async Task<IAction> GetActionFromPlayer(IObservation observation, Player player)
    {
        return await Task.Run(() => IsHumanPlayer ? player.Think(observation, HumanTimeToThink) : player.Think(observation, BotTimeToThink), CancellationTokenSource.Token);
    }

    private float timeRemaining;
    private async void ResetTimer()
    {
        try
        {
            timeCounter.enabled = true;
            float timer = _gameState.GetPlayerTurnIndex() == 0 && IsHumanPlayer ? HumanTimeToThink : BotTimeToThink;
            timeRemaining = timer;
            timeCounter.color = new Color(0, 1, 0, 1);
            timeCounter.fillAmount = 1;
            _stop = false;
            try
            {
                while (timeRemaining >= 0 && !_stop)
                {
                    if (CancellationTokenSource.IsCancellationRequested) return;
                    await Task.Delay(100);
                    timeRemaining -= 0.1f;
                    float relation = timeRemaining / timer;

                    timeCounter.fillAmount = relation;
                    timeCounter.color = new Color(1 - relation, relation, 0, 1);

                    while (_isPaused)
                    {
                        await Task.Yield();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Juego cancelado");
            }

            await Task.Delay(1000);

            timeRemaining = 0;

            while (timeCounter.fillAmount > 0)
            {
                if (timeRemaining > 0) return;
                timeCounter.fillAmount -= 0.04f;
                await Task.Yield();
            }

            await Task.Delay(250);
            
            timeCounter.enabled = false;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Juego cancelado");
        }
    }
    
    async Task WaitForSpace()
    {
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            await Task.Yield();
        }
    }
    public void ShowTutorial()
    {
        tutorialGameObject.SetActive(true);
        _isPaused = true;
        Time.timeScale = 0;
    }
    
    public void HideTutorial()
    {
        tutorialGameObject.SetActive(false);
        _isPaused = false;
        Time.timeScale = 1;
    }
}