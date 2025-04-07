using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VisualUnoAction: MonoBehaviour
{
    public GameObject selectColorGo;
    
    private static SynchronizationContext _mainThreadContext;
    
    void Awake()
    {
        _mainThreadContext = SynchronizationContext.Current;
    }

    public async Task SelectColor(UnoHumanPlayer humanPlayer)
    {
        _mainThreadContext.Send(_ => { selectColorGo.SetActive(true); }, null);
        humanPlayer._color = UnoColor.Wild;
        
        while (humanPlayer._color== UnoColor.Wild)
        {
            await Task.Yield();
        }
        
        _mainThreadContext.Send(_ => { selectColorGo.SetActive(false); }, null);
    }

    public async void ExitGame()
    {
        GameManager.CancellationTokenSource.Cancel();
        await Task.Delay(1000);
        SceneManager.LoadScene(1);
    }
    
}
