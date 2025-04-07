using System;
using System.Collections.Generic;
using System.Linq;

public class VirusSimpleHeuristic : IHeuristic
{
    private const float OrganCompleteValue = 0.5f;
    private const float OrganValue = 0.2f;
    private const float MedicineValue = 0.3f;
    private const float VirusValue = 0.1f;

    public float Evaluate(IObservation observation)
    {
        VirusObservation virusObs = (VirusObservation)observation;
        int playerIndex = virusObs.PlayerIndexPerspective;

        float myScore = EvaluatePlayer(virusObs.PlayersStatus[playerIndex].Body);
        float bestOpponentScore = float.MinValue;

        // Evaluar a cada oponente
        for (int i = 0; i < virusObs.PlayersStatus.Count; i++)
        {
            if (i == playerIndex) continue; // Saltar al jugador actual

            float opponentScore = EvaluatePlayer(virusObs.PlayersStatus[i].Body);
            if (opponentScore > bestOpponentScore)
            {
                bestOpponentScore = opponentScore;
            }
        }

        // Queremos maximizar nuestra diferencia con el mejor rival
        return myScore /*- bestOpponentScore*/;
    }
    
    private float EvaluatePlayer(List<VirusOrgan> body)
    {
        float score = 0;
        
        if (body.Count(organ => organ.Status is Status.Normal or Status.Immune or Status.Vaccinated) >= 4) return float.PositiveInfinity;
        
        foreach (VirusOrgan organ in body)
        {
            switch (organ.Status)
            {
                case Status.Normal:
                    score += OrganValue;
                    break;
                case Status.Infected:
                    score += VirusValue;
                    break;
                case Status.Vaccinated:
                    score += MedicineValue;
                    break;
                case Status.Immune:
                    score += OrganCompleteValue;
                    break;
            }
        }

        // Normalizar por el número de órganos (suponiendo que cada jugador tiene el mismo número máximo)
        return score / (body.Count * 2f);
    }
}