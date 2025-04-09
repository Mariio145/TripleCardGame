using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VirusObservation: IObservation
{
    //La baraja no tiene porque ser igual que la original
    private Deck<VirusCard> _mixedDrawDeck;
    public Deck<VirusCard> DiscardDeck;
    
    // Contiene los datos del player, tanto de su mano como de su cuerpo
    public readonly List<VirusPlayerStatus> PlayersStatus;
    private int _currentPlayerTurn;
    public int PlayerIndexPerspective {get;}

    public VirusObservation(Deck<VirusCard> mixedDrawDeck, Deck<VirusCard> discardDeck, List<VirusPlayerStatus> playersStatus, int currentPlayerTurn, int playerPerspective)
    {
        _mixedDrawDeck = mixedDrawDeck;
        DiscardDeck = discardDeck;
        PlayersStatus = playersStatus;
        _currentPlayerTurn = currentPlayerTurn;
        PlayerIndexPerspective = playerPerspective;
    }

    public IObservation Clone()
    {
        List<VirusPlayerStatus> playerStatus = PlayersStatus.Select(player => player.Clone()).ToList();
        Deck<VirusCard> mixedDrawDeck = new(_mixedDrawDeck);
        Deck<VirusCard> discardDeck = new(DiscardDeck);
        return new VirusObservation(mixedDrawDeck, discardDeck, playerStatus, _currentPlayerTurn, PlayerIndexPerspective);
    }

    public int GetPlayerTurnIndex()
    {
        return _currentPlayerTurn;
    }

    public bool IsTerminal()
    {
        return PlayersStatus.Any(playerInfo => playerInfo.HasWon());
    }

    public Card DrawCardFromMixedDrawDeck()
    {
        if (_mixedDrawDeck.RemainingCards() <= 0)
        {
            _mixedDrawDeck = new Deck<VirusCard>(DiscardDeck);
            _mixedDrawDeck.ShuffleDeck();

            DiscardDeck = new Deck<VirusCard>();
        }

        VirusCard card = _mixedDrawDeck.DrawCard();

        return card;
    }

    public void DiscardCard(Card card)
    {
        DiscardDeck.Add((VirusCard)card);
    }
    
    public void DiscardCard(VirusCard card)
    {
        DiscardDeck.Add(card);
    }

    public void ChangeTurnIndex()
    {
        _currentPlayerTurn = (_currentPlayerTurn + 1) % PlayersStatus.Count;
    }

    public IObservation GetCloneRandomized()
    {
        Deck<VirusCard> drawDeck = new (_mixedDrawDeck);
        foreach (VirusPlayerStatus player in PlayersStatus)
        {
            if (player == PlayersStatus[PlayerIndexPerspective]) continue;

            foreach (VirusCard card in player.Hand.Cast<VirusCard>())
            {
                drawDeck.Add(card);
            }
        }
        
        drawDeck.ShuffleDeck();
        
        List<VirusPlayerStatus> playersStatus = new();
        
        foreach (VirusPlayerStatus player in PlayersStatus)
        {
            VirusPlayerStatus playerCopy = player.Clone();
            playersStatus.Add(playerCopy);
            if (player == PlayersStatus[PlayerIndexPerspective]) continue;
            if (playerCopy.Hand.All(card => card is null)) continue;
            playerCopy.Hand.Clear();
            for (int i = 0; i < VirusGameParameters.NCardsPlayerHand; i++)
            {
                // No hay que preocuparse por que la baraja de robo se vacíe, ya que antes se llena con las cartas de la mano de los otros jugadores
                playerCopy.Hand.Enqueue(drawDeck.DrawCard());
            }
        }
        
        drawDeck.ShuffleDeck();
        Deck<VirusCard> discardDeck = new (DiscardDeck);

        return new VirusObservation(drawDeck, discardDeck, playersStatus, _currentPlayerTurn, PlayerIndexPerspective);
    }

    public List<IAction> GetActions()
    {
        VirusPlayerStatus currentPlayer = PlayersStatus[_currentPlayerTurn];
        Queue<Card> playerHand = currentPlayer.Hand;
        List<IAction> actions = new ();
        int handIndex = 0;
        //Si no tienes cartas en la mano, robas 3 y saltas el turno

        if (playerHand.All(card => card is null))
        {
            actions.Add(new VirusAction());
            return actions;
        }

        // 1. Jugar 1 unica carta de tu mano
        foreach (Card cardFromHand in playerHand)
        {
            VirusCard card = (VirusCard)cardFromHand;
            if (card is null) continue;
            VirusType type = card.GetType();
            VirusColor color = card.GetColor();

            VirusOrgan organ;
            switch (type)
            {
                //----------------------------ÓRGANOS--------------------------
                case VirusType.Organ:
                    if (currentPlayer.SearchOrganColor(color) == null)
                        actions.Add(new VirusActionPlayOrgan(color, _currentPlayerTurn, handIndex));
                    break;
                //----------------------------MEDICAMENTOS---------------------
                case VirusType.Medicine:
                    if (color != VirusColor.Rainbow)
                    {
                        organ = currentPlayer.SearchOrganColor(color);
                        if (organ != null)
                            if (organ.Status != Status.Immune)
                                actions.Add(new VirusActionPlayMedicine(color, _currentPlayerTurn, color, handIndex));

                        organ = currentPlayer.SearchOrganColor(VirusColor.Rainbow);
                        if (organ != null)
                            if (organ.Status != Status.Immune)
                                actions.Add(new VirusActionPlayMedicine(VirusColor.Rainbow, _currentPlayerTurn, color, handIndex));
                    }
                    else
                    {
                        foreach (VirusOrgan playerOrgan in currentPlayer.Body)
                        {
                            if (playerOrgan.Status != Status.Immune)
                                actions.Add(new VirusActionPlayMedicine(playerOrgan.OrganColor, _currentPlayerTurn, color, handIndex));
                        }
                    }

                    break;
                //----------------------------VIRUS----------------------------
                case VirusType.Virus:
                    for (int i = 0; i < PlayersStatus.Count; i++)
                    {
                        if (i == _currentPlayerTurn) continue;
                        if (color != VirusColor.Rainbow)
                        {
                            organ = PlayersStatus[i].SearchOrganColor(color);
                            if (organ != null)
                                if (organ.Status != Status.Immune)
                                    actions.Add(new VirusActionPlayVirus(color, i, color, handIndex));
                            organ = PlayersStatus[i].SearchOrganColor(VirusColor.Rainbow);
                            if (organ != null)
                                if (organ.Status != Status.Immune)
                                    actions.Add(new VirusActionPlayVirus(VirusColor.Rainbow, i, color, handIndex));
                        }
                        else
                        {
                            foreach (VirusOrgan playerOrgan in PlayersStatus[i].Body)
                            {
                                if (playerOrgan.Status != Status.Immune)
                                    actions.Add(new VirusActionPlayVirus(playerOrgan.OrganColor, i, color, handIndex));
                            }
                        }
                    }
                    break;
                //-----------------------------------------------------------------------------
                //----------------------------CARTAS DE TRATAMIENTO----------------------------
                //-----------------------------------------------------------------------------
                case VirusType.Treatment:
                    TreatmentType treatment = card.GetTreatmentType();
                    List<VirusColor> selfNeededColors;
                    switch (treatment)
                    {
                        //----------------------------TRANSPLANTE-------------------------
                        case TreatmentType.Transplant:
                            selfNeededColors = (Enum.GetValues(typeof(VirusColor)) as VirusColor[])!.ToList();
                            selfNeededColors.Remove(VirusColor.None);
                            
                            //Se busca todos los colores que el jugador actual no tenga
                            foreach (VirusOrgan selfOrgan in currentPlayer.Body)
                            {
                                selfNeededColors.Remove(selfOrgan.OrganColor);
                            }

                            //Para cada jugador rival del jugador que actua en este turno, se busca todos los colores que el rival no tenga
                            
                            for (int i = 0; i < PlayersStatus.Count; i++)
                            {
                                if (i == _currentPlayerTurn) continue;
                                VirusPlayerStatus player = PlayersStatus[i];
                                List<VirusColor> otherNeededColors = (Enum.GetValues(typeof(VirusColor)) as VirusColor[])!.ToList();
                                otherNeededColors.Remove(VirusColor.None);
                                
                                foreach (VirusOrgan enemyOrgan in player.Body)
                                {
                                    VirusColor organColor = enemyOrgan.OrganColor;
                                    otherNeededColors.Remove(organColor);
                                    if (enemyOrgan.Status != Status.Immune && !selfNeededColors.Exists(virusColor => virusColor == organColor) && currentPlayer.SearchOrganColor(organColor).Status != Status.Immune) 
                                        actions.Add(new VirusActionTransplant(organColor, organColor, _currentPlayerTurn, i, handIndex));
                                }
                                
                                //Se añade una accion para cada combinacion entre los organos que el jugador actual no tiene y el rival que se esta analizando ahora
                                
                                foreach (VirusColor organColorOther in otherNeededColors)
                                {
                                    if (currentPlayer.Body.Exists(organPlayer => organPlayer.OrganColor == organColorOther && organPlayer.Status != Status.Immune))
                                    {
                                        foreach (VirusColor organColorCurrent in selfNeededColors)
                                        {
                                            if (player.Body.Exists(organPlayer => organPlayer.OrganColor == organColorCurrent && organPlayer.Status != Status.Immune)) actions.Add(new VirusActionTransplant(organColorOther, organColorCurrent, _currentPlayerTurn, i, handIndex));
                                        }
                                    }
                                }
                            }
                            
                            break;
                        //----------------------------LADRÓN DE ÓRGANOS-------------------
                        case TreatmentType.OrganThief:
                            
                            selfNeededColors = (from VirusColor colors in Enum.GetValues(typeof(VirusColor))
                                let coloredOrgan = currentPlayer.Body.Any(playerOrgan => playerOrgan.OrganColor == colors)
                                where !coloredOrgan
                                select colors).ToList();
                            
                            //Se busca todos los colores que el jugador actual no tenga

                            for (int i = 0; i < PlayersStatus.Count; i++)
                            {
                                if (i == _currentPlayerTurn) continue;
                                foreach (VirusOrgan playerOrgan in PlayersStatus[i].Body)
                                {
                                    if(selfNeededColors.Contains(playerOrgan.OrganColor) && playerOrgan.Status != Status.Immune)
                                        actions.Add(new VirusActionOrganThief(playerOrgan.OrganColor, _currentPlayerTurn, i, handIndex));
                                }
                            }

                            break;
                        //----------------------------CONTAGIO----------------------------
                        
                        // Simplificado, solo une un virus por carta
                        case TreatmentType.Spreading:
                            foreach (VirusOrgan selfOrgan in PlayersStatus[_currentPlayerTurn].Body.Where(selfOrgan => selfOrgan.Status == Status.Infected))
                            {
                                for (int i = 0; i < PlayersStatus.Count; i++)
                                {
                                    if (i == _currentPlayerTurn) continue;
                                    foreach (VirusOrgan otherOrgan in PlayersStatus[i].Body.Where(otherOrgan => otherOrgan.Status == Status.Normal))
                                    {
                                        if (otherOrgan.OrganColor == VirusColor.Rainbow || selfOrgan.VirusColor == VirusColor.Rainbow)
                                        {
                                            actions.Add(new VirusActionSpread(selfOrgan.OrganColor, otherOrgan.OrganColor, _currentPlayerTurn, i, handIndex));
                                            continue;
                                        }
              
                                        if (selfOrgan.VirusColor == otherOrgan.OrganColor)
                                            actions.Add(new VirusActionSpread(selfOrgan.OrganColor, otherOrgan.OrganColor, _currentPlayerTurn, i, handIndex));
                                    }
                                }
                            }

                            break;
                        //----------------------------GUANTE DE LÁTEX---------------------
                        case TreatmentType.LatexGlove:
                            actions.Add(new VirusActionLatexGlove(handIndex));
                            break;
                        //----------------------------ERROR MÉDICO------------------------
                        case TreatmentType.MedicalError:
                            for (int i = 0; i < PlayersStatus.Count; i++)
                            {
                                if (i == _currentPlayerTurn) continue;
                                actions.Add(new VirusActionMedicalError(_currentPlayerTurn, i, handIndex));
                            }
                            break;
                    }
                    break;
            }

            handIndex++;
        }
        // 2. Descartar tantas como quieras (minimo 1)
        
        int[] cardIndex = { 0, 1, 2};
        //actions.Add(new VirusActionDiscard(cardIndex.ToList(), _currentPlayerTurn));
        actions.AddRange(Auxiliar<int>.GetCombinations(cardIndex).Select(combination => new VirusActionDiscard(combination, _currentPlayerTurn)));

        if (actions.Count == 0) actions.Add(new VirusAction());
        
        return actions;
    }

    public bool IsCardPlayable(Card card)
    { 
        if (card is null) Debug.LogError("Draw card failed observation");
        VirusCard vCard = card as VirusCard;
        
        VirusType type = vCard!.GetType();
        VirusColor color = vCard.GetColor();
        
        List<VirusColor> allColors = (Enum.GetValues(typeof(VirusColor)) as VirusColor[])!.ToList();
        allColors.Remove(VirusColor.None);
        
        VirusPlayerStatus currentPlayer = PlayersStatus[_currentPlayerTurn];

        VirusOrgan organ;
        switch (type)
        {
            //----------------------------ÓRGANOS--------------------------
            case VirusType.Organ:
                return PlayersStatus[_currentPlayerTurn].SearchOrganColor(color) == null;
            //----------------------------MEDICAMENTOS---------------------
            case VirusType.Medicine:
                if (color != VirusColor.Rainbow)
                {
                    organ = currentPlayer.SearchOrganColor(color);
                    if (organ != null)
                        if (organ.Status != Status.Immune)
                            return true;

                    organ = currentPlayer.SearchOrganColor(VirusColor.Rainbow);
                    if (organ != null)
                        if (organ.Status != Status.Immune)
                            return true;
                }
                else
                {
                    if (currentPlayer.Body.Any(playerOrgan => playerOrgan.Status != Status.Immune))
                        return true;
                }

                break;
            //----------------------------VIRUS----------------------------
            case VirusType.Virus:
                for (int i = 0; i < PlayersStatus.Count; i++)
                {
                    if (i == _currentPlayerTurn) continue;
                    if (color != VirusColor.Rainbow)
                    {
                        organ = PlayersStatus[i].SearchOrganColor(color);
                        if (organ != null)
                            if (organ.Status != Status.Immune)
                                return true;
                        
                        organ = PlayersStatus[i].SearchOrganColor(VirusColor.Rainbow);
                        if (organ != null)
                            if (organ.Status != Status.Immune)
                                return true;
                    }
                    else
                    {
                        if (PlayersStatus[i].Body.Any(playerOrgan => playerOrgan.Status != Status.Immune))
                            return true;
                    }
                }
                break;
            //-----------------------------------------------------------------------------
            //----------------------------CARTAS DE TRATAMIENTO----------------------------
            //-----------------------------------------------------------------------------
            case VirusType.Treatment:
                TreatmentType treatment = vCard.GetTreatmentType();
                List<VirusColor> selfNeededColors;
                switch (treatment)
                {
                    //----------------------------TRANSPLANTE-------------------------
                    case TreatmentType.Transplant:
                        selfNeededColors = (Enum.GetValues(typeof(VirusColor)) as VirusColor[])!.ToList();
                        selfNeededColors.Remove(VirusColor.None);
                        
                        //Se busca todos los colores que el jugador actual no tenga
                        foreach (VirusOrgan selfOrgan in currentPlayer.Body)
                        {
                            selfNeededColors.Remove(selfOrgan.OrganColor);
                        }

                        //Para cada jugador rival del jugador que actua en este turno, se busca todos los colores que el rival no tenga

                        for (int i = 0; i < PlayersStatus.Count; i++)
                        {
                            if (i == _currentPlayerTurn) continue;
                            VirusPlayerStatus player = PlayersStatus[i];
                            List<VirusColor> otherNeededColors =
                                (Enum.GetValues(typeof(VirusColor)) as VirusColor[])!.ToList();
                            otherNeededColors.Remove(VirusColor.None);

                            foreach (VirusOrgan enemyOrgan in player.Body)
                            {
                                VirusColor organColor = enemyOrgan.OrganColor;
                                otherNeededColors.Remove(organColor);
                                if (enemyOrgan.Status != Status.Immune &&
                                    !selfNeededColors.Exists(virusColor => virusColor == organColor) &&
                                    currentPlayer.SearchOrganColor(organColor).Status != Status.Immune)
                                    return true;
                            }

                            //Se añade una accion para cada combinacion entre los organos que el jugador actual no tiene y el rival que se esta analizando ahora

                            foreach (VirusColor organColorOther in otherNeededColors)
                            {
                                if (currentPlayer.Body.Exists(organ => organ.OrganColor == organColorOther))
                                {
                                    foreach (VirusColor organColorCurrent in selfNeededColors)
                                    {
                                        if (player.Body.Exists(organ => organ.OrganColor == organColorCurrent))
                                            return true;
                                    }
                                }
                            }
                        }
                        break;
                    //----------------------------LADRÓN DE ÓRGANOS-------------------
                    case TreatmentType.OrganThief:
                        
                        /*selfNeededColors = new List<CardColor>();
                        
                        //Se busca todos los colores que el jugador actual no tenga
                        foreach (CardColor colors in Enum.GetValues(typeof(CardColor)))
                        {
                            bool coloredOrgan = currentPlayer.Body.Any(playerOrgan => playerOrgan.OrganColor == colors);
                            if (!coloredOrgan) selfNeededColors.Add(colors);
                        }*/
                        
                        //Código compactado
                        selfNeededColors = (from VirusColor colors in Enum.GetValues(typeof(VirusColor)) let coloredOrgan = currentPlayer.Body.Any(playerOrgan => playerOrgan.OrganColor == colors) where !coloredOrgan select colors).ToList();
                        
                        //Se busca todos los colores que el jugador actual no tenga

                        for (int i = 0; i < PlayersStatus.Count; i++)
                        {
                            if (i == _currentPlayerTurn) continue;
                            if (PlayersStatus[i].Body.Any(playerOrgan => selfNeededColors.Contains(playerOrgan.OrganColor) && playerOrgan.Status != Status.Immune))
                            {
                                return true;
                            }
                        }

                        break;
                    //----------------------------CONTAGIO----------------------------
                    
                    // Simplificado, solo une un virus por carta
                    case TreatmentType.Spreading:
                        foreach (VirusOrgan selfOrgan in PlayersStatus[_currentPlayerTurn].Body.Where(selfOrgan => selfOrgan.Status == Status.Infected))
                        {
                            for (int i = 0; i < PlayersStatus.Count; i++)
                            {
                                if (i == _currentPlayerTurn) continue;
                                foreach (VirusOrgan otherOrgan in PlayersStatus[i].Body.Where(otherOrgan => otherOrgan.Status == Status.Normal))
                                {
                                    if (otherOrgan.OrganColor == VirusColor.Rainbow || selfOrgan.VirusColor == VirusColor.Rainbow)
                                        return true;
                                    
                                    if (selfOrgan.VirusColor == otherOrgan.OrganColor)
                                        return true;
                                }
                            }
                        }
                        break;
                    //----------------------------GUANTE DE LÁTEX---------------------
                    case TreatmentType.LatexGlove:
                        return true;
                    //----------------------------ERROR MÉDICO------------------------
                    case TreatmentType.MedicalError:
                        for (int i = 0; i < PlayersStatus.Count; i++)
                        {
                            if (i == _currentPlayerTurn) continue;
                            if (PlayersStatus[i].Body.Count > 0) return true;
                        }
                        break;
                }
                break;
        }
            
        return false;
    }
}