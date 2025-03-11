public class UnoSimpleHeuristic: IHeuristic
{
    public float Evaluate(IObservation observation)
    {
        float score = 0;
        UnoObservation unoObservation = (UnoObservation)observation;
        
        // Factores positivos
        score += EvaluateHandSize(unoObservation);
        score += EvaluateCardTypes(unoObservation);
        score += EvaluatePositionalAdvantage(unoObservation);
        
        return score;
    }

    private float EvaluateHandSize(UnoObservation observation)
    {
        int currentPlayerHandSize = observation.PlayersStatus[observation.GetPlayerTurnIndex()].Hand.Count;
        // Menos cartas es mejor
        return 50f / (currentPlayerHandSize + 1);
    }

    private float EvaluateCardTypes(UnoObservation observation)
    {
        float score = 0;
        Card[] hand = observation.PlayersStatus[observation.GetPlayerTurnIndex()].Hand.ToArray();
        
        foreach (Card cardFromHand in hand)
        {
            UnoCard card = (UnoCard)cardFromHand;
            // Asignar valores a diferentes tipos de cartas
            switch (card.Type)
            {
                case UnoType.Draw4:
                    score += 5f; // Carta más poderosa del juego
                    break;
                case UnoType.Draw2:
                    score += 3f; // Fuerza al siguiente a robar cartas
                    break;
                case UnoType.Block:
                    score += 2f; // Salta el turno del siguiente jugador
                    break;
                case UnoType.Reverse:
                    score += 1f; // Cambia la dirección del juego
                    break;
                case UnoType.Change:
                    score += 1f; // Permite cambiar el color
                    break;
                case UnoType.Number:
                    score += 1f; // Cartas numéricas básicas
                    break;
            }
        }
        
        return score;
    }

    private float EvaluatePositionalAdvantage(UnoObservation observation)
    {
        return observation.IsTerminal() ? 50f : 10f;
    }
}