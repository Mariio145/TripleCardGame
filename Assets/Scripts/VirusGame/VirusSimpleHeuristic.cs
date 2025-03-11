using System.Collections.Generic;

public class VirusSimpleHeuristic : IHeuristic
{
    private const float OrganCompleteValue = 0.6f;
    private const float OrganValue = 0.2f;
    private const float MedicineValue = 0.4f;
    private const float VirusValue = -0.3f;

    public float Evaluate(IObservation observation)
    {
        VirusObservation virusObs = (VirusObservation)observation;
        float score = 0;

        // Evaluar el estado del cuerpo del jugador actual
        List<VirusOrgan> playerBody = virusObs.PlayersStatus[virusObs.PlayerIndexPerspective].Body;
        foreach (VirusOrgan organ in playerBody)
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

        // Normalizar el score
        int totalOrgans = playerBody.Count;
        return score / (totalOrgans * 2f);
    }
}