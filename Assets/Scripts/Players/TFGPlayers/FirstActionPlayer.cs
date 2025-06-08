using System.Threading.Tasks;
public class FirstActionPlayer : Player
{
    public override IAction Think(IObservation observation, float timeToThink)
    {
        return observation.GetActions()[0];
    }
}
