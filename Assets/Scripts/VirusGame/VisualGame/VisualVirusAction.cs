using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class VisualVirusAction: MonoBehaviour
{
    private VirusColor _colorSelected = VirusColor.None;
    private static readonly Vector2 OrganPosition1 = new (-480, -60);
    private static readonly Vector2 OrganPosition2 = new (-250, -140);
    private static readonly Vector2 OrganPosition3 = new (0, -180);
    private static readonly Vector2 OrganPosition4 = new (250, -140);
    private static readonly Vector2 OrganPosition5 = new (480, -60);

    private int _playerIndex = -1;
    private static readonly Vector2 PlayerPosition1 = new (-250, -140);
    private static readonly Vector2 PlayerPosition2 = new (0, -180);
    private static readonly Vector2 PlayerPosition3 = new (250, -140);

    public GameObject redOrgan, blueOrgan, rainbowOrgan, yellowOrgan, greenOrgan;
    public GameObject playerIcon1, playerIcon2, playerIcon3;
    
    public GameObject selectPlayerGo, selectOrganGo;

    private readonly Dictionary<VirusColor, GameObject> _organs = new();
    private readonly Dictionary<int, GameObject> _playerIcons = new();

    private void Start()
    {
        _organs.Add(VirusColor.Red, redOrgan);
        _organs.Add(VirusColor.Blue, blueOrgan);
        _organs.Add(VirusColor.Rainbow, rainbowOrgan);
        _organs.Add(VirusColor.Yellow, yellowOrgan);
        _organs.Add(VirusColor.Green, greenOrgan);
        
        _playerIcons.Add(1, playerIcon1);
        _playerIcons.Add(2, playerIcon2);
        _playerIcons.Add(3, playerIcon3);
    }

    public async Task<VirusColor> SelectOrganTarget(VirusType type, VirusPlayerStatus player, List<VirusColor> colorFilter = null, TreatmentType treatment = TreatmentType.None)
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
                    case TreatmentType.OrganThief:
                        if (colorFilter is null || colorFilter.Count == 0) // Elige un órgano propio de tu cuerpo
                        {
                            organsTarget.AddRange(from organ in player.Body where organ.Status != Status.Immune select organ.OrganColor);
                        }
                        else //Elige un órgano de los enemigos usando colorFilter como el parametro para determinar que colores puede elegir
                        {
                            organsTarget.AddRange(from organ in player.Body where colorFilter.Contains(organ.OrganColor) && organ.Status != Status.Immune  select organ.OrganColor);
                            Debug.Log(organsTarget.Count);
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
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        ShowOrganCanvas(true);
        _colorSelected = VirusColor.None;
        PlaceOrganButtons(organsTarget);
        while (_colorSelected == VirusColor.None)
        {
            await Task.Yield();
        }
        ShowOrganCanvas(false);
        return _colorSelected;
    }

    public async Task<int> SelectPlayerTarget(VirusObservation observable, VirusType type, TreatmentType treatment = TreatmentType.None, List<VirusColor> colorFilter = null, VirusColor organSelf = VirusColor.None)
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
                    case TreatmentType.Spreading: // ColorFilter tiene los colores que podemos infectar
                    case TreatmentType.OrganThief: // Elige los player que tengan organos que nos puedan servir
                        foreach (VirusPlayerStatus player in observable.PlayersStatus)
                        {
                            if (player.Index == observable.GetPlayerTurnIndex()) continue; //Ignoramos el jugador humano
                            if (player.Body.Any(organ => colorFilter.Contains(organ.OrganColor) && organ.Status != Status.Immune)) playersTarget.Add(player.Index);
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

    private void PlaceOrganButtons(List<VirusColor> colors)
    {
        switch (colors.Count)
        {
            case 1:
                _organs[colors[0]].GetComponent<RectTransform>().anchoredPosition = OrganPosition3;
                break;
            case 2:
                _organs[colors[0]].GetComponent<RectTransform>().anchoredPosition = OrganPosition2;
                _organs[colors[1]].GetComponent<RectTransform>().anchoredPosition = OrganPosition4;
                break;
            case 3:
                _organs[colors[0]].GetComponent<RectTransform>().anchoredPosition = OrganPosition2;
                _organs[colors[1]].GetComponent<RectTransform>().anchoredPosition = OrganPosition3;
                _organs[colors[2]].GetComponent<RectTransform>().anchoredPosition = OrganPosition4;
                break;
            case 4:
                _organs[colors[0]].GetComponent<RectTransform>().anchoredPosition = OrganPosition1;
                _organs[colors[1]].GetComponent<RectTransform>().anchoredPosition = OrganPosition2;
                _organs[colors[2]].GetComponent<RectTransform>().anchoredPosition = OrganPosition4;
                _organs[colors[3]].GetComponent<RectTransform>().anchoredPosition = OrganPosition5;
                break;
            default:
                _organs[colors[0]].GetComponent<RectTransform>().anchoredPosition = OrganPosition1;
                _organs[colors[1]].GetComponent<RectTransform>().anchoredPosition = OrganPosition2;
                _organs[colors[2]].GetComponent<RectTransform>().anchoredPosition = OrganPosition3;
                _organs[colors[3]].GetComponent<RectTransform>().anchoredPosition = OrganPosition4;
                _organs[colors[4]].GetComponent<RectTransform>().anchoredPosition = OrganPosition5;
                break;
        }

        foreach (VirusColor color in colors)
        {
            _organs[color].SetActive(true);
        }
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

    public void SetColor(int numColor)
    {
        /*  0 == Red,
            1 == Blue,
            2 == Rainbow,
            3 == Yellow,
            4 == Green,
            5 == None   */
        _colorSelected = (VirusColor)numColor;
    }

    public void SetPlayer(int playerIndex)
    {
        _playerIndex = playerIndex;
    }

    private void ShowOrganCanvas(bool show)
    {
        foreach (GameObject objects in _organs.Values)
        {
            objects.SetActive(false);
        }
        selectOrganGo.SetActive(show);
    }
    
    private void ShowPlayerCanvas(bool show)
    {
        foreach (GameObject objects in _playerIcons.Values)
        {
            objects.SetActive(false);
        }
        selectPlayerGo.SetActive(show);
    }
}