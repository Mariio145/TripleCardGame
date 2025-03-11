using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class VirusHumanPlayer : Player
{
    private bool _playCard, _discardCard;
    public VisualVirusAction virusVisualAction;


    public override IAction Think(IObservation observable, float thinkingTime)
    {
        VisualVirusCard[] listCards = handSlot.GetComponentsInChildren<VisualVirusCard>();
        foreach (VisualVirusCard card in listCards)
        {
            card.selected = false;
        }
        
        while (true)
        {
            _playCard = false;
            _discardCard = false;
            
            if (observable.GetActions().All(action => action.GetType() == typeof(VirusAction))) return new VirusAction();

            while (!_playCard && !_discardCard && thinkingTime > 0)
            {
                //TODO
                //await Task.Yield();
            }
            
            if (!_playCard && !_discardCard) return new VirusAction();

            int cardSlot = 0;
            if (_discardCard)
            {
                List<int> combination = new();
                foreach (VisualVirusCard card in listCards)
                {
                    if (card.selected) combination.Add(cardSlot);
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
                    selectedCard = (VirusCard)card.MemoryCard;
                    break;
                }
                cardSlot++;
            }

            if (!observable.IsCardPlayable(selectedCard)) continue;

            //TODO
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
        
        VirusColor organTarget;
        int playerTargetIndex;

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
                organTarget = await virusVisualAction.SelectOrganTarget(type, currentPlayer, colorFilter);
                return new VirusActionPlayMedicine(organTarget, currentPlayerIndex, color, handIndex);
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
                
                playerTargetIndex = await virusVisualAction.SelectPlayerTarget(observable, type, TreatmentType.None , colorFilter);
                organTarget = await virusVisualAction.SelectOrganTarget(type, observable.PlayersStatus[playerTargetIndex], colorFilter);
                return new VirusActionPlayVirus(organTarget, playerTargetIndex, color, handIndex);
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
                        organSelf = await virusVisualAction.SelectOrganTarget(type, currentPlayer, null, treatment);
                        colorFilter.Add(organSelf);
                        playerTargetIndex = await virusVisualAction.SelectPlayerTarget(observable, type, treatment, colorFilter, organSelf);
                        organTarget = await virusVisualAction.SelectOrganTarget(type, observable.PlayersStatus[playerTargetIndex], colorFilter, treatment);
                        return new VirusActionTransplant(organSelf, organTarget, currentPlayerIndex, playerTargetIndex, handIndex);
                    
                    //----------------------------LADRÓN DE ÓRGANOS-------------------
                    case TreatmentType.OrganThief:
                        colorFilter = new List<VirusColor>(allColors);
                        colorFilter.Remove(VirusColor.None);
                        foreach (VirusOrgan organ in currentPlayer.Body)
                        {
                            colorFilter.Remove(organ.OrganColor);
                        }
                        
                        playerTargetIndex = await virusVisualAction.SelectPlayerTarget(observable, type, treatment, colorFilter);
                        organTarget = await virusVisualAction.SelectOrganTarget(type, observable.PlayersStatus[playerTargetIndex], colorFilter, treatment);
                        return new VirusActionOrganThief(organTarget, currentPlayerIndex, playerTargetIndex, handIndex);
                    
                    //----------------------------CONTAGIO----------------------------
                    // Simplificado, solo une un virus por carta
                    case TreatmentType.Spreading:
                        colorFilter = new List<VirusColor>();
                        organSelf = await virusVisualAction.SelectOrganTarget(type, currentPlayer, null, treatment);
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
                        
                        playerTargetIndex = await virusVisualAction.SelectPlayerTarget(observable, type, treatment, colorFilter);
                        organTarget = await virusVisualAction.SelectOrganTarget(type, observable.PlayersStatus[playerTargetIndex], null, treatment);
                        
                        return new VirusActionSpread(organSelf, organTarget, currentPlayerIndex, playerTargetIndex, handIndex);
                    
                    //----------------------------GUANTE DE LÁTEX---------------------
                    case TreatmentType.LatexGlove:
                        return new VirusActionLatexGlove(handIndex);
                    //----------------------------ERROR MÉDICO------------------------
                    case TreatmentType.MedicalError:
                        playerTargetIndex = await virusVisualAction.SelectPlayerTarget(observable, type, treatment);
                        return new VirusActionMedicalError(currentPlayerIndex, playerTargetIndex, handIndex);
                }
                break;
        }

        return null;
    }

    public void PlayCard()
    {
        int cardsSelected = handSlot.GetComponentsInChildren<VisualVirusCard>().Count(card => card.selected);

        if (cardsSelected == 1) _playCard = true;
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
}
