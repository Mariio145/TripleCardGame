using UnityEngine;

public class MLAgentsPlayer : Player
{
    public UnoAgentML _UnoAgent;
    public VirusAgentML _VirusAgent;

    private GameToPlay game;
    
    public override void SetHeuristic(GameToPlay gametoPlay)
    {
        game = gametoPlay;
    }

    public override IAction Think(IObservation observable, float thinkingTime)
    {
        if (game == GameToPlay.UnoGame)
        {
            _UnoAgent.Reset();

            while (_UnoAgent.ObtenerUltimaAccion() != null)
            {

            }

            _UnoAgent.PrepararObservacion(observable);

            _UnoAgent.RequestDecision();

            while (_UnoAgent.ObtenerUltimaAccion() == null)
            {
            }
            
            return _UnoAgent.ObtenerUltimaAccion();
        }
        else if (game == GameToPlay.VirusGame)
        {
            _VirusAgent.Reset();

            while (_VirusAgent.ObtenerUltimaAccion() != null)
            {
            }

            _VirusAgent.PrepararObservacion(observable);

            _VirusAgent.RequestDecision();

            while (_VirusAgent.ObtenerUltimaAccion() == null)
            {
            }
            
            return _VirusAgent.ObtenerUltimaAccion();
        }

        return null;
    }
}
