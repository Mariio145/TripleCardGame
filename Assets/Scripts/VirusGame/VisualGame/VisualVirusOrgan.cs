using System;
using UnityEngine;

public class VisualVirusOrgan : MonoBehaviour
{
    private VirusHumanPlayer _virusHumanPlayer;
    public VirusColor color;
    public int playerIndex;
    private void OnMouseDown()
    {
        _virusHumanPlayer.SetColorAndPlayer((int)color, playerIndex);
    }
}
