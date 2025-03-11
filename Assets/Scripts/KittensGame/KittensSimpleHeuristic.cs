using System.Linq;

public class KittensSimpleHeuristic: IHeuristic
{
    private const float ExplodingValue = -1;
    private const float DefuseValue = 1;
    private const float RegularValue = -0.01f;
    private const float SeeFutureValue = -0.3f;
    private const float NopeValue = -0.5f;
    private const float AttackValue = -0.4f;
    private const float SkipValue = 0.2f;
    private const float FavorValue = -0.1f;
    private const float ShuffleValue = -0.2f;

    public float Evaluate(IObservation observation)
     {
         KittensObservation kittensObs = (KittensObservation)observation;

         if (!kittensObs.PlayersStatus[kittensObs.PlayerIndexPerspective].IsAlive())
             return -1;
         if (kittensObs.PlayersStatus.Count(p => p.IsAlive()) == 1 && kittensObs.PlayersStatus[kittensObs.PlayerIndexPerspective].IsAlive())
             return 1;

         float cardValues = kittensObs.PlayersStatus[kittensObs.PlayerIndexPerspective].Hand.Cast<KittensCard>().Sum(card => GetCardValue(kittensObs, card));

         return cardValues / (kittensObs.PlayersStatus[kittensObs.PlayerIndexPerspective].Hand.Count + 1);
     }

     private float GetCardValue(KittensObservation kittensObs, KittensCard card)
     {
         switch (card.Type)
         {
             case KittensType.Exploding:
                 return ExplodingValue;
             case KittensType.Defuse:
                 return DefuseValue;
             case KittensType.Nope:
                 if (kittensObs.ActionIsPlaying)
                     return NopeValue;
                 return 0;
             case KittensType.Attack:
                 return AttackValue;
             case KittensType.Skip:
                 return SkipValue;
             case KittensType.Favor:
                 return FavorValue;
             case KittensType.Shuffle:
                 return ShuffleValue;
             case KittensType.SeeTheFuture:
                 return SeeFutureValue;
             default:
                 return RegularValue;
         }
     }
}