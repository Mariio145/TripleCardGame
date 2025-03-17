using UnityEngine;

public class VisualVirusOrgan : MonoBehaviour
{
    public VirusHumanPlayer virusHumanPlayer;
    public VirusColor color;
    public int playerIndex;

    public void Activate()
    {
        enabled = true;
    }
    
    public void Deactivate()
    {
        enabled = false;
    }
    private void OnMouseDown()
    {
        virusHumanPlayer.SetColorAndPlayer((int)color, playerIndex);
    }
}
