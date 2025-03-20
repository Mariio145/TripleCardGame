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
    public int PlayerIndexPerspective {get; }

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
        Card card = _mixedDrawDeck.DrawCard();
        if (card is null) Debug.LogError("Draw card failed observation");

        if (_mixedDrawDeck.RemainingCards() > 0) return card;
        
        _mixedDrawDeck = new Deck<VirusCard>(DiscardDeck);
        DiscardDeck = new Deck<VirusCard>();
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
                            selfNeededColors = new List<VirusColor>();
                            
                            //Se busca todos los colores que el jugador actual no tenga
                            foreach (VirusColor colors in Enum.GetValues(typeof(VirusColor)))
                            {
                                bool coloredOrgan = currentPlayer.Body.Any(playerOrgan => playerOrgan.OrganColor == colors);
                                if (!coloredOrgan) selfNeededColors.Add(colors);
                            }

                            //Para cada jugador rival del jugador que actua en este turno, se busca todos los colores que el rival no tenga
                            
                            for (int i = 0; i < PlayersStatus.Count; i++)
                            {
                                if (i == _currentPlayerTurn) continue;
                                List<VirusColor> otherNeededColors = new();
                                
                                foreach (VirusColor colors in Enum.GetValues(typeof(VirusColor)))
                                {
                                    bool coloredOrgan = PlayersStatus[i].Body.Any(playerOrgan => playerOrgan.OrganColor == colors);
                                    if (!coloredOrgan) otherNeededColors.Add(colors);
                                }
                                
                                //Se añade una accion para cada combinacion entre los organos que el jugador actual no tiene y el rival que se esta analizando ahora
                                
                                foreach (VirusColor organColorOther in otherNeededColors)
                                {
                                    bool currentPlayerNeedsOrgan = currentPlayer.Body.Any(playerOrgan => playerOrgan.OrganColor == organColorOther);
                                    if (currentPlayerNeedsOrgan)
                                    {
                                        foreach (VirusColor organColorCurrent in selfNeededColors)
                                        {
                                            bool otherPlayerNeedsOrgan = PlayersStatus[i].Body.Any(playerOrgan => playerOrgan.OrganColor == organColorOther);
                                            if (otherPlayerNeedsOrgan) actions.Add(new VirusActionTransplant(organColorOther, organColorCurrent, _currentPlayerTurn, i, handIndex));
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
                            
                            //Código compactado del que no me fio
                            selfNeededColors = (from VirusColor colors in Enum.GetValues(typeof(VirusColor)) let coloredOrgan = currentPlayer.Body.Any(playerOrgan => playerOrgan.OrganColor == colors) where !coloredOrgan select colors).ToList();
                            
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
                            List<VirusColor> selfInfectedColors = new();
                            
                            foreach (VirusOrgan selfOrgan in PlayersStatus[_currentPlayerTurn].Body)
                            {
                                if (selfOrgan.Status == Status.Infected) selfInfectedColors.Add(selfOrgan.OrganColor);
                            }

                            if (selfInfectedColors.Count == 0) continue;
                    
                            for (int i = 0; i < PlayersStatus.Count; i++)
                            {
                                if (i == _currentPlayerTurn) continue;
                                foreach (VirusOrgan otherOrgan in PlayersStatus[i].Body)
                                {
                                    if (otherOrgan.Status == Status.Normal)
                                    {
                                        if (otherOrgan.OrganColor == VirusColor.Rainbow)
                                            foreach (VirusColor organColor in selfInfectedColors)
                                                actions.Add(new VirusActionSpread(organColor, otherOrgan.OrganColor, _currentPlayerTurn, i, handIndex));
            
                                        if (selfInfectedColors.Contains(otherOrgan.OrganColor))
                                            actions.Add(new VirusActionSpread(otherOrgan.OrganColor, otherOrgan.OrganColor, _currentPlayerTurn, i, handIndex));

                                        if (!selfInfectedColors.Contains(VirusColor.Rainbow)) continue;
                                        VirusColor selfVirusColor = PlayersStatus[PlayerIndexPerspective].SearchOrganColor(VirusColor.Rainbow).VirusColor;
                                        
                                        if (otherOrgan.OrganColor == selfVirusColor)
                                            actions.Add(new VirusActionSpread(VirusColor.Rainbow, otherOrgan.OrganColor, _currentPlayerTurn, i, handIndex));
                                            
                                        if (selfVirusColor == VirusColor.Rainbow)
                                            actions.Add(new VirusActionSpread(VirusColor.Rainbow, otherOrgan.OrganColor, _currentPlayerTurn, i, handIndex));
                                    }
                                }
                            }

                            if (selfInfectedColors.Contains(VirusColor.Rainbow))
                                for (int i = 0; i < PlayersStatus.Count; i++)
                                {
                                    if (i == _currentPlayerTurn) continue;
                                    actions.AddRange(from otherOrgan in PlayersStatus[i].Body
                                        where otherOrgan.Status == Status.Normal
                                        select new VirusActionSpread(VirusColor.Rainbow, otherOrgan.OrganColor,
                                            _currentPlayerTurn, i, handIndex));
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
        actions.Add(new VirusActionDiscard(cardIndex.ToList(), _currentPlayerTurn));
        //actions.AddRange(Auxiliar<int>.GetCombinations(cardIndex).Select(combination => new VirusActionDiscard(combination, _currentPlayerTurn)));

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
                        selfNeededColors = new List<VirusColor>(allColors);
                        List<VirusColor> selfHasColors = new();
                        
                        //Se busca todos los colores que el jugador actual no tenga
                        foreach (VirusOrgan bodyOrgan in currentPlayer.Body)
                        {
                            selfHasColors.Add(bodyOrgan.OrganColor);
                            selfNeededColors.Remove(bodyOrgan.OrganColor);
                        }

                        for (int i = 0; i < PlayersStatus.Count; i++)
                        {
                            if (i == _currentPlayerTurn) continue;
                            List<VirusColor> otherNeededColors = new (allColors);
                            List<VirusColor> otherHasColors = new ();

                            foreach (VirusOrgan bodyOrgan in PlayersStatus[i].Body)
                            {
                                if (selfHasColors.Contains(bodyOrgan.OrganColor)) return true;
                                otherNeededColors.Remove(bodyOrgan.OrganColor);
                                otherHasColors.Add(bodyOrgan.OrganColor);
                            }

                            foreach (VirusColor bodyOrgan in otherHasColors)
                            {
                                if (!selfNeededColors.Contains(bodyOrgan)) continue;
                                if (selfHasColors.Any(otherBodyOrgan => otherNeededColors.Contains(otherBodyOrgan)))
                                {
                                    return true;
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
                        List<VirusColor> selfInfectedColors = new();
                        
                        foreach (VirusOrgan selfOrgan in PlayersStatus[_currentPlayerTurn].Body)
                        {
                            if (selfOrgan.Status == Status.Infected) selfInfectedColors.Add(selfOrgan.OrganColor);
                        }
                
                        for (int i = 0; i < PlayersStatus.Count; i++)
                        {
                            if (i == _currentPlayerTurn) continue;
                            foreach (VirusOrgan otherOrgan in PlayersStatus[i].Body.Where(otherOrgan => otherOrgan.Status == Status.Normal))
                            {
                                if (otherOrgan.OrganColor == VirusColor.Rainbow)
                                    if (selfInfectedColors.Any())
                                    {
                                        return true;
                                    }
        
                                if (selfInfectedColors.Contains(otherOrgan.OrganColor))
                                    return true;
                                if (selfInfectedColors.Contains(VirusColor.Rainbow))
                                    return true;
                            }
                        }

                        if (selfInfectedColors.Contains(VirusColor.Rainbow))
                            for (int i = 0; i < PlayersStatus.Count; i++)
                            {
                                if (i == PlayerIndexPerspective) continue;

                                if (PlayersStatus[i].Body.Any(otherOrgan => otherOrgan.Status == Status.Normal))
                                    return true;
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