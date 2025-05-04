using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SNOMPlayer : Player
{
    private static IObservation actualObservation; // para el juego del UNO,
                                                   // se tiene que entregar hoy y se me ha liado mucho,
                                                   // lo siento si se podía hacer mejor :(

    public override IAction Think(IObservation observation, float thinkingTime)
    {
        List<IAction> possibleActions = observation.GetActions();
        actualObservation = observation;
        IAction bestAction = null;
        float bestValue = float.MinValue;

        foreach (IAction action in possibleActions)
        {
            IObservation cloneObservation = observation.Clone();
            ForwardModel.TestAction(cloneObservation, action);
            
            float value = Heuristic.Evaluate(cloneObservation);
            
            if (value > bestValue)
            {
                bestValue = value;
                bestAction = action;
            }
        }

        return bestAction ?? possibleActions[0];
    }

    public override void SetHeuristic(GameToPlay gameToPlay)
    {
        Heuristic = gameToPlay switch
        {
            GameToPlay.VirusGame => new MyVirusHeuristic(),
            GameToPlay.UnoGame => new MyUnoHeuristic(),
        };
    }

    public class MyUnoHeuristic : IHeuristic
    {
        public float Evaluate(IObservation observation)
        {
            float score = 0;
            UnoObservation unoActualObs = (UnoObservation)SNOMPlayer.actualObservation;
            UnoObservation unoSimulatedObs = (UnoObservation)observation;

            score += EvaluateRivalHandSize(unoActualObs, unoSimulatedObs);
            score += EvaluateChangeColor(unoActualObs, unoSimulatedObs);
            score += EvaluatePositionalAdvantage(unoSimulatedObs);
            
            return score;
        }

        private float EvaluateRivalHandSize(UnoObservation actualObs, UnoObservation simulatedObs)
        {
            int rivalIndex = GetNextRivalIndex(actualObs);
            int RivalHandSizeActual = actualObs.PlayersStatus[rivalIndex].Hand.Count;
            int RivalHandSizeSimulated = simulatedObs.PlayersStatus[rivalIndex].Hand.Count;
            
            if (RivalHandSizeActual < 4) // cuando el rival tenga menos de 4 cartas, se quiere ser más agresivo (^-^)
            {
                if (RivalHandSizeSimulated < RivalHandSizeActual) return 20f;
                else return 3f;
            } else { // sino se busca ser más pacífico (u-u)
                if (RivalHandSizeSimulated < RivalHandSizeActual) return 3f;
                else return 10f;
            }
        }

        private int GetNextRivalIndex(UnoObservation unoObs)
        {
            // creo que calcula bien el índice, sabiendo no producir errores (^^')
            int myIndex = unoObs.GetPlayerTurnIndex();
            int numPlayers = unoObs.PlayersStatus.Count;

            if (unoObs.IsReversed)
            {
                if (myIndex - 1 < 0) return numPlayers - 1;
                else return myIndex - 1;
            } else {
                if (myIndex + 1 >= numPlayers) return 0;
                else return myIndex + 1;
            }
        }

        private float EvaluateChangeColor(UnoObservation actualObs, UnoObservation simulatedObs)
        {
            UnoColor myBestColor = GetMyBestColor(actualObs);
            UnoColor simulatedTopCardColor = simulatedObs.TopCard.Color;
            
            return myBestColor == simulatedTopCardColor ? 30f : 1f;
        }

        private UnoColor GetMyBestColor(UnoObservation observation)
        {
            int [] numColorsCard = new int[4]; // 4 colores diferentes sin contar el Wild
            /*
            Los índices de los colores son:
            0 - verde
            1 - azul
            2 - rojo
            3 - amarillo
            */
            foreach (Card cardFromHand in observation.PlayersStatus[observation.GetPlayerTurnIndex()].Hand)
            {
                UnoCard card = (UnoCard)cardFromHand;
                switch (card.Color)
                {
                    case UnoColor.Green:
                        numColorsCard[0]++; // 0 - verde
                        break;
                    case UnoColor.Blue:
                        numColorsCard[1]++; // 1 - azul
                        break;
                    case UnoColor.Red:
                        numColorsCard[2]++; // 2 - rojo
                        break;
                    case UnoColor.Yellow:
                        numColorsCard[3]++; // 3 - amarillo
                        break;
                }
            }
            int indexBestColor = 0;
            int numBestColor = numColorsCard[0];

            // Vemos cuál es el color más repetido
            for (int color = 0;color < numColorsCard.Length;color++)
            {
                if (numBestColor < numColorsCard[color]){
                    indexBestColor = color;
                    numBestColor = numColorsCard[color];
                }
            }

            if (indexBestColor == 0) return UnoColor.Green;
            else if (indexBestColor == 1) return UnoColor.Blue;
            else if (indexBestColor == 2) return UnoColor.Red;
            else return UnoColor.Yellow;
        }

        private float EvaluatePositionalAdvantage(UnoObservation observation)
        {
            // para ganar la partida
            return observation.IsTerminal() ? 100f : 0f;
        }
    }

    public class MyVirusHeuristic : IHeuristic
    {
        // he usado bastante código del ejemplo (^^')

        // Constantes para SNOM :)
        private const float MyOrganBaseValue = 3f;
        private const float MyOrganImmuneValue = 4f;
        private const float MyOrganVaccinatedValue = 2f;
        private const float MyOrganNormalValue = 1f;
        private const float MyOrganVirusValue = -2f;

        // Constantes para los malos :(
        private const float RivalOrganBaseValue = -4f;
        private const float RivalOrganImmuneValue = -2.5f;
        private const float RivalOrganVaccinatedValue = -1f;
        private const float RivalOrganNormalValue = -1.5f;
        private const float RivalOrganVirusValue = 3f;


        public float Evaluate(IObservation observation)
        {
            VirusObservation virusObs = (VirusObservation)observation;
            int playerIndex = virusObs.PlayerIndexPerspective;

            float myScore = EvaluateMyPlayer(virusObs.PlayersStatus[playerIndex].Body);
            float bestOpponentScore = float.MinValue;

            // Evaluar a cada oponente
            for (int i = 0; i < virusObs.PlayersStatus.Count; i++)
            {
                if (i == playerIndex) continue; // Saltar al jugador actual

                float opponentScore = EvaluateRivalPlayer(virusObs.PlayersStatus[i].Body);
                if (opponentScore > bestOpponentScore)
                {
                    bestOpponentScore = opponentScore;
                }
            }

            // Queremos maximizar nuestra diferencia con el mejor rival
            return myScore - bestOpponentScore;
        }
        
        private float EvaluateMyPlayer(List<VirusOrgan> body)
        {
            float score = 0;
            
            if (body.Count(organ => organ.Status is Status.Normal or Status.Immune or Status.Vaccinated) >= 4) return float.PositiveInfinity;
            
            foreach (VirusOrgan organ in body)
            {
                switch (organ.Status)
                {
                    case Status.Normal:
                        score += MyOrganNormalValue;
                        break;
                    case Status.Infected:
                        score += MyOrganVirusValue;
                        break;
                    case Status.Vaccinated:
                        score += MyOrganVaccinatedValue;
                        break;
                    case Status.Immune:
                        score += MyOrganImmuneValue;
                        break;
                }
            }

            score += body.Count * MyOrganBaseValue;
            return score;
        }

        private float EvaluateRivalPlayer(List<VirusOrgan> body)
        {
            float score = 0;
            foreach (VirusOrgan organ in body)
            {
                switch (organ.Status)
                {
                    case Status.Normal:
                        score += RivalOrganNormalValue;
                        break;
                    case Status.Infected:
                        score += RivalOrganVirusValue;
                        break;
                    case Status.Vaccinated:
                        score += RivalOrganVaccinatedValue;
                        break;
                    case Status.Immune:
                        score += RivalOrganImmuneValue;
                        break;
                }
            }

            score += body.Count * RivalOrganBaseValue;
            return score;
        }
    }
}