using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using VirusGame.Actions;

public class VirusAgentML : Agent
{
    private Player _player;
    private VirusObservation _obsVirus;
    private IAction _accionElegida;
    private List<int> _accionesLegales;
    private VirusAllActions _allActions;
    
    public void Reset()
    {
        _obsVirus = null;
        _accionElegida = null;
        _accionesLegales = new List<int>();
    }

    public void PrepararObservacion(IObservation observacion)
    {
        
        _obsVirus = observacion as VirusObservation;
        if (_allActions == null) _allActions = new VirusAllActions(_obsVirus.PlayersStatus.Count);
        // Asume que puedes generar aquí la lista de acciones legales
        _accionesLegales = GenerarAccionesLegales(observacion);
    }

public override void CollectObservations(VectorSensor sensor)
{
    if (_obsVirus == null) return;

    VirusPlayerStatus playerStatus = _obsVirus.PlayersStatus[_obsVirus.PlayerIndexPerspective];
    List<VirusCard> hand = playerStatus.Hand.Cast<VirusCard>().ToList();
    
    // 1. Mano del jugador (conteo de tipos de cartas)
    int[] cartaTipoCount = new int[4]; // 0: órgano, 1: virus, 2: medicina, 3: multicolor
    foreach (VirusCard card in hand)
    {
        switch (card.GetType())
        {
            case VirusType.Organ: cartaTipoCount[0]++; break;
            case VirusType.Virus: cartaTipoCount[1]++; break;
            case VirusType.Medicine: cartaTipoCount[2]++; break;
            case VirusType.Treatment: cartaTipoCount[3]++; break;
        }
    }

    // 2. Estado de órganos en su cuerpo (4 órganos posibles)
    foreach (VirusColor color in (VirusColor[])Enum.GetValues(typeof(VirusColor)))
    {
        if (color == VirusColor.None) continue;
        // Cada órgano codificado como:
        // - 1 si existe, 0 si no
        // - 1 si tiene virus, 0 si no
        // - 1 si tiene medicina, 0 si no
        VirusOrgan organ = playerStatus.SearchOrganColor(color);
        if (organ != null)
        {
            sensor.AddObservation(1f);
            switch (organ.Status)
            {
                case Status.Normal:
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                    break;
                case Status.Infected:
                    sensor.AddObservation(1f);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                    break;
                case Status.Vaccinated:
                    sensor.AddObservation(0f);
                    sensor.AddObservation(1f);
                    sensor.AddObservation(0f);
                    break;
                case Status.Immune:
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(1f);
                    break;
            }
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    // 3. Cuerpo de oponentes (solo si hay órganos colocados, no su estado exacto)
    foreach ((VirusPlayerStatus player, int i) in _obsVirus.PlayersStatus.Select((p, i) => (p, i)))
    {
        if (i == _obsVirus.PlayerIndexPerspective) continue;

        foreach (VirusColor color in (VirusColor[])Enum.GetValues(typeof(VirusColor)))
        {
            if (color == VirusColor.None) continue;
            sensor.AddObservation(player.SearchOrganColor(color) != null ? 1f : 0f);
        }
    }
}

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        for (int i = 0; i < _allActions.AllPosibleActions.Count; i++)
        {
            if (!_accionesLegales.Contains(i))
                actionMask.SetActionEnabled(0, i, false);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int accion = actions.DiscreteActions[0];
        _accionElegida = TraducirAccion(accion);
    }

    public IAction ObtenerUltimaAccion()
    {
        return _accionElegida;
    }

    private List<int> GenerarAccionesLegales(IObservation obs)
    {
        // Aquí puedes extraer del forward model todas las acciones posibles
        // y devolver una lista de enteros codificados como IDs de acción
        List<IAction> posibles = obs.GetActions();

        return posibles.Select(accion => CodificarAccion(accion)).ToList();
    }

    private int CodificarAccion(IAction accion)
    {
        VirusAction virusAccion = accion as VirusAction;
        for (int i = 0; i < _allActions.AllPosibleActions.Count; i++)
        {
            VirusAction allAction = _allActions.AllPosibleActions[i];
            if (allAction.GetType() == virusAccion.GetType())
            {
                if (allAction.ColorSelf == virusAccion.ColorSelf && 
                    allAction.ColorTarget == virusAccion.ColorTarget &&
                    allAction.PlayerSelf == virusAccion.PlayerSelf &&
                    allAction.PlayerTarget == virusAccion.PlayerTarget)
                {
                    if (virusAccion.GetType() == typeof(VirusActionPlayMedicine))
                    {
                        if (((VirusActionPlayMedicine)virusAccion).MedicineColor ==
                            ((VirusActionPlayMedicine)allAction).MedicineColor)
                            return i;
                    }
                    else if (virusAccion.GetType() == typeof(VirusActionPlayVirus))
                    {
                        if (((VirusActionPlayVirus)virusAccion).VirusColor ==
                            ((VirusActionPlayVirus)allAction).VirusColor)
                            return i;
                    }
                    else return i;
                }
            }
        }
        return -1;
    }

    private IAction TraducirAccion(int code)
    {
        VirusAction virusAccion = _allActions.AllPosibleActions[code];
        List<IAction> allActions = _obsVirus.GetActions();

        for (int i = 0; i < allActions.Count; i++)
        {
            VirusAction allAction = allActions[i] as VirusAction;
            if (allAction.GetType() == virusAccion.GetType())
            {

                if (allAction.ColorSelf == virusAccion.ColorSelf && 
                    allAction.ColorTarget == virusAccion.ColorTarget &&
                    allAction.PlayerSelf == virusAccion.PlayerSelf &&
                    allAction.PlayerTarget == virusAccion.PlayerTarget)
                {
                    if (virusAccion.GetType() == typeof(VirusActionPlayMedicine))
                    {
                        if (((VirusActionPlayMedicine)virusAccion).MedicineColor ==
                            ((VirusActionPlayMedicine)allAction).MedicineColor)
                            return allAction;
                    }
                    else if (virusAccion.GetType() == typeof(VirusActionPlayVirus))
                    {
                        if (((VirusActionPlayVirus)virusAccion).VirusColor ==
                            ((VirusActionPlayVirus)allAction).VirusColor)
                            return allAction;
                    }
                    else return allAction;
                }
            }
        }
        
        return null;
    }
}
