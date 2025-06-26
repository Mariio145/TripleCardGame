using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class UnoAgentML : Agent
{
    private Player _player;
    private UnoObservation _obsUno;
    private IAction _accionElegida;
    private List<int> _accionesLegales;

    public void Reset()
    {
        _obsUno = null;
        _accionElegida = null;
        _accionesLegales = new List<int>();
    }

    public void PrepararObservacion(IObservation observacion)
    {

        _obsUno = observacion as UnoObservation;

        // Asume que puedes generar aquí la lista de acciones legales
        _accionesLegales = GenerarAccionesLegales(observacion);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_obsUno == null) return;

        UnoPlayerStatus playerStatus = _obsUno.PlayersStatus[_obsUno.PlayerIndexPerspective];
        List<UnoCard> hand = playerStatus.Hand.Cast<UnoCard>().ToList();


        int[] colorCounts = new int[5];
        foreach (var card in hand)
            colorCounts[(int)card.Color]++;

        foreach (int count in colorCounts)
            sensor.AddObservation(count / 25f); //1. Cantidad de cartas por tipo

        int[] numberCounts = new int[10];
        foreach (UnoCard card in hand)
            if (card.Type == UnoType.Number)
                numberCounts[card.Number]++;

        foreach (int count in numberCounts)
            sensor.AddObservation(count / 10f); //2. Cantidad de cartas por número


        int[] typeCounts = new int[5];
        foreach (var card in hand)
        {
            switch (card.Type)
            {
                case UnoType.Block: typeCounts[0]++; break;
                case UnoType.Reverse: typeCounts[1]++; break;
                case UnoType.Draw2: typeCounts[2]++; break;
                case UnoType.Draw4: typeCounts[3]++; break;
                case UnoType.Change: typeCounts[4]++; break;
            }
        }

        foreach (int count in typeCounts)
            sensor.AddObservation(count / 5f); //3. Cantidad de cartas por tipo

        //4. Total de cartas en mano
        sensor.AddObservation(Mathf.Clamp01(hand.Count / 50f));

        //5. Cartas jugables
        int jugables = hand.Count(card => _obsUno.IsCardPlayable(card));
        sensor.AddObservation(Mathf.Clamp01(jugables / 20f));

        //6. Carta en el centro
        UnoCard top = _obsUno.TopCard;
        sensor.AddObservation((float)top.Color / 4f);
        sensor.AddObservation((float)top.Type / 5f);
        sensor.AddObservation((top.Number + 5f) / 14f);

        // 7. Tamaño de la mano de los oponentes
        foreach ((UnoPlayerStatus status, int i) in _obsUno.PlayersStatus.Select((s, i) => (s, i)))
        {
            if (i == _obsUno.PlayerIndexPerspective) continue;
            sensor.AddObservation(Mathf.Clamp01(status.Hand.Count / 50f));
        }

        //8. Reglas activas
        sensor.AddObservation(_obsUno.IsReversed ? 1f : 0f);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        for (int i = 0; i <= 58; i++)
        {
            if (!_accionesLegales.Contains(i))
            {
                actionMask.SetActionEnabled(0, i, false);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int indice = actions.DiscreteActions[0];
        _accionElegida = TraducirAccion(indice);
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
        switch (accion)
        {
            case UnoPlayCard play:
                switch (play.Card.Type)
                {
                    case UnoType.Number:
                        return play.Card.Number + ((int)play.Card.Color * 10); // 0 - 39
                    case UnoType.Reverse:
                        return 40 + (int)play.Card.Color; // 40 - 43
                    case UnoType.Draw2:
                        return 44 + (int)play.Card.Color;
                    case UnoType.Block:
                        return 48 + (int)play.Card.Color;
                    case UnoType.Change:
                        return 52;
                    case UnoType.Draw4:
                        return 53;
                }

                break;
            case UnoDrawCard:
                return 54;
            case UnoChangeColor color:
                return 55 + (int)color.ColorChange;
            default:
                return -1;
        }

        return -1;
    }

    private IAction TraducirAccion(int code)
    {
        Debug.Log("Traduciendo: " + code);
        switch (code)
        {
            case >= 0 and <= 53:
            {
                Debug.Log("Ha entrado en <54: " + code);
                UnoType type = UnoType.Change;
                UnoColor color = UnoColor.Red;
                int number = -1;
                switch (code)
                {
                    case >= 0 and <= 39:
                        type = UnoType.Number;
                        number = code % 10;
                        color = (UnoColor)(code / 10);
                        break;
                    case >= 40 and <= 43:
                        type = UnoType.Reverse;
                        color = (UnoColor)(code - 40);
                        break;
                    case >= 44 and <= 47:
                        type = UnoType.Draw2;
                        color = (UnoColor)(code - 44);
                        break;
                    case >= 48 and <= 51:
                        type = UnoType.Block;
                        color = (UnoColor)(code - 48);
                        break;
                    case 52:
                        type = UnoType.Change;
                        color = UnoColor.Wild;
                        break;
                    case 53:
                        type = UnoType.Draw4;
                        color = UnoColor.Wild;
                        break;
                }

                int handIndex = -1;
                
                foreach (UnoCard card in _obsUno.PlayersStatus[_obsUno.PlayerIndexPerspective].Hand.Cast<UnoCard>())
                {
                    handIndex++;
                    Debug.Log(handIndex);
                    Debug.Log("Tipo: " + card.Type + " | Color: " + card.Color + " | Numero: " + card.Number);
                    switch (card.Type)
                    {
                        case UnoType.Number:
                            if (type != UnoType.Number) continue;
                            if (card.Number != number) continue;
                            if (card.Color == color)
                                return new UnoPlayCard(card, handIndex);
                            break;
                        case UnoType.Reverse:
                            if (type != UnoType.Reverse) continue;
                            if (card.Color == color)
                                return new UnoPlayCard(card, handIndex);
                            break;
                        case UnoType.Draw2:
                            if (type != UnoType.Draw2) continue;
                            if (card.Color == color)
                                return new UnoPlayCard(card, handIndex);
                            break;
                        case UnoType.Block:
                            if (type != UnoType.Block) continue;
                            if (card.Color == color)
                                return new UnoPlayCard(card, handIndex);
                            break;
                        case UnoType.Change:
                            if (type == UnoType.Change) return new UnoPlayCard(card, handIndex);
                            break;
                        case UnoType.Draw4:
                            if (type == UnoType.Draw4) return new UnoPlayCard(card, handIndex);
                            break;
                    }
                }
                Debug.LogError("NO SE HA ENCONTRADO LA CARTA ESPECIFICA");

                break;
            }
            case 54:
                return new UnoDrawCard();
            case >= 55 and <= 58:
                return new UnoChangeColor((UnoColor)(code - 55));
            default:
                return null;
        }

        return null;
    }
}
