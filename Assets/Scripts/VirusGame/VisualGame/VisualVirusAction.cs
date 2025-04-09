using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VisualVirusAction: MonoBehaviour
{
    public Light globalLight;
    private static SynchronizationContext _mainThreadContext;

    void OnEnable()
    {
        _mainThreadContext = SynchronizationContext.Current;
    }
    public void SelectOrganTarget(VirusType type, VirusPlayerStatus player, List<VirusColor> colorFilter = null, TreatmentType treatment = TreatmentType.None, VirusObservation observation = null)
    {
        List<VirusColor> organsTarget = new();
        switch (type)
        {
            case VirusType.Virus:
            case VirusType.Medicine:
                organsTarget.AddRange(from organ in player.Body where organ.Status != Status.Immune && colorFilter!.Contains(organ.OrganColor) select organ.OrganColor);
                break;
            case VirusType.Treatment:
                switch (treatment)
                {
                    case TreatmentType.Transplant:
                        if (colorFilter is null || colorFilter.Count == 0)
                        {
                            List<VirusOrgan> selfOrgans = observation!.PlayersStatus[observation.GetPlayerTurnIndex()].Body;
                            foreach (VirusOrgan selfOrgan in selfOrgans) //Por cada color de organo en nuestro cuerpo
                            {
                                if (selfOrgan is { Status: Status.Immune }) continue;
                                VirusColor organColor = selfOrgan.OrganColor;
                                
                                foreach (VirusPlayerStatus enemyPlayer in observation.PlayersStatus)
                                {
                                    if (enemyPlayer == observation.PlayersStatus[observation.GetPlayerTurnIndex()])
                                        continue;

                                    foreach (VirusOrgan enemyOrgan in enemyPlayer.Body)
                                    {
                                        if (enemyOrgan.Status == Status.Immune) continue;
                                        if (enemyOrgan.OrganColor == organColor)
                                        {
                                            organsTarget.Add(organColor);
                                            continue;
                                        }

                                        if (!selfOrgans.Exists(organ => organ.OrganColor == enemyOrgan.OrganColor) &&
                                            !enemyPlayer.Body.Exists(organ => organ.OrganColor == organColor))
                                        {
                                            organsTarget.Add(organColor);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            List<VirusColor> temp = new(colorFilter);
                            VirusColor organColorToReceive = colorFilter[^1];
                            temp.Remove(organColorToReceive);
                            

                            foreach (VirusOrgan organ in player.Body)
                            {
                                if (organ.Status == Status.Immune) continue;
                                if (organ.OrganColor == organColorToReceive)
                                {
                                    organsTarget.Add(organ.OrganColor);
                                    continue;
                                }

                                if (temp.Contains(organ.OrganColor) &&
                                    !player.Body.Exists(organPlayer => organPlayer.OrganColor == organColorToReceive))
                                {
                                    organsTarget.Add(organ.OrganColor);
                                }
                            }
                        }

                        break;
                    case TreatmentType.OrganThief:
                        if (colorFilter is null || colorFilter.Count == 0) // Elige un órgano propio de tu cuerpo
                        {
                            organsTarget.AddRange(from organ in player.Body where organ.Status != Status.Immune select organ.OrganColor);
                        }
                        else //Elige un órgano de los enemigos usando colorFilter como el parametro para determinar que colores puede elegir
                        {
                            organsTarget.AddRange(from organ in player.Body where colorFilter.Contains(organ.OrganColor) && organ.Status != Status.Immune  select organ.OrganColor);
                        }
                        break;
                    case TreatmentType.Spreading:
                        if (colorFilter is null || colorFilter.Count == 0) // Elige un órgano propio de tu cuerpo
                        {
                            organsTarget.AddRange(from organ in player.Body where organ.Status == Status.Infected select organ.OrganColor);
                        }
                        else // Elige un órgano de los enemigos usando colorFilter como el parametro para determinar que colores puede elegir
                        {
                            organsTarget.AddRange(from organ in player.Body where colorFilter.Contains(organ.OrganColor) && organ.Status == Status.Normal select organ.OrganColor);
                        }
                        break;
                    case TreatmentType.MedicalError:
                        organsTarget.AddRange(from organ in player.Body select organ.OrganColor);
                        break;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        EnableGlobalLight(false);
        // Debug.Log("Player: " + player);
        // Debug.Log("PlayerBody: " + player.VisualBody);
        //
        // Debug.Log("organsTarget: " + organsTarget);
        // Debug.Log("Index: " + player.Index);
        player.VisualBody.IluminateOrgans(organsTarget);
    }

    public void EnableGlobalLight(bool enable)
    {
        _mainThreadContext.Send(_ =>
        {
            globalLight.enabled = enable;
        }, null);
    }

    public List<int> GetPlayersTarget(VirusObservation observable, VirusType type, TreatmentType treatment = TreatmentType.None, List<VirusColor> colorFilter = null, VirusColor organSelf = VirusColor.None)
    {
        List<int> playersTarget = new();
        switch (type)
        {
            case VirusType.Virus:
                foreach (VirusPlayerStatus player in observable.PlayersStatus)
                {
                    if (player == observable.PlayersStatus[observable.GetPlayerTurnIndex()]) continue;
                    if (player.Body.Any(organ => organ.Status != Status.Immune && colorFilter!.Contains(organ.OrganColor))) playersTarget.Add(player.Index);
                }
                break;
            case VirusType.Treatment:
                switch (treatment)
                {
                    case TreatmentType.Transplant: // ColorFilter tiene los colores que nos faltan y el mismo que damos
                        playersTarget.AddRange(from player in observable.PlayersStatus where player.Index != observable.GetPlayerTurnIndex() select player.Index);
                        break;
                    case TreatmentType.Spreading: // ColorFilter tiene los colores que podemos infectar
                    case TreatmentType.OrganThief: // Elige los player que tengan organos que nos puedan servir
                        foreach (VirusPlayerStatus player in observable.PlayersStatus)
                        {
                            if (player.Index == observable.GetPlayerTurnIndex()) continue; //Ignoramos el jugador humano
                            if (player.Body.Any(organ => colorFilter!.Contains(organ.OrganColor) && organ.Status != Status.Immune)) playersTarget.Add(player.Index);
                        }
                        break;
                    case TreatmentType.MedicalError: // Cualquier player nos vale, no hay excepciones
                        foreach (VirusPlayerStatus player in observable.PlayersStatus)
                        {
                            if (player.Index == observable.GetPlayerTurnIndex()) continue; //Ignoramos el jugador humano
                            playersTarget.Add(player.Index);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(treatment), treatment, null);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        
        return playersTarget;
    }

    public async void ExitGame()
    {
        GameManager.CancellationTokenSource.Cancel();
        await Task.Delay(1000);
        SceneManager.LoadScene(1);
    }
}