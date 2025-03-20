using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class VirusHumanPlayer : Player
{
    private bool _playCard, _discardCard;
    public VisualVirusAction virusVisualAction;
    private VirusColor _colorSelected;
    private int _playerTarget;
    
    private static SynchronizationContext _mainThreadContext;

    void Awake()
    {
        _mainThreadContext = SynchronizationContext.Current;
    }
    public override IAction Think(IObservation observable, float thinkingTime)
    {
        VisualVirusCard[] listCards = null;

        // Ejecutar en el hilo principal
        _mainThreadContext.Send(_ =>
        {
            listCards = handSlot.GetComponentsInChildren<VisualVirusCard>();
            foreach (VisualVirusCard card in listCards)
            {
                card.selected = false;
                card.SetOutline(false);
            }
        }, null);
        
        while (true)
        {
            _playCard = false;
            _discardCard = false;
            
            if (observable.GetActions().All(action => action.GetType() == typeof(VirusAction))) return new VirusAction();

            while (!_playCard && !_discardCard && thinkingTime > 0)
            { }
            
            if (!_playCard && !_discardCard) return new VirusAction();

            int cardSlot = 0;
            if (_discardCard)
            {
                List<int> combination = new();
                foreach (VisualVirusCard card in listCards)
                {
                    if (card.selected)
                    {
                        card.DOKill();
                        combination.Add(cardSlot);
                        _mainThreadContext.Send(_ =>
                        {
                            card.SetOutline(false);
                        }, null);
                        
                    }
                    cardSlot++;
                }

                return new VirusActionDiscard(combination, observable.GetPlayerTurnIndex());
            }

            
            //Seleccionar la accion dependiendo de la carta seleccionada
            VirusCard selectedCard = null;
            foreach (VisualVirusCard card in listCards)
            {
                if (card.selected)
                {
                    if (selectedCard is not null)
                    {
                        selectedCard = null;
                        _mainThreadContext.Send(_ =>
                        {
                            GameManager.EndText.text = "Selecciona solo una carta para jugarla";
                        }, null);
                        
                        float timer = 0;
                        while (timer < 3f) 
                        {
                            _mainThreadContext.Send(_ =>
                            {
                                timer += Time.deltaTime;
                            }, null);
                        }
                        
                        _mainThreadContext.Send(_ =>
                        {
                            GameManager.EndText.text = "";
                        }, null);
                        
                        break;
                    }
                    selectedCard = (VirusCard)card.MemoryCard;
                    card.DOKill();
                }
                cardSlot++;
            }
            
            Debug.Log("Hola");

            if (selectedCard is null) continue;
            
            selectedCard.VisualCard.DOKill();

            if (!observable.IsCardPlayable(selectedCard))
            {
                _mainThreadContext.Send(_ =>
                {
                    GameManager.EndText.text = "Esta carta no puede ser jugada";
                }, null);
                        
                float timer = 0;
                while (timer < 3f)
                {
                    _mainThreadContext.Send(_ =>
                    {
                        timer += Time.deltaTime;
                    }, null);
                }
                        
                _mainThreadContext.Send(_ =>
                {
                    GameManager.EndText.text = "";
                }, null);

                continue;
            }

            _mainThreadContext.Send(_ => { ((VisualVirusCard)selectedCard!.VisualCard).SetOutline(false); }, null);

            return GetCardAction((VirusObservation)observable, selectedCard, cardSlot).Result;
            
        }
    }

    private async Task<IAction> GetCardAction(VirusObservation observable, VirusCard card, int handIndex)
    {
        if (card is null) Debug.LogError("Draw card failed observation");
        List<VirusColor> allColors = (Enum.GetValues(typeof(VirusColor)) as VirusColor[])!.ToList();
        List<VirusColor> colorFilter = new();
        VirusType type = card!.GetType();
        VirusColor color = card.GetColor();
        int currentPlayerIndex = observable.GetPlayerTurnIndex();
        VirusPlayerStatus currentPlayer = observable.PlayersStatus[currentPlayerIndex];
        _colorSelected = VirusColor.None;
        _playerTarget = 0;

        switch (type)
        {
            //----------------------------ÓRGANOS--------------------------
            case VirusType.Organ:
                return new VirusActionPlayOrgan(color, currentPlayerIndex, handIndex);
            //----------------------------MEDICAMENTOS---------------------
            case VirusType.Medicine:
                if (color == VirusColor.Rainbow)
                {
                    colorFilter = new List<VirusColor>(allColors);
                    colorFilter.Remove(VirusColor.None);
                }
                else
                {
                    colorFilter.Add(VirusColor.Rainbow);
                    colorFilter.Add(color);
                }
                virusVisualAction.SelectOrganTarget(type, currentPlayer, colorFilter);
                while (_colorSelected == VirusColor.None)
                {
                    await Task.Yield();
                }
                
                foreach (VirusPlayerStatus player in observable.PlayersStatus)
                {
                    player.VisualBody.ObscureOrgans();
                }

                virusVisualAction.EnableGlobalLight(true);
                
                return new VirusActionPlayMedicine(_colorSelected, currentPlayerIndex, color, handIndex);
            //----------------------------VIRUS----------------------------
            case VirusType.Virus:
                if (color == VirusColor.Rainbow)
                {
                    colorFilter = allColors;
                    colorFilter.Remove(VirusColor.None);
                }
                else
                {
                    colorFilter.Add(VirusColor.Rainbow);
                    colorFilter.Add(color);
                }

                foreach (int playerIndex in virusVisualAction.GetPlayersTarget(observable, type, TreatmentType.None , colorFilter))
                    virusVisualAction.SelectOrganTarget(type, observable.PlayersStatus[playerIndex], colorFilter);
                
                while (_colorSelected == VirusColor.None)
                {
                    await Task.Yield();
                }
                
                foreach (VirusPlayerStatus player in observable.PlayersStatus)
                {
                    player.VisualBody.ObscureOrgans();
                }
                
                virusVisualAction.EnableGlobalLight(true);
                
                return new VirusActionPlayVirus(_colorSelected, _playerTarget, color, handIndex);
            //-----------------------------------------------------------------------------
            //----------------------------CARTAS DE TRATAMIENTO----------------------------
            //-----------------------------------------------------------------------------
            case VirusType.Treatment:
                TreatmentType treatment = card.GetTreatmentType();
                VirusColor organSelf;
                switch (treatment)
                {
                    //----------------------------TRANSPLANTE-------------------------
                    case TreatmentType.Transplant:
                        colorFilter = new List<VirusColor>(allColors);
                        colorFilter.Remove(VirusColor.None);
                        foreach (VirusOrgan organ in currentPlayer.Body)
                        {
                            colorFilter.Remove(organ.OrganColor);
                        }
                        virusVisualAction.SelectOrganTarget(type, currentPlayer, null, treatment);
                        while (_colorSelected == VirusColor.None)
                        {
                            await Task.Yield();
                        }

                        foreach (VirusPlayerStatus player in observable.PlayersStatus)
                        {
                            player.VisualBody.ObscureOrgans();
                        }
                        
                        colorFilter.Add(_colorSelected);

                        organSelf = _colorSelected;
                        
                        foreach (int playerIndex in virusVisualAction.GetPlayersTarget(observable, type, treatment, colorFilter, organSelf))
                            virusVisualAction.SelectOrganTarget(type, observable.PlayersStatus[playerIndex], colorFilter, treatment);

                        _colorSelected = VirusColor.None;
                        
                        while (_colorSelected == VirusColor.None)
                        {
                            await Task.Yield();
                        }
                        
                        foreach (VirusPlayerStatus player in observable.PlayersStatus)
                        {
                            player.VisualBody.ObscureOrgans();
                        }
                        
                        virusVisualAction.EnableGlobalLight(true);
                        
                        return new VirusActionTransplant(organSelf, _colorSelected, currentPlayerIndex, _playerTarget, handIndex);
                    
                    //----------------------------LADRÓN DE ÓRGANOS-------------------
                    case TreatmentType.OrganThief:
                        colorFilter = new List<VirusColor>(allColors);
                        colorFilter.Remove(VirusColor.None);
                        foreach (VirusOrgan organ in currentPlayer.Body)
                        {
                            colorFilter.Remove(organ.OrganColor);
                        }
                        
                        foreach (int playerIndex in virusVisualAction.GetPlayersTarget(observable, type, treatment, colorFilter))
                            virusVisualAction.SelectOrganTarget(type, observable.PlayersStatus[playerIndex], colorFilter, treatment);
                        
                        while (_colorSelected == VirusColor.None)
                        {
                            await Task.Yield();
                        }
                        
                        foreach (VirusPlayerStatus player in observable.PlayersStatus)
                        {
                            player.VisualBody.ObscureOrgans();
                        }
                        
                        virusVisualAction.EnableGlobalLight(true);
                        
                        return new VirusActionOrganThief(_colorSelected, currentPlayerIndex, _playerTarget, handIndex);
                    
                    //----------------------------CONTAGIO----------------------------
                    // Simplificado, solo une un virus por carta
                    case TreatmentType.Spreading:
                        colorFilter = new List<VirusColor>();
                        virusVisualAction.SelectOrganTarget(type, currentPlayer, null, treatment);
                        while (_colorSelected == VirusColor.None)
                        {
                            await Task.Yield();
                        }
                        
                        foreach (VirusPlayerStatus player in observable.PlayersStatus)
                        {
                            player.VisualBody.ObscureOrgans();
                        }

                        organSelf = _colorSelected;
                        VirusColor virusColor = currentPlayer.Body.Find(organ => organ.OrganColor == organSelf).VirusColor;
                        if (virusColor == VirusColor.Rainbow)
                        {
                            colorFilter = new List<VirusColor>(allColors);
                            colorFilter.Remove(VirusColor.None);
                        }
                        else
                        {
                            colorFilter.Add(VirusColor.Rainbow);
                            colorFilter.Add(virusColor);
                        }

                        _colorSelected = VirusColor.None;
                        
                        foreach (int playerIndex in virusVisualAction.GetPlayersTarget(observable, type, treatment, colorFilter))
                            virusVisualAction.SelectOrganTarget(type, observable.PlayersStatus[playerIndex], null, treatment);
                        
                        while (_colorSelected == VirusColor.None)
                        {
                            await Task.Yield();
                        }
                        
                        foreach (VirusPlayerStatus player in observable.PlayersStatus)
                        {
                            player.VisualBody.ObscureOrgans();
                        }
                        
                        virusVisualAction.EnableGlobalLight(true);
                        
                        return new VirusActionSpread(organSelf, _colorSelected, currentPlayerIndex, _playerTarget, handIndex);
                    
                    //----------------------------GUANTE DE LÁTEX---------------------
                    case TreatmentType.LatexGlove:
                        return new VirusActionLatexGlove(handIndex);
                    //----------------------------ERROR MÉDICO------------------------
                    case TreatmentType.MedicalError:
                        foreach (int playerIndex in virusVisualAction.GetPlayersTarget(observable, type, treatment))
                            virusVisualAction.SelectOrganTarget(type, observable.PlayersStatus[playerIndex], colorFilter);
                        
                        while (_colorSelected == VirusColor.None)
                        {
                            await Task.Yield();
                        }
                        
                        foreach (VirusPlayerStatus player in observable.PlayersStatus)
                        {
                            player.VisualBody.ObscureOrgans();
                        }
                        
                        virusVisualAction.EnableGlobalLight(true);
                        return new VirusActionMedicalError(currentPlayerIndex, _playerTarget, handIndex);
                }
                break;
        }

        return null;
    }

    public void PlayCard()
    {
        _playCard = true;
    }

    public void DiscardCard()
    {
        foreach (VisualVirusCard card in handSlot.GetComponentsInChildren<VisualVirusCard>())
        {
            if (!card.selected) continue;
            _discardCard = true;
            break;
        }
    }
    
    /*
     *
     * HUMAN ACTIONS
     * 
     */
    
    public void SetColorAndPlayer(int numColor, int playerIndex)
    {
        /*  0 == Red,
            1 == Blue,
            2 == Rainbow,
            3 == Yellow,
            4 == Green,
            5 == None   */
        _colorSelected = (VirusColor)numColor;
        _playerTarget = playerIndex;
    }
}
