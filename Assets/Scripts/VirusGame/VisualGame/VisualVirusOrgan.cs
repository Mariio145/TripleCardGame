using System;
using UnityEngine;

public class VisualVirusOrgan : MonoBehaviour
{
    public VirusHumanPlayer virusHumanPlayer;
    public VirusColor color;
    public int playerIndex;
    [SerializeField] private MeshCollider meshCollider;
    
    public void Activate()
    {
        meshCollider.enabled = true;
        enabled = true;
    }
    
    public void Deactivate()
    {
        meshCollider.enabled = false;
        enabled = false;
    }
    private void OnMouseDown()
    {
        virusHumanPlayer.SetColorAndPlayer((int)color, playerIndex);
    }
}
